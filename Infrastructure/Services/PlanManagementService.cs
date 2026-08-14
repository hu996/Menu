using System.Text.RegularExpressions;
using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;
using RestaurantMenuPlatform.Application.DTOs;
using RestaurantMenuPlatform.Application.Interfaces;
using RestaurantMenuPlatform.Domain.Constants;
using RestaurantMenuPlatform.Domain.Entities;
using RestaurantMenuPlatform.Infrastructure.Persistence;

namespace RestaurantMenuPlatform.Infrastructure.Services;

public sealed class PlanManagementService : IPlanManagementService
{
    private readonly AppDbContext _db;
    private readonly ILookupService _lookupService;

    public PlanManagementService(AppDbContext db, ILookupService lookupService)
    {
        _db = db;
        _lookupService = lookupService;
    }

    public async Task<IReadOnlyList<PlanDto>> GetAllAsync(CancellationToken cancellationToken = default) =>
        await _db.Plans
            .AsNoTracking()
            .Include(x => x.Features)
            .OrderBy(x => x.MonthlyPrice)
            .ThenBy(x => x.Name)
            .Select(ToProjection())
            .ToListAsync(cancellationToken);

    public async Task<PlanDto?> GetAsync(Guid id, CancellationToken cancellationToken = default) =>
        await _db.Plans
            .AsNoTracking()
            .Include(x => x.Features)
            .Where(x => x.Id == id)
            .Select(ToProjection())
            .SingleOrDefaultAsync(cancellationToken);

    public async Task<PlanDto> CreateAsync(PlanManagementInput input, CancellationToken cancellationToken = default)
    {
        var normalized = await Normalize(input, cancellationToken);
        var plan = new Plan
        {
            Name = normalized.Name,
            MonthlyPrice = normalized.MonthlyPrice,
            Currency = normalized.Currency,
            MaxBranches = normalized.MaxBranches,
            MaxMenuItems = normalized.MaxMenuItems,
            MaxUsers = normalized.MaxUsers,
            AdvancedAnalytics = normalized.AdvancedAnalytics,
            CustomBranding = normalized.CustomBranding,
            IsActive = normalized.IsActive
        };
        AddFeatures(plan, normalized.Features);
        _db.Plans.Add(plan);
        await _db.SaveChangesAsync(cancellationToken);
        return (await GetAsync(plan.Id, cancellationToken))!;
    }

    public async Task<PlanDto?> UpdateAsync(Guid id, PlanManagementInput input, CancellationToken cancellationToken = default)
    {
        var plan = await _db.Plans.Include(x => x.Features).SingleOrDefaultAsync(x => x.Id == id, cancellationToken);
        if (plan is null)
            return null;

        var normalized = await Normalize(input, cancellationToken);
        plan.Name = normalized.Name;
        plan.MonthlyPrice = normalized.MonthlyPrice;
        plan.Currency = normalized.Currency;
        plan.MaxBranches = normalized.MaxBranches;
        plan.MaxMenuItems = normalized.MaxMenuItems;
        plan.MaxUsers = normalized.MaxUsers;
        plan.AdvancedAnalytics = normalized.AdvancedAnalytics;
        plan.CustomBranding = normalized.CustomBranding;
        plan.IsActive = normalized.IsActive;
        plan.UpdatedAtUtc = DateTime.UtcNow;
        _db.PlanFeatures.RemoveRange(plan.Features.ToList());
        _db.PlanFeatures.AddRange(normalized.Features.Select(feature => new PlanFeature
        {
            PlanId = plan.Id,
            FeatureKey = feature.FeatureKey,
            Enabled = feature.Enabled,
            LimitValue = feature.LimitValue
        }));
        await _db.SaveChangesAsync(cancellationToken);
        return await GetAsync(plan.Id, cancellationToken);
    }

    public async Task<bool> SetActiveAsync(Guid id, bool isActive, CancellationToken cancellationToken = default)
    {
        var plan = await _db.Plans.SingleOrDefaultAsync(x => x.Id == id, cancellationToken);
        if (plan is null)
            return false;
        plan.IsActive = isActive;
        plan.UpdatedAtUtc = DateTime.UtcNow;
        await _db.SaveChangesAsync(cancellationToken);
        return true;
    }

    private static Expression<Func<Plan, PlanDto>> ToProjection() => x => new PlanDto(
        x.Id,
        x.Name,
        x.MonthlyPrice,
        x.Currency,
        x.MaxBranches,
        x.MaxMenuItems,
        x.MaxUsers,
        x.AdvancedAnalytics,
        x.CustomBranding,
        x.IsActive,
        x.Features.OrderBy(f => f.FeatureKey)
            .Select(f => new PlanFeatureDto(f.FeatureKey, f.Enabled, f.LimitValue))
            .ToList());

    private static void AddFeatures(Plan plan, IReadOnlyList<PlanFeatureInput> features)
    {
        foreach (var feature in features)
            plan.Features.Add(new PlanFeature
            {
                PlanId = plan.Id,
                FeatureKey = feature.FeatureKey,
                Enabled = feature.Enabled,
                LimitValue = feature.LimitValue
            });
    }

    private async Task<PlanManagementInput> Normalize(
        PlanManagementInput input,
        CancellationToken cancellationToken)
    {
        var name = input.Name?.Trim() ?? string.Empty;
        var currency = input.Currency?.Trim().ToUpperInvariant() ?? string.Empty;
        if (name.Length is < 2 or > 160)
            throw new ArgumentException("Plan name must be between 2 and 160 characters.");
        if (input.MonthlyPrice < 0)
            throw new ArgumentException("Plan price cannot be negative.");
        if (!Regex.IsMatch(currency, "^[A-Z]{3}$"))
            throw new ArgumentException("Plan currency must be a three-letter code.");
        if (!await _lookupService.IsActiveAsync(LookupTypes.Currency, currency, cancellationToken))
            throw new ArgumentException("Plan currency must be selected from the active global currency catalog.");
        if (input.MaxBranches < 0 || input.MaxMenuItems < 0 || input.MaxUsers < 0)
            throw new ArgumentException("Plan limits cannot be negative. Use zero for unlimited.");

        var features = input.Features
            .Select(x => new PlanFeatureInput(x.FeatureKey?.Trim() ?? string.Empty, x.Enabled, x.LimitValue))
            .Where(x => !string.IsNullOrWhiteSpace(x.FeatureKey))
            .ToList();
        if (features.Any(x => x.FeatureKey.Length > 120 || x.LimitValue < 0))
            throw new ArgumentException("Feature keys must be 120 characters or fewer and feature limits cannot be negative.");
        if (features.GroupBy(x => x.FeatureKey, StringComparer.OrdinalIgnoreCase).Any(x => x.Count() > 1))
            throw new ArgumentException("Each feature key may appear only once per plan.");

        return input with { Name = name, Currency = currency, Features = features };
    }
}
