using Microsoft.EntityFrameworkCore;
using RestaurantMenuPlatform.Application.DTOs;
using RestaurantMenuPlatform.Application.Interfaces;
using RestaurantMenuPlatform.Domain.Entities;
using RestaurantMenuPlatform.Domain.Enums;
using RestaurantMenuPlatform.Domain.Interfaces;
using RestaurantMenuPlatform.Infrastructure.Persistence;

namespace RestaurantMenuPlatform.Infrastructure.Services;

public sealed class OrderService : IOrderService
{
    private readonly AppDbContext _db;
    private readonly ITenantContext _tenantContext;
    private readonly IAuditLogService _audit;

    public OrderService(AppDbContext db, ITenantContext tenantContext, IAuditLogService audit)
    {
        _db = db;
        _tenantContext = tenantContext;
        _audit = audit;
    }

    public async Task<PublicOrderItemDto?> GetPublicItemAsync(Guid branchId, Guid itemId, string? language = null, CancellationToken cancellationToken = default)
    {
        var item = await QueryPublicItems(branchId, new[] { itemId })
            .SingleOrDefaultAsync(cancellationToken);
        if (item is null)
            return null;

        var overrideEntity = await _db.BranchMenuItemOverrides.AsNoTracking()
            .SingleOrDefaultAsync(x => x.BranchId == branchId && x.MenuItemId == itemId, cancellationToken);
        if (overrideEntity?.IsVisibleOverride == false)
            return null;

        return ToPublicItem(item, overrideEntity, language);
    }

    public async Task<CartDto?> RecalculateCartAsync(
        Guid branchId,
        string restaurantSlug,
        string branchSlug,
        IReadOnlyList<CartLineInput> lines,
        CancellationToken cancellationToken = default,
        PublicOrderingContextDto? publicContext = null)
    {
        if (publicContext is not null)
            await ValidatePublicContextAsync(branchId, publicContext.TableId, publicContext.QrCodeId, publicContext.QrCodeCode, cancellationToken);
        if (lines.Count == 0)
            return new CartDto(restaurantSlug, branchSlug, branchId, [], 0, string.Empty, publicContext?.TableId, publicContext?.TableName, publicContext?.TableNameAr, publicContext?.QrCodeId, publicContext?.QrCodeCode);

        var validated = await ValidateLinesAsync(branchId, lines, cancellationToken);
        var currency = validated.First().Currency;
        if (validated.Any(x => !string.Equals(x.Currency, currency, StringComparison.OrdinalIgnoreCase)))
            throw new ArgumentException("All items in a cart must use the same currency.");

        return new CartDto(
            restaurantSlug,
            branchSlug,
            branchId,
            validated.Select(ToCartLine).ToList(),
            validated.Sum(x => x.LineTotal),
            currency,
            publicContext?.TableId,
            publicContext?.TableName,
            publicContext?.TableNameAr,
            publicContext?.QrCodeId,
            publicContext?.QrCodeCode);
    }

    public async Task<OrderReceiptDto?> CreateAsync(CheckoutInput input, CancellationToken cancellationToken = default)
    {
        if (!_tenantContext.TenantId.HasValue)
            throw new InvalidOperationException("A tenant context is required to create an order.");
        if (string.IsNullOrWhiteSpace(input.IdempotencyKey) || input.IdempotencyKey.Length > 120)
            throw new ArgumentException("A valid checkout request key is required.");
        if (string.IsNullOrWhiteSpace(input.CustomerName) || input.CustomerName.Trim().Length > 160)
            throw new ArgumentException("Customer name is required.");
        if (string.IsNullOrWhiteSpace(input.CustomerPhone) || input.CustomerPhone.Trim().Length > 40)
            throw new ArgumentException("Customer phone is required.");

        await ValidatePublicContextAsync(input.BranchId, input.TableId, input.QrCodeId, input.QrCodeCode, cancellationToken);

        var existing = await _db.Orders.AsNoTracking().Include(x => x.Branch).ThenInclude(x => x.Tenant).Include(x => x.Table).Include(x => x.QrCode).Include(x => x.Items).ThenInclude(x => x.Modifiers).Include(x => x.Items).ThenInclude(x => x.MenuItem)
            .SingleOrDefaultAsync(x => x.IdempotencyKey == input.IdempotencyKey.Trim(), cancellationToken);
        if (existing is not null && existing.BranchId != input.BranchId)
            throw new ArgumentException("This checkout request key belongs to another branch.");
        if (existing is not null)
            return ToReceipt(existing);

        var validated = await ValidateLinesAsync(input.BranchId, input.Lines, cancellationToken);
        if (validated.Count == 0)
            throw new ArgumentException("Add at least one available product before placing the order.");
        if (input.MenuId.HasValue && validated.Any(x => x.MenuId != input.MenuId.Value))
            throw new ArgumentException("Cart items must remain within the selected menu context.");

        var currency = validated.First().Currency;
        if (validated.Any(x => !string.Equals(x.Currency, currency, StringComparison.OrdinalIgnoreCase)))
            throw new ArgumentException("All items in an order must use the same currency.");

        Order? createdOrder = null;
        Order? idempotentOrder = null;
        var strategy = _db.Database.CreateExecutionStrategy();
        await strategy.ExecuteAsync(async () =>
        {
            await using var transaction = await _db.Database.BeginTransactionAsync(cancellationToken);
            idempotentOrder = await _db.Orders.Include(x => x.Branch).ThenInclude(x => x.Tenant).Include(x => x.Table).Include(x => x.QrCode).Include(x => x.Items).ThenInclude(x => x.Modifiers).Include(x => x.Items).ThenInclude(x => x.MenuItem)
                .SingleOrDefaultAsync(x => x.IdempotencyKey == input.IdempotencyKey.Trim(), cancellationToken);
            if (idempotentOrder is not null && idempotentOrder.BranchId != input.BranchId)
                throw new ArgumentException("This checkout request key belongs to another branch.");
            if (idempotentOrder is not null)
                return;

            createdOrder = new Order
            {
                TenantId = _tenantContext.TenantId.Value,
                BranchId = input.BranchId,
                TableId = input.TableId,
                QrCodeId = input.QrCodeId,
                MenuId = input.MenuId ?? validated.First().MenuId,
                OrderNumber = await NextOrderNumberAsync(cancellationToken),
                IdempotencyKey = input.IdempotencyKey.Trim(),
                CustomerName = input.CustomerName.Trim(),
                CustomerPhone = input.CustomerPhone.Trim(),
                Notes = string.IsNullOrWhiteSpace(input.Notes) ? null : input.Notes.Trim(),
                Total = validated.Sum(x => x.LineTotal),
                Currency = currency,
                Status = OrderStatus.Pending
            };
            foreach (var line in validated)
            {
                var orderItem = new OrderItem
                {
                    TenantId = createdOrder.TenantId,
                    OrderId = createdOrder.Id,
                    MenuItemId = line.MenuItemId,
                    ProductName = line.ProductName,
                    UnitPrice = line.UnitPrice,
                    Quantity = line.Quantity,
                    LineTotal = line.LineTotal
                };
                foreach (var option in line.Options)
                {
                    orderItem.Modifiers.Add(new OrderItemModifier
                    {
                        TenantId = createdOrder.TenantId,
                        OrderItemId = orderItem.Id,
                        ModifierOptionId = option.Id,
                        OptionName = option.Name,
                        PriceAdjustment = option.PriceAdjustment
                    });
                }
                createdOrder.Items.Add(orderItem);
            }
            createdOrder.StatusHistory.Add(new OrderStatusHistory
            {
                TenantId = createdOrder.TenantId,
                OrderId = createdOrder.Id,
                ToStatus = OrderStatus.Pending,
                ActorDisplayName = "Customer"
            });
            _db.Orders.Add(createdOrder);
            await _db.SaveChangesAsync(cancellationToken);
            await transaction.CommitAsync(cancellationToken);
        });

        var order = idempotentOrder ?? createdOrder ?? throw new InvalidOperationException("Order creation did not produce an order.");
        if (createdOrder is null)
            return ToReceipt(order);
        await _db.Entry(order).Reference(x => x.Branch).LoadAsync(cancellationToken);
        await _db.Entry(order.Branch).Reference(x => x.Tenant).LoadAsync(cancellationToken);
        await _db.Entry(order).Reference(x => x.Table).LoadAsync(cancellationToken);
        await _db.Entry(order).Reference(x => x.QrCode).LoadAsync(cancellationToken);
        foreach (var item in order.Items)
            await _db.Entry(item).Reference(x => x.MenuItem).LoadAsync(cancellationToken);
        await _audit.WriteAsync("order.created", "Order", order.Id, null, new { order.OrderNumber, order.Total, order.Status }, cancellationToken);
        return ToReceipt(order);
    }

    public async Task<OrderReceiptDto?> GetPublicOrderAsync(string orderNumber, Guid branchId, CancellationToken cancellationToken = default)
    {
        var order = await _db.Orders.AsNoTracking().Include(x => x.Branch).ThenInclude(x => x.Tenant).Include(x => x.Table).Include(x => x.QrCode).Include(x => x.Items).ThenInclude(x => x.Modifiers).Include(x => x.Items).ThenInclude(x => x.MenuItem)
            .SingleOrDefaultAsync(x => x.OrderNumber == orderNumber && x.BranchId == branchId, cancellationToken);
        return order is null ? null : ToReceipt(order);
    }

    public async Task<IReadOnlyList<StaffOrderDto>> GetStaffOrdersAsync(
        Guid? branchScopeId,
        Guid? branchId = null,
        Guid? tableId = null,
        string? status = null,
        string? search = null,
        DateTime? dateFrom = null,
        DateTime? dateTo = null,
        CancellationToken cancellationToken = default)
    {
        var query = StaffQuery();
        if (branchScopeId.HasValue)
            query = query.Where(x => x.BranchId == branchScopeId.Value);
        if (branchId.HasValue)
            query = query.Where(x => x.BranchId == branchId.Value);
        if (tableId.HasValue)
            query = query.Where(x => x.TableId == tableId.Value);
        if (!string.IsNullOrWhiteSpace(status) && Enum.TryParse<OrderStatus>(status, true, out var parsedStatus))
            query = query.Where(x => x.Status == parsedStatus);
        if (!string.IsNullOrWhiteSpace(search))
        {
            var normalizedSearch = search.Trim();
            query = query.Where(x => x.OrderNumber.Contains(normalizedSearch) || x.CustomerName.Contains(normalizedSearch) || x.CustomerPhone.Contains(normalizedSearch));
        }
        if (dateFrom.HasValue)
            query = query.Where(x => x.CreatedAtUtc >= dateFrom.Value);
        if (dateTo.HasValue)
            query = query.Where(x => x.CreatedAtUtc < dateTo.Value);
        var orders = await query.OrderBy(x => x.Status == OrderStatus.Pending ? 0 : 1).ThenByDescending(x => x.CreatedAtUtc)
            .Take(100).ToListAsync(cancellationToken);
        return orders.Select(ToStaff).ToList();
    }

    public async Task<StaffOrderDto?> GetStaffOrderAsync(Guid id, Guid? branchId, CancellationToken cancellationToken = default)
    {
        var query = StaffQuery().Where(x => x.Id == id);
        if (branchId.HasValue)
            query = query.Where(x => x.BranchId == branchId.Value);
        var order = await query.SingleOrDefaultAsync(cancellationToken);
        return order is null ? null : ToStaff(order);
    }

    public async Task<StaffOrderDto?> TransitionAsync(
        Guid id,
        OrderStatus targetStatus,
        Guid? branchId,
        Guid? actorUserId,
        string? actorDisplayName,
        CancellationToken cancellationToken = default)
    {
        var query = _db.Orders.AsNoTracking().Include(x => x.Branch).Include(x => x.Table).Include(x => x.QrCode).Include(x => x.Items).ThenInclude(x => x.Modifiers).Include(x => x.Items).ThenInclude(x => x.MenuItem).Where(x => x.Id == id);
        if (branchId.HasValue)
            query = query.Where(x => x.BranchId == branchId.Value);
        var order = await query.SingleOrDefaultAsync(cancellationToken);
        if (order is null)
            return null;
        if (!IsValidTransition(order.Status, targetStatus))
            throw new ArgumentException($"Order {order.OrderNumber} cannot move from {order.Status} to {targetStatus}.");

        var previous = order.Status;
        DateTime? completedAt = targetStatus == OrderStatus.Completed ? DateTime.UtcNow : null;
        var strategy = _db.Database.CreateExecutionStrategy();
        await strategy.ExecuteAsync(async () =>
        {
            await using var transaction = await _db.Database.BeginTransactionAsync(cancellationToken);
            var updated = await _db.Orders
                .Where(x => x.Id == id && x.Status == previous)
                .ExecuteUpdateAsync(setters => setters
                    .SetProperty(x => x.Status, targetStatus)
                    .SetProperty(x => x.CompletedAtUtc, completedAt), cancellationToken);
            if (updated != 1)
                throw new InvalidOperationException("The order changed before this transition could be saved.");

            _db.OrderStatusHistories.Add(new OrderStatusHistory
            {
                TenantId = order.TenantId,
                OrderId = order.Id,
                FromStatus = previous,
                ToStatus = targetStatus,
                ActorUserId = actorUserId,
                ActorDisplayName = actorDisplayName
            });
            await _db.SaveChangesAsync(cancellationToken);
            await transaction.CommitAsync(cancellationToken);
        });
        order.Status = targetStatus;
        order.CompletedAtUtc = completedAt;
        await _audit.WriteAsync("order.status.changed", "Order", order.Id, new { Status = previous }, new { Status = targetStatus, order.OrderNumber }, cancellationToken);
        return ToStaff(order);
    }

    private IQueryable<MenuItem> QueryPublicItems(Guid branchId, IReadOnlyCollection<Guid> itemIds) =>
        _db.MenuItems.AsNoTracking()
            .Include(x => x.MenuCategory).ThenInclude(x => x.Menu).ThenInclude(x => x.Tenant)
            .Include(x => x.MenuCategory).ThenInclude(x => x.Menu).ThenInclude(x => x.BranchMenus)
            .Include(x => x.Images)
            .Include(x => x.Ingredients).ThenInclude(x => x.Ingredient)
            .Include(x => x.Allergens).ThenInclude(x => x.Allergen)
            .Include(x => x.Modifiers).ThenInclude(x => x.Modifier).ThenInclude(x => x.Options)
            .Where(x => itemIds.Contains(x.Id) && x.MenuCategory.IsActive && x.MenuCategory.Menu.Status == MenuStatus.Published &&
                        x.MenuCategory.Menu.BranchMenus.Any(b => b.BranchId == branchId && b.IsActive));

    private async Task<List<ValidatedLine>> ValidateLinesAsync(Guid branchId, IReadOnlyList<CartLineInput> lines, CancellationToken cancellationToken)
    {
        if (!await _db.Branches.AsNoTracking().AnyAsync(x => x.Id == branchId && x.IsActive, cancellationToken))
            throw new ArgumentException("The selected branch is unavailable.");
        var ids = lines.Select(x => x.MenuItemId).Distinct().ToArray();
        var items = await QueryPublicItems(branchId, ids).ToListAsync(cancellationToken);
        if (items.Count != ids.Length)
            throw new ArgumentException("One or more products are no longer available in this branch menu.");
        var overrides = await _db.BranchMenuItemOverrides.AsNoTracking().Where(x => x.BranchId == branchId && ids.Contains(x.MenuItemId)).ToDictionaryAsync(x => x.MenuItemId, cancellationToken);
        var output = new List<ValidatedLine>();
        foreach (var input in lines)
        {
            if (input.Quantity is < 1 or > 20)
                throw new ArgumentException("Quantity must be between 1 and 20.");
            var item = items.Single(x => x.Id == input.MenuItemId);
            var overrideEntity = overrides.GetValueOrDefault(item.Id);
            // A global disable is authoritative. A branch override may make an
            // item less available, but must never resurrect a disabled product.
            if (overrideEntity?.IsVisibleOverride == false || !item.IsAvailable || overrideEntity?.IsAvailableOverride == false)
                throw new ArgumentException($"{item.Name} is currently unavailable.");
            var selectedIds = input.ModifierOptionIds.Distinct().ToArray();
            var options = item.Modifiers.SelectMany(x => x.Modifier.Options).Where(x => x.IsActive && selectedIds.Contains(x.Id)).ToList();
            if (options.Count != selectedIds.Length)
                throw new ArgumentException($"One or more modifier choices for {item.Name} are invalid.");
            foreach (var group in item.Modifiers.Select(x => x.Modifier))
            {
                var count = options.Count(x => x.ModifierId == group.Id);
                if (count < group.MinSelections || count > group.MaxSelections || (group.IsRequired && count == 0))
                    throw new ArgumentException($"Choose between {group.MinSelections} and {group.MaxSelections} options for {group.Name}.");
            }
            var unitPrice = overrideEntity?.PriceOverride ?? item.Price;
            var modifierTotal = options.Sum(x => x.PriceAdjustment);
            output.Add(new ValidatedLine(item, item.MenuCategory.MenuId, input.Quantity, unitPrice, modifierTotal, options));
        }
        return output;
    }

    private async Task ValidatePublicContextAsync(
        Guid branchId,
        Guid? tableId,
        Guid? qrCodeId,
        string? qrCodeCode,
        CancellationToken cancellationToken)
    {
        if (!tableId.HasValue || !qrCodeId.HasValue || string.IsNullOrWhiteSpace(qrCodeCode))
            throw new ArgumentException("Scan an active table QR code before placing an order.");

        var qr = await _db.QrCodes.AsNoTracking()
            .Include(x => x.Branch).ThenInclude(x => x.Tenant)
            .Include(x => x.Table)
            .SingleOrDefaultAsync(x => x.Id == qrCodeId.Value && x.Code == qrCodeCode.Trim() &&
                                       x.TableId == tableId.Value && x.BranchId == branchId && x.TargetType == "branch-menu" && x.IsActive,
                cancellationToken);
        if (qr?.Table is null || !qr.Table.IsActive || !qr.Branch.IsActive || !qr.Branch.Tenant.IsActive || qr.Table.BranchId != branchId)
            throw new ArgumentException("This table QR code is no longer active. Scan the current QR code and try again.");
    }

    private async Task<string> NextOrderNumberAsync(CancellationToken cancellationToken)
    {
        for (var attempt = 0; attempt < 5; attempt++)
        {
            var candidate = $"RM-{Random.Shared.Next(100000, 999999)}";
            if (!await _db.Orders.AnyAsync(x => x.OrderNumber == candidate, cancellationToken))
                return candidate;
        }
        return $"RM-{DateTime.UtcNow:MMddHHmmssfff}";
    }

    private IQueryable<Order> StaffQuery() => _db.Orders.AsNoTracking().Include(x => x.Branch).Include(x => x.Table).Include(x => x.QrCode).Include(x => x.Items).ThenInclude(x => x.Modifiers);

    private static bool IsValidTransition(OrderStatus from, OrderStatus to) =>
        (from, to) is (OrderStatus.Pending, OrderStatus.Accepted) or
        (OrderStatus.Pending, OrderStatus.Rejected) or
        (OrderStatus.Accepted, OrderStatus.Preparing) or
        (OrderStatus.Preparing, OrderStatus.Ready) or
        (OrderStatus.Ready, OrderStatus.Completed);

    private static PublicOrderItemDto ToPublicItem(MenuItem item, BranchMenuItemOverride? overrideEntity, string? language) =>
        new(item.Id,
            item.MenuCategory.MenuId,
            Localized(item.Name, item.NameEn, item.NameAr, language) ?? string.Empty,
            overrideEntity?.PriceOverride ?? item.Price,
            item.Currency,
            item.IsAvailable && (overrideEntity?.IsAvailableOverride ?? true),
            item.Modifiers.Where(x => x.Modifier.IsActive).OrderBy(x => x.SortOrder).Select(x => new PublicModifierDto(
                Localized(x.Modifier.Name, x.Modifier.NameEn, x.Modifier.NameAr, language) ?? x.Modifier.Name,
                x.Modifier.IsRequired,
                x.Modifier.Options.Where(o => o.IsActive).OrderBy(o => o.SortOrder).Select(o => new PublicModifierOptionDto(
                    Localized(o.Name, o.NameEn, o.NameAr, language) ?? o.Name,
                    o.PriceAdjustment,
                    o.NameAr,
                    o.Id)).ToList(),
                x.Modifier.NameAr,
                x.Modifier.MinSelections,
                x.Modifier.MaxSelections)).ToList(),
            Localized(item.Description, item.DescriptionEn, item.DescriptionAr, language),
            item.Images.OrderByDescending(x => x.IsPrimary).ThenBy(x => x.SortOrder).Select(x => new PublicMenuImageDto(
                ToPublicImageUrl(x.Url, item.MenuCategory.Menu.TenantId, item.MenuCategory.Menu.Tenant.Slug) ?? x.Url,
                string.IsNullOrWhiteSpace(x.AltText) ? item.Name : x.AltText,
                x.IsPrimary,
                x.SortOrder)).FirstOrDefault()?.Url,
            item.Ingredients.Where(x => x.Ingredient.IsActive).Select(x => Localized(x.Ingredient.Name, x.Ingredient.NameEn, x.Ingredient.NameAr, language) ?? x.Ingredient.Name).Distinct().ToList(),
            item.Allergens.Where(x => x.Allergen.IsActive).Select(x => Localized(x.Allergen.Name, x.Allergen.NameEn, x.Allergen.NameAr, language) ?? x.Allergen.Name).Distinct().ToList(),
            item.Images.OrderByDescending(x => x.IsPrimary).ThenBy(x => x.SortOrder).Select(x => new PublicMenuImageDto(
                ToPublicImageUrl(x.Url, item.MenuCategory.Menu.TenantId, item.MenuCategory.Menu.Tenant.Slug) ?? x.Url,
                string.IsNullOrWhiteSpace(x.AltText) ? item.Name : x.AltText,
                x.IsPrimary,
                x.SortOrder)).ToList());

    private static string? ToPublicImageUrl(string? url, Guid tenantId, string tenantSlug) =>
        string.IsNullOrWhiteSpace(url)
            ? null
            : url.StartsWith($"/media/{tenantId:D}/", StringComparison.OrdinalIgnoreCase)
                ? $"/media/{tenantSlug}/menu-items/{Uri.EscapeDataString(Path.GetFileName(url))}"
                : url;

    private static string? Localized(string? fallback, string? english, string? arabic, string? language) =>
        string.Equals(language, "ar", StringComparison.OrdinalIgnoreCase)
            ? arabic ?? english ?? fallback
            : english ?? fallback;

    private static CartLineDto ToCartLine(ValidatedLine line)
    {
        var key = $"{line.MenuItemId:N}:{string.Join(',', line.Options.Select(x => x.Id.ToString("N")).OrderBy(x => x))}";
        return new(key, line.MenuItemId, line.ProductName, line.Quantity, line.UnitPrice, line.ModifierTotal, line.LineTotal, line.Currency,
            line.Options.Select(x => x.Id).ToList(), line.Options.Select(x => x.Name).ToList(),
            line.Item.Images.OrderByDescending(x => x.IsPrimary).ThenBy(x => x.SortOrder)
                .Select(x => ToPublicImageUrl(x.Url, line.Item.MenuCategory.Menu.TenantId, line.Item.MenuCategory.Menu.Tenant.Slug) ?? x.Url)
                .FirstOrDefault());
    }

    private static OrderReceiptDto ToReceipt(Order order) =>
        new(order.Id, order.OrderNumber, order.Branch?.Tenant?.Name ?? string.Empty, order.Branch?.Name ?? string.Empty, order.Total, order.Currency, order.Status.ToString(), order.Items.Select(ToCartLine).ToList(), order.Table?.Name, order.Table?.NameAr, order.QrCode?.Code);

    private static StaffOrderDto ToStaff(Order order) =>
        new(order.Id, order.OrderNumber, order.CustomerName, order.CustomerPhone, order.Total, order.Currency, order.Status.ToString(), order.Branch?.Name ?? string.Empty, order.CreatedAtUtc, order.Items.Select(ToCartLine).ToList(), order.TableId, order.Table?.Name, order.Table?.NameAr, order.QrCodeId, order.QrCode?.Code);

    private static CartLineDto ToCartLine(OrderItem item) =>
        new($"{item.MenuItemId:N}:{string.Join(',', item.Modifiers.Select(x => x.ModifierOptionId.ToString("N")).OrderBy(x => x))}", item.MenuItemId, item.ProductName, item.Quantity, item.UnitPrice,
            item.Modifiers.Sum(x => x.PriceAdjustment), item.LineTotal, item.MenuItem?.Currency ?? string.Empty, item.Modifiers.Select(x => x.ModifierOptionId).ToList(), item.Modifiers.Select(x => x.OptionName).ToList());

    private sealed record ValidatedLine(MenuItem Item, Guid MenuId, int Quantity, decimal UnitPrice, decimal ModifierTotal, IReadOnlyList<ModifierOption> Options)
    {
        public Guid MenuItemId => Item.Id;
        public string ProductName => Item.NameEn ?? Item.Name;
        public string Currency => Item.Currency;
        public decimal LineTotal => (UnitPrice + ModifierTotal) * Quantity;
    }
}
