using Microsoft.EntityFrameworkCore;
using RestaurantMenuPlatform.Domain.Common;
using RestaurantMenuPlatform.Domain.Entities;
using RestaurantMenuPlatform.Domain.Interfaces;

namespace RestaurantMenuPlatform.Infrastructure.Persistence;

public class AppDbContext : DbContext
{
    private readonly ITenantContext _tenantContext;

    public AppDbContext(
        DbContextOptions<AppDbContext> options,
        ITenantContext tenantContext) : base(options)
    {
        _tenantContext = tenantContext;
    }

    public DbSet<Tenant> Tenants => Set<Tenant>();
    public DbSet<Branch> Branches => Set<Branch>();
    public DbSet<Menu> Menus => Set<Menu>();
    public DbSet<BranchMenu> BranchMenus => Set<BranchMenu>();
    public DbSet<MenuCategory> MenuCategories => Set<MenuCategory>();
    public DbSet<MenuItem> MenuItems => Set<MenuItem>();
    public DbSet<MenuItemImage> MenuItemImages => Set<MenuItemImage>();
    public DbSet<TenantBrandingImage> TenantBrandingImages => Set<TenantBrandingImage>();
    public DbSet<Ingredient> Ingredients => Set<Ingredient>();
    public DbSet<MenuItemIngredient> MenuItemIngredients => Set<MenuItemIngredient>();
    public DbSet<Allergen> Allergens => Set<Allergen>();
    public DbSet<MenuItemAllergen> MenuItemAllergens => Set<MenuItemAllergen>();
    public DbSet<QrCode> QrCodes => Set<QrCode>();
    public DbSet<RestaurantTable> RestaurantTables => Set<RestaurantTable>();
    public DbSet<Plan> Plans => Set<Plan>();
    public DbSet<PlanFeature> PlanFeatures => Set<PlanFeature>();
    public DbSet<Subscription> Subscriptions => Set<Subscription>();
    public DbSet<PaymentTransaction> PaymentTransactions => Set<PaymentTransaction>();
    public DbSet<AuditLog> AuditLogs => Set<AuditLog>();
    public DbSet<AnalyticsEvent> AnalyticsEvents => Set<AnalyticsEvent>();
    public DbSet<User> Users => Set<User>();
    public DbSet<PasswordResetToken> PasswordResetTokens => Set<PasswordResetToken>();
    public DbSet<Membership> Memberships => Set<Membership>();
    public DbSet<PermissionDefinition> PermissionDefinitions => Set<PermissionDefinition>();
    public DbSet<RolePermission> RolePermissions => Set<RolePermission>();
    public DbSet<UserPermission> UserPermissions => Set<UserPermission>();
    public DbSet<BranchMenuItemOverride> BranchMenuItemOverrides => Set<BranchMenuItemOverride>();
    public DbSet<BranchSpecificMenuItem> BranchSpecificMenuItems => Set<BranchSpecificMenuItem>();
    public DbSet<LookupValue> LookupValues => Set<LookupValue>();
    public DbSet<LookupType> LookupTypes => Set<LookupType>();
    public DbSet<PriceHistory> PriceHistories => Set<PriceHistory>();
    public DbSet<Modifier> Modifiers => Set<Modifier>();
    public DbSet<ModifierOption> ModifierOptions => Set<ModifierOption>();
    public DbSet<MenuItemModifier> MenuItemModifiers => Set<MenuItemModifier>();
    public DbSet<Order> Orders => Set<Order>();
    public DbSet<OrderItem> OrderItems => Set<OrderItem>();
    public DbSet<OrderItemModifier> OrderItemModifiers => Set<OrderItemModifier>();
    public DbSet<OrderStatusHistory> OrderStatusHistories => Set<OrderStatusHistory>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.ApplyConfigurationsFromAssembly(typeof(AppDbContext).Assembly);

        modelBuilder.Entity<Tenant>().HasIndex(x => x.Slug).IsUnique();
        modelBuilder.Entity<Tenant>().Property(x => x.Name).HasMaxLength(160).IsRequired();
        modelBuilder.Entity<Tenant>().Property(x => x.NameEn).HasMaxLength(160);
        modelBuilder.Entity<Tenant>().Property(x => x.NameAr).HasMaxLength(160);
        modelBuilder.Entity<Tenant>().Property(x => x.Slug).HasMaxLength(120).IsRequired();
        modelBuilder.Entity<Tenant>().Property(x => x.LogoUrl).HasMaxLength(1000);
        modelBuilder.Entity<Tenant>().Property(x => x.CoverImageUrl).HasMaxLength(1000);
        modelBuilder.Entity<Tenant>().Property(x => x.Phone).HasMaxLength(40);
        modelBuilder.Entity<Tenant>().Property(x => x.Email).HasMaxLength(320);
        modelBuilder.Entity<Tenant>().Property(x => x.Address).HasMaxLength(500);
        modelBuilder.Entity<Tenant>().Property(x => x.Currency).HasMaxLength(8);
        modelBuilder.Entity<Tenant>().Property(x => x.DefaultLanguage).HasMaxLength(10).IsRequired();
        modelBuilder.Entity<Tenant>().Property(x => x.IsActive).HasDefaultValue(true);
        modelBuilder.Entity<Tenant>().Property(x => x.BrandPrimaryColor).HasMaxLength(16);
        modelBuilder.Entity<Tenant>().Property(x => x.BrandAccentColor).HasMaxLength(16);
        modelBuilder.Entity<Branch>().HasIndex(x => new { x.TenantId, x.Name });
        modelBuilder.Entity<Branch>().Property(x => x.Name).HasMaxLength(160).IsRequired();
        modelBuilder.Entity<Branch>().Property(x => x.NameEn).HasMaxLength(160);
        modelBuilder.Entity<Branch>().Property(x => x.NameAr).HasMaxLength(160);
        modelBuilder.Entity<Branch>().Property(x => x.Slug).HasMaxLength(120).IsRequired();
        modelBuilder.Entity<Branch>().Property(x => x.Address).HasMaxLength(500);
        modelBuilder.Entity<Branch>().Property(x => x.Phone).HasMaxLength(40);
        modelBuilder.Entity<Branch>().Property(x => x.OpeningHours).HasMaxLength(1000);
        modelBuilder.Entity<Branch>().Property(x => x.Latitude).HasPrecision(9, 6);
        modelBuilder.Entity<Branch>().Property(x => x.Longitude).HasPrecision(9, 6);
        modelBuilder.Entity<Branch>().Property(x => x.BrandPrimaryColorOverride).HasMaxLength(16);
        modelBuilder.Entity<Branch>().Property(x => x.BrandAccentColorOverride).HasMaxLength(16);
        modelBuilder.Entity<Branch>().HasIndex(x => new { x.TenantId, x.Slug }).IsUnique();
        modelBuilder.Entity<Branch>().HasAlternateKey(x => new { x.TenantId, x.Id });
        modelBuilder.Entity<Menu>().HasIndex(x => new { x.TenantId, x.Slug }).IsUnique();
        modelBuilder.Entity<Menu>().HasIndex(x => new { x.TenantId, x.MenuTypeCode, x.ScopeCode });
        modelBuilder.Entity<Menu>().HasAlternateKey(x => new { x.TenantId, x.Id });
        modelBuilder.Entity<Menu>().Property(x => x.Name).HasMaxLength(160).IsRequired();
        modelBuilder.Entity<Menu>().Property(x => x.NameEn).HasMaxLength(160);
        modelBuilder.Entity<Menu>().Property(x => x.NameAr).HasMaxLength(160);
        modelBuilder.Entity<Menu>().Property(x => x.Slug).HasMaxLength(120).IsRequired();
        modelBuilder.Entity<Menu>().Property(x => x.MenuTypeCode).HasMaxLength(64);
        modelBuilder.Entity<Menu>().Property(x => x.ScopeCode).HasMaxLength(64);
        modelBuilder.Entity<Menu>().Property(x => x.Description).HasMaxLength(500);
        modelBuilder.Entity<Menu>().Property(x => x.DescriptionAr).HasMaxLength(500);
        modelBuilder.Entity<Menu>().Property(x => x.BrandPrimaryColor).HasMaxLength(16);
        modelBuilder.Entity<Menu>().Property(x => x.BrandAccentColor).HasMaxLength(16);
        modelBuilder.Entity<Menu>().HasIndex(x => new { x.TenantId, x.SortOrder });
        modelBuilder.Entity<MenuCategory>().Property(x => x.ClassificationCode).HasMaxLength(64);
        modelBuilder.Entity<MenuCategory>().Property(x => x.Name).HasMaxLength(160).IsRequired();
        modelBuilder.Entity<MenuCategory>().Property(x => x.NameEn).HasMaxLength(160);
        modelBuilder.Entity<MenuCategory>().Property(x => x.NameAr).HasMaxLength(160);
        modelBuilder.Entity<MenuCategory>().Property(x => x.Description).HasMaxLength(1000);
        modelBuilder.Entity<MenuCategory>().Property(x => x.DescriptionAr).HasMaxLength(1000);
        modelBuilder.Entity<MenuCategory>().Property(x => x.IsActive).HasDefaultValue(true);
        modelBuilder.Entity<MenuCategory>().HasAlternateKey(x => new { x.TenantId, x.Id });
        modelBuilder.Entity<MenuItem>().Property(x => x.ProductTypeCode).HasMaxLength(64);
        modelBuilder.Entity<MenuItem>().Property(x => x.Name).HasMaxLength(160).IsRequired();
        modelBuilder.Entity<MenuItem>().Property(x => x.NameEn).HasMaxLength(160);
        modelBuilder.Entity<MenuItem>().Property(x => x.NameAr).HasMaxLength(160);
        modelBuilder.Entity<MenuItem>().Property(x => x.Description).HasMaxLength(2000);
        modelBuilder.Entity<MenuItem>().Property(x => x.DescriptionEn).HasMaxLength(2000);
        modelBuilder.Entity<MenuItem>().Property(x => x.DescriptionAr).HasMaxLength(2000);
        modelBuilder.Entity<MenuItem>().Property(x => x.Currency).HasMaxLength(8).IsRequired();
        modelBuilder.Entity<MenuItem>().HasAlternateKey(x => new { x.TenantId, x.Id });
        modelBuilder.Entity<RestaurantTable>().HasAlternateKey(x => new { x.TenantId, x.Id });
        modelBuilder.Entity<RestaurantTable>().HasIndex(x => new { x.TenantId, x.BranchId, x.Name }).IsUnique();
        modelBuilder.Entity<RestaurantTable>().Property(x => x.Name).HasMaxLength(120).IsRequired();
        modelBuilder.Entity<RestaurantTable>().Property(x => x.NameAr).HasMaxLength(120);
        modelBuilder.Entity<QrCode>().HasIndex(x => x.Code).IsUnique();
        modelBuilder.Entity<QrCode>().Property(x => x.TargetType).HasMaxLength(64).IsRequired();
        modelBuilder.Entity<QrCode>().Property(x => x.TableLabel).HasMaxLength(120);
        modelBuilder.Entity<QrCode>().HasIndex(x => new { x.TenantId, x.BranchId, x.TargetType, x.TableLabel }).IsUnique();
        modelBuilder.Entity<QrCode>().HasAlternateKey(x => new { x.TenantId, x.Id });
        modelBuilder.Entity<QrCode>()
            .HasIndex(x => new { x.TenantId, x.BranchId, x.TableId, x.TargetType })
            .IsUnique()
            .HasFilter("[TableId] IS NOT NULL");
        modelBuilder.Entity<User>().HasIndex(x => x.NormalizedEmail).IsUnique();
        modelBuilder.Entity<User>().Property(x => x.Email).HasMaxLength(320).IsRequired();
        modelBuilder.Entity<User>().Property(x => x.NormalizedEmail).HasMaxLength(320).IsRequired();
        modelBuilder.Entity<User>().Property(x => x.DisplayName).HasMaxLength(120).IsRequired();
        modelBuilder.Entity<User>().Property(x => x.PasswordHash).HasMaxLength(512).IsRequired();
        modelBuilder.Entity<User>().Property(x => x.SecurityStamp).HasMaxLength(64).IsRequired();
        modelBuilder.Entity<PasswordResetToken>().HasIndex(x => x.TokenHash).IsUnique();
        modelBuilder.Entity<PasswordResetToken>().HasIndex(x => new { x.UserId, x.UsedAtUtc, x.ExpiresAtUtc });
        modelBuilder.Entity<PasswordResetToken>()
            .HasOne(x => x.User)
            .WithMany()
            .HasForeignKey(x => x.UserId)
            .OnDelete(DeleteBehavior.Cascade);
        modelBuilder.Entity<Membership>().HasIndex(x => new { x.TenantId, x.UserId }).IsUnique();
        modelBuilder.Entity<Membership>().Property(x => x.Role).HasMaxLength(32);
        modelBuilder.Entity<Membership>().HasAlternateKey(x => new { x.TenantId, x.Id });
        modelBuilder.Entity<PermissionDefinition>().HasIndex(x => x.Code).IsUnique();
        modelBuilder.Entity<PermissionDefinition>().Property(x => x.Code).HasMaxLength(120).IsRequired();
        modelBuilder.Entity<PermissionDefinition>().Property(x => x.GroupCode).HasMaxLength(64).IsRequired();
        modelBuilder.Entity<PermissionDefinition>().Property(x => x.NameEn).HasMaxLength(160).IsRequired();
        modelBuilder.Entity<PermissionDefinition>().Property(x => x.NameAr).HasMaxLength(160).IsRequired();
        modelBuilder.Entity<RolePermission>().HasIndex(x => new { x.Role, x.PermissionCode }).IsUnique();
        modelBuilder.Entity<RolePermission>().Property(x => x.Role).HasConversion<string>();
        modelBuilder.Entity<RolePermission>().Property(x => x.PermissionCode).HasMaxLength(120).IsRequired();
        modelBuilder.Entity<UserPermission>().HasIndex(x => new { x.TenantId, x.MembershipId, x.PermissionCode }).IsUnique();
        modelBuilder.Entity<UserPermission>().Property(x => x.PermissionCode).HasMaxLength(120).IsRequired();
        modelBuilder.Entity<UserPermission>()
            .HasOne(x => x.Membership)
            .WithMany()
            .HasForeignKey(x => new { x.TenantId, x.MembershipId })
            .HasPrincipalKey(x => new { x.TenantId, x.Id })
            .OnDelete(DeleteBehavior.Cascade);
        modelBuilder.Entity<BranchMenuItemOverride>().HasIndex(x => new { x.TenantId, x.BranchId, x.MenuItemId }).IsUnique();
        modelBuilder.Entity<LookupValue>().HasIndex(x => new { x.TenantId, x.Type, x.Code }).IsUnique();
        modelBuilder.Entity<LookupValue>().HasIndex(x => new { x.TenantId, x.Type, x.IsActive, x.SortOrder });
        modelBuilder.Entity<LookupValue>().Property(x => x.Type).HasMaxLength(64).IsRequired();
        modelBuilder.Entity<LookupValue>().Property(x => x.Code).HasMaxLength(64).IsRequired();
        modelBuilder.Entity<LookupValue>().Property(x => x.NameEn).HasMaxLength(160).IsRequired();
        modelBuilder.Entity<LookupValue>().Property(x => x.NameAr).HasMaxLength(160);
        modelBuilder.Entity<LookupValue>().Property(x => x.Description).HasMaxLength(500);
        modelBuilder.Entity<LookupType>().HasIndex(x => new { x.TenantId, x.Code }).IsUnique();
        modelBuilder.Entity<LookupType>().Property(x => x.Code).HasMaxLength(64).IsRequired();
        modelBuilder.Entity<LookupType>().Property(x => x.NameEn).HasMaxLength(160).IsRequired();
        modelBuilder.Entity<LookupType>().Property(x => x.NameAr).HasMaxLength(160);
        modelBuilder.Entity<LookupType>().Property(x => x.Description).HasMaxLength(500);
        modelBuilder.Entity<PriceHistory>().HasIndex(x => new { x.TenantId, x.CreatedAtUtc });
        modelBuilder.Entity<PriceHistory>().Property(x => x.PreviousPrice).HasPrecision(18, 2);
        modelBuilder.Entity<PriceHistory>().Property(x => x.NewPrice).HasPrecision(18, 2);
        modelBuilder.Entity<PriceHistory>().Property(x => x.ChangeAmount).HasPrecision(18, 2);
        modelBuilder.Entity<PriceHistory>().Property(x => x.ChangePercentage).HasPrecision(9, 4);
        modelBuilder.Entity<PriceHistory>().Property(x => x.OperationCode).HasMaxLength(64).IsRequired();
        modelBuilder.Entity<PriceHistory>().Property(x => x.Reason).HasMaxLength(500);
        modelBuilder.Entity<PriceHistory>()
            .HasOne(x => x.MenuItem)
            .WithMany()
            .HasForeignKey(x => new { x.TenantId, x.MenuItemId })
            .HasPrincipalKey(x => new { x.TenantId, x.Id })
            .OnDelete(DeleteBehavior.NoAction);
        modelBuilder.Entity<PriceHistory>()
            .HasOne(x => x.Branch)
            .WithMany()
            .HasForeignKey(x => new { x.TenantId, x.BranchId })
            .HasPrincipalKey(x => new { x.TenantId, x.Id })
            .OnDelete(DeleteBehavior.NoAction);

        modelBuilder.Entity<BranchMenu>()
            .HasKey(x => new { x.BranchId, x.MenuId });
        modelBuilder.Entity<BranchMenu>().HasIndex(x => new { x.TenantId, x.MenuId, x.IsActive });
        modelBuilder.Entity<MenuCategory>().HasIndex(x => new { x.TenantId, x.MenuId, x.IsActive, x.SortOrder });
        modelBuilder.Entity<MenuItem>().HasIndex(x => new { x.TenantId, x.MenuCategoryId, x.IsAvailable, x.SortOrder });

        modelBuilder.Entity<BranchMenu>()
            .HasOne(x => x.Branch)
            .WithMany(x => x.BranchMenus)
            .HasForeignKey(x => new { x.TenantId, x.BranchId })
            .HasPrincipalKey(x => new { x.TenantId, x.Id })
            .OnDelete(DeleteBehavior.NoAction);

        modelBuilder.Entity<BranchMenu>()
            .HasOne(x => x.Menu)
            .WithMany(x => x.BranchMenus)
            .HasForeignKey(x => new { x.TenantId, x.MenuId })
            .HasPrincipalKey(x => new { x.TenantId, x.Id })
            .OnDelete(DeleteBehavior.NoAction);

        modelBuilder.Entity<MenuCategory>()
            .HasOne(x => x.Menu)
            .WithMany(x => x.Categories)
            .HasForeignKey(x => new { x.TenantId, x.MenuId })
            .HasPrincipalKey(x => new { x.TenantId, x.Id })
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<MenuCategory>()
            .HasOne<MenuCategory>()
            .WithMany()
            .HasForeignKey(x => new { x.TenantId, x.ParentCategoryId })
            .HasPrincipalKey(x => new { x.TenantId, x.Id })
            .OnDelete(DeleteBehavior.NoAction);

        modelBuilder.Entity<MenuItem>()
            .HasOne(x => x.MenuCategory)
            .WithMany(x => x.Items)
            .HasForeignKey(x => new { x.TenantId, x.MenuCategoryId })
            .HasPrincipalKey(x => new { x.TenantId, x.Id })
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<MenuItemImage>()
            .HasOne(x => x.MenuItem)
            .WithMany(x => x.Images)
            .HasForeignKey(x => new { x.TenantId, x.MenuItemId })
            .HasPrincipalKey(x => new { x.TenantId, x.Id })
              .OnDelete(DeleteBehavior.Cascade);
        modelBuilder.Entity<MenuItemImage>().Property(x => x.Url).HasMaxLength(500).IsRequired();
        modelBuilder.Entity<MenuItemImage>().Property(x => x.StorageKey).HasMaxLength(260);
        modelBuilder.Entity<MenuItemImage>().Property(x => x.OriginalFileName).HasMaxLength(260);
        modelBuilder.Entity<MenuItemImage>().Property(x => x.ContentType).HasMaxLength(100);
        modelBuilder.Entity<MenuItemImage>().Property(x => x.AltText).HasMaxLength(300);
        modelBuilder.Entity<MenuItemImage>().HasIndex(x => new { x.TenantId, x.MenuItemId, x.SortOrder });

        modelBuilder.Entity<TenantBrandingImage>().HasIndex(x => new { x.TenantId, x.Kind }).IsUnique();
        modelBuilder.Entity<TenantBrandingImage>().Property(x => x.Kind).HasConversion<string>().HasMaxLength(32);
        modelBuilder.Entity<TenantBrandingImage>().Property(x => x.Url).HasMaxLength(500).IsRequired();
        modelBuilder.Entity<TenantBrandingImage>().Property(x => x.StorageKey).HasMaxLength(260).IsRequired();
        modelBuilder.Entity<TenantBrandingImage>().Property(x => x.OriginalFileName).HasMaxLength(260);
        modelBuilder.Entity<TenantBrandingImage>().Property(x => x.ContentType).HasMaxLength(100).IsRequired();
        modelBuilder.Entity<TenantBrandingImage>().Property(x => x.AltText).HasMaxLength(300);
        modelBuilder.Entity<TenantBrandingImage>()
            .HasOne(x => x.Tenant)
            .WithMany(x => x.BrandingImages)
            .HasForeignKey(x => x.TenantId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<MenuItemIngredient>()
            .HasKey(x => new { x.MenuItemId, x.IngredientId });

        modelBuilder.Entity<MenuItemIngredient>()
            .HasOne(x => x.MenuItem)
            .WithMany(x => x.Ingredients)
            .HasForeignKey(x => new { x.TenantId, x.MenuItemId })
            .HasPrincipalKey(x => new { x.TenantId, x.Id })
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<MenuItemIngredient>()
            .HasOne(x => x.Ingredient)
            .WithMany(x => x.MenuItems)
            .HasForeignKey(x => new { x.TenantId, x.IngredientId })
            .HasPrincipalKey(x => new { x.TenantId, x.Id })
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<Allergen>().HasIndex(x => new { x.TenantId, x.Name }).IsUnique();
        modelBuilder.Entity<Allergen>().Property(x => x.Name).HasMaxLength(160).IsRequired();
        modelBuilder.Entity<Allergen>().Property(x => x.NameEn).HasMaxLength(160);
        modelBuilder.Entity<Allergen>().Property(x => x.NameAr).HasMaxLength(160);
        modelBuilder.Entity<Allergen>().Property(x => x.IsActive).HasDefaultValue(true);
        modelBuilder.Entity<Allergen>().HasAlternateKey(x => new { x.TenantId, x.Id });
        modelBuilder.Entity<MenuItemAllergen>().HasKey(x => new { x.MenuItemId, x.AllergenId });
        modelBuilder.Entity<MenuItemAllergen>()
            .HasOne(x => x.MenuItem)
            .WithMany(x => x.Allergens)
            .HasForeignKey(x => new { x.TenantId, x.MenuItemId })
            .HasPrincipalKey(x => new { x.TenantId, x.Id })
            .OnDelete(DeleteBehavior.Cascade);
        modelBuilder.Entity<MenuItemAllergen>()
            .HasOne(x => x.Allergen)
            .WithMany(x => x.MenuItems)
            .HasForeignKey(x => new { x.TenantId, x.AllergenId })
            .HasPrincipalKey(x => new { x.TenantId, x.Id })
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<Subscription>()
            .HasOne(x => x.Plan)
            .WithMany()
            .HasForeignKey(x => x.PlanId)
            .OnDelete(DeleteBehavior.Restrict);
        modelBuilder.Entity<Subscription>().HasAlternateKey(x => new { x.TenantId, x.Id });
        modelBuilder.Entity<Subscription>().Property(x => x.Status).HasMaxLength(32);
        modelBuilder.Entity<Subscription>().Property(x => x.PaymentProvider).HasMaxLength(64);
        modelBuilder.Entity<Subscription>().Property(x => x.ExternalSubscriptionId).HasMaxLength(200);

        modelBuilder.Entity<PaymentTransaction>()
            .HasIndex(x => new { x.Provider, x.ProviderReference })
            .IsUnique();

        modelBuilder.Entity<PaymentTransaction>()
            .Property(x => x.Amount)
            .HasPrecision(18, 2);

        modelBuilder.Entity<PaymentTransaction>()
            .Property(x => x.Status)
            .HasConversion<string>()
            .HasMaxLength(32);
        modelBuilder.Entity<PaymentTransaction>().Property(x => x.Provider).HasMaxLength(64).IsRequired();
        modelBuilder.Entity<PaymentTransaction>().Property(x => x.ProviderReference).HasMaxLength(200).IsRequired();
        modelBuilder.Entity<PaymentTransaction>().Property(x => x.Currency).HasMaxLength(8).IsRequired();
        modelBuilder.Entity<PaymentTransaction>().Property(x => x.CheckoutUrl).HasMaxLength(1000);
        modelBuilder.Entity<PaymentTransaction>().Property(x => x.RowVersion).IsRowVersion();
        modelBuilder.Entity<PaymentTransaction>().HasIndex(x => new { x.Provider, x.ProviderReference }).IsUnique();
        modelBuilder.Entity<PaymentTransaction>().HasIndex(x => new { x.TenantId, x.Status, x.CreatedAtUtc });

        modelBuilder.Entity<PaymentTransaction>()
            .HasOne(x => x.Subscription)
            .WithMany()
            .HasForeignKey(x => new { x.TenantId, x.SubscriptionId })
            .HasPrincipalKey(x => new { x.TenantId, x.Id })
            .OnDelete(DeleteBehavior.NoAction);

        modelBuilder.Entity<PaymentTransaction>()
            .HasOne<Plan>()
            .WithMany()
            .HasForeignKey(x => x.RequestedPlanId)
            .OnDelete(DeleteBehavior.NoAction);

        modelBuilder.Entity<AuditLog>()
            .HasIndex(x => new { x.TenantId, x.CreatedAtUtc });
        modelBuilder.Entity<AuditLog>().Property(x => x.Action).HasMaxLength(120).IsRequired();
        modelBuilder.Entity<AuditLog>().Property(x => x.EntityType).HasMaxLength(120).IsRequired();
        modelBuilder.Entity<AuditLog>().Property(x => x.ActorDisplayName).HasMaxLength(160);

        modelBuilder.Entity<AnalyticsEvent>()
            .HasIndex(x => new { x.TenantId, x.EventType, x.CreatedAtUtc });
        modelBuilder.Entity<AnalyticsEvent>().Property(x => x.Device).HasMaxLength(512).IsRequired();
        modelBuilder.Entity<AnalyticsEvent>().Property(x => x.EventType).HasMaxLength(32);

        modelBuilder.Entity<AnalyticsEvent>()
            .HasIndex(x => new { x.TenantId, x.BranchId, x.CreatedAtUtc });

        modelBuilder.Entity<AnalyticsEvent>()
            .Property(x => x.EventType)
            .HasConversion<string>();

        modelBuilder.Entity<AnalyticsEvent>()
            .HasOne<Branch>()
            .WithMany()
            .HasForeignKey(x => new { x.TenantId, x.BranchId })
            .HasPrincipalKey(x => new { x.TenantId, x.Id })
            .OnDelete(DeleteBehavior.NoAction);

        modelBuilder.Entity<AnalyticsEvent>()
            .HasOne<Menu>()
            .WithMany()
            .HasForeignKey(x => new { x.TenantId, x.MenuId })
            .HasPrincipalKey(x => new { x.TenantId, x.Id })
            .OnDelete(DeleteBehavior.NoAction);

        modelBuilder.Entity<AnalyticsEvent>()
            .HasOne<MenuItem>()
            .WithMany()
            .HasForeignKey(x => new { x.TenantId, x.MenuItemId })
            .HasPrincipalKey(x => new { x.TenantId, x.Id })
            .OnDelete(DeleteBehavior.NoAction);

        modelBuilder.Entity<PlanFeature>()
            .HasIndex(x => new { x.PlanId, x.FeatureKey })
            .IsUnique();

        modelBuilder.Entity<PlanFeature>()
            .HasOne(x => x.Plan)
            .WithMany(x => x.Features)
            .HasForeignKey(x => x.PlanId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<Subscription>()
            .Property(x => x.Status)
            .HasConversion<string>();

        modelBuilder.Entity<Membership>()
            .Property(x => x.Role)
            .HasConversion<string>();

        modelBuilder.Entity<Membership>()
            .HasOne(x => x.User)
            .WithMany(x => x.Memberships)
            .HasForeignKey(x => x.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<Membership>()
            .HasOne(x => x.Branch)
            .WithMany()
            .HasForeignKey(x => new { x.TenantId, x.BranchId })
            .HasPrincipalKey(x => new { x.TenantId, x.Id })
            .OnDelete(DeleteBehavior.NoAction);

        modelBuilder.Entity<BranchMenuItemOverride>()
            .Property(x => x.PriceOverride)
            .HasPrecision(18, 2);

        modelBuilder.Entity<BranchSpecificMenuItem>()
            .Property(x => x.Price)
            .HasPrecision(18, 2);
        modelBuilder.Entity<BranchSpecificMenuItem>().Property(x => x.Name).HasMaxLength(160).IsRequired();
        modelBuilder.Entity<BranchSpecificMenuItem>().Property(x => x.NameEn).HasMaxLength(160);
        modelBuilder.Entity<BranchSpecificMenuItem>().Property(x => x.NameAr).HasMaxLength(160);
        modelBuilder.Entity<BranchSpecificMenuItem>().Property(x => x.Description).HasMaxLength(2000);
        modelBuilder.Entity<BranchSpecificMenuItem>().Property(x => x.DescriptionEn).HasMaxLength(2000);
        modelBuilder.Entity<BranchSpecificMenuItem>().Property(x => x.DescriptionAr).HasMaxLength(2000);
        modelBuilder.Entity<BranchSpecificMenuItem>().Property(x => x.Currency).HasMaxLength(8).IsRequired();

        modelBuilder.Entity<BranchMenuItemOverride>()
            .HasOne(x => x.Branch)
            .WithMany()
            .HasForeignKey(x => new { x.TenantId, x.BranchId })
            .HasPrincipalKey(x => new { x.TenantId, x.Id })
            .OnDelete(DeleteBehavior.NoAction);

        modelBuilder.Entity<BranchMenuItemOverride>()
            .HasOne(x => x.MenuItem)
            .WithMany()
            .HasForeignKey(x => new { x.TenantId, x.MenuItemId })
            .HasPrincipalKey(x => new { x.TenantId, x.Id })
            .OnDelete(DeleteBehavior.NoAction);

        modelBuilder.Entity<BranchSpecificMenuItem>()
            .HasOne(x => x.Branch)
            .WithMany()
            .HasForeignKey(x => new { x.TenantId, x.BranchId })
            .HasPrincipalKey(x => new { x.TenantId, x.Id })
            .OnDelete(DeleteBehavior.NoAction);

        modelBuilder.Entity<BranchSpecificMenuItem>()
            .HasOne(x => x.Category)
            .WithMany()
            .HasForeignKey(x => new { x.TenantId, x.CategoryId })
            .HasPrincipalKey(x => new { x.TenantId, x.Id })
            .OnDelete(DeleteBehavior.NoAction);

        modelBuilder.Entity<ModifierOption>()
            .HasOne(x => x.Modifier)
            .WithMany(x => x.Options)
            .HasForeignKey(x => new { x.TenantId, x.ModifierId })
            .HasPrincipalKey(x => new { x.TenantId, x.Id })
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<MenuItemModifier>()
            .HasOne(x => x.MenuItem)
            .WithMany(x => x.Modifiers)
            .HasForeignKey(x => new { x.TenantId, x.MenuItemId })
            .HasPrincipalKey(x => new { x.TenantId, x.Id })
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<MenuItemModifier>()
            .HasOne(x => x.Modifier)
            .WithMany(x => x.MenuItems)
            .HasForeignKey(x => new { x.TenantId, x.ModifierId })
            .HasPrincipalKey(x => new { x.TenantId, x.Id })
            .OnDelete(DeleteBehavior.NoAction);

        modelBuilder.Entity<QrCode>()
            .HasOne(x => x.Branch)
            .WithMany(x => x.QrCodes)
            .HasForeignKey(x => new { x.TenantId, x.BranchId })
            .HasPrincipalKey(x => new { x.TenantId, x.Id })
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<RestaurantTable>()
            .HasOne(x => x.Branch)
            .WithMany(x => x.Tables)
            .HasForeignKey(x => new { x.TenantId, x.BranchId })
            .HasPrincipalKey(x => new { x.TenantId, x.Id })
            .OnDelete(DeleteBehavior.NoAction);

        modelBuilder.Entity<QrCode>()
            .HasOne(x => x.Table)
            .WithMany(x => x.QrCodes)
            .HasForeignKey(x => new { x.TenantId, x.TableId })
            .HasPrincipalKey(x => new { x.TenantId, x.Id })
            .OnDelete(DeleteBehavior.NoAction);

        modelBuilder.Entity<Menu>()
            .Property(x => x.Status)
            .HasConversion<string>()
            .HasMaxLength(32);

        modelBuilder.Entity<MenuItem>()
            .Property(x => x.Price)
            .HasPrecision(18, 2);

        modelBuilder.Entity<Plan>()
            .Property(x => x.MonthlyPrice)
            .HasPrecision(18, 2);
        modelBuilder.Entity<Plan>().Property(x => x.Currency).HasMaxLength(3).IsRequired();
        modelBuilder.Entity<Plan>().Property(x => x.Name).HasMaxLength(160).IsRequired();
        modelBuilder.Entity<Plan>().Property(x => x.IsActive).HasDefaultValue(true);
        modelBuilder.Entity<PaymentTransaction>().HasIndex(x => x.RequestedPlanId);

        // Tenant isolation for tenant-owned data.
        modelBuilder.Entity<Tenant>().HasQueryFilter(x =>
            _tenantContext.HasTenant && x.Id == _tenantContext.TenantId);

        modelBuilder.Entity<Branch>().HasQueryFilter(x =>
            _tenantContext.HasTenant && x.TenantId == _tenantContext.TenantId);

        modelBuilder.Entity<Menu>().HasQueryFilter(x =>
            _tenantContext.HasTenant && x.TenantId == _tenantContext.TenantId);

        modelBuilder.Entity<BranchMenu>().HasQueryFilter(x =>
            _tenantContext.HasTenant && x.TenantId == _tenantContext.TenantId);

        modelBuilder.Entity<MenuCategory>().HasQueryFilter(x =>
            _tenantContext.HasTenant && x.TenantId == _tenantContext.TenantId);

        modelBuilder.Entity<MenuItem>().HasQueryFilter(x =>
            _tenantContext.HasTenant && x.TenantId == _tenantContext.TenantId);

        modelBuilder.Entity<Ingredient>().HasIndex(x => new { x.TenantId, x.Name }).IsUnique();
        modelBuilder.Entity<Ingredient>().Property(x => x.Name).HasMaxLength(160).IsRequired();
        modelBuilder.Entity<Ingredient>().Property(x => x.NameEn).HasMaxLength(160);
        modelBuilder.Entity<Ingredient>().Property(x => x.NameAr).HasMaxLength(160);
        modelBuilder.Entity<Ingredient>().Property(x => x.IsActive).HasDefaultValue(true);
        modelBuilder.Entity<Ingredient>().HasAlternateKey(x => new { x.TenantId, x.Id });
        modelBuilder.Entity<Modifier>().HasIndex(x => new { x.TenantId, x.Name }).IsUnique();
        modelBuilder.Entity<Modifier>().Property(x => x.Name).HasMaxLength(160).IsRequired();
        modelBuilder.Entity<Modifier>().Property(x => x.NameEn).HasMaxLength(160);
        modelBuilder.Entity<Modifier>().Property(x => x.NameAr).HasMaxLength(160);
        modelBuilder.Entity<Modifier>().HasAlternateKey(x => new { x.TenantId, x.Id });
        modelBuilder.Entity<ModifierOption>().HasIndex(x => new { x.TenantId, x.ModifierId, x.Name }).IsUnique();
        modelBuilder.Entity<ModifierOption>().Property(x => x.Name).HasMaxLength(160).IsRequired();
        modelBuilder.Entity<ModifierOption>().Property(x => x.NameEn).HasMaxLength(160);
        modelBuilder.Entity<ModifierOption>().Property(x => x.NameAr).HasMaxLength(160);
        modelBuilder.Entity<MenuItemModifier>().HasKey(x => new { x.MenuItemId, x.ModifierId });
        modelBuilder.Entity<MenuItemModifier>().HasIndex(x => new { x.TenantId, x.ModifierId });
        modelBuilder.Entity<ModifierOption>().Property(x => x.PriceAdjustment).HasPrecision(18, 2);
        modelBuilder.Entity<ModifierOption>().HasAlternateKey(x => new { x.TenantId, x.Id });

        modelBuilder.Entity<MenuItemImage>().HasQueryFilter(x =>
            _tenantContext.HasTenant && x.TenantId == _tenantContext.TenantId);

        modelBuilder.Entity<TenantBrandingImage>().HasQueryFilter(x =>
            _tenantContext.HasTenant && x.TenantId == _tenantContext.TenantId);

        modelBuilder.Entity<Ingredient>().HasQueryFilter(x =>
            _tenantContext.HasTenant && x.TenantId == _tenantContext.TenantId);

        modelBuilder.Entity<MenuItemIngredient>().HasQueryFilter(x =>
            _tenantContext.HasTenant && x.TenantId == _tenantContext.TenantId);

        modelBuilder.Entity<Allergen>().HasQueryFilter(x =>
            _tenantContext.HasTenant && x.TenantId == _tenantContext.TenantId);

        modelBuilder.Entity<MenuItemAllergen>().HasQueryFilter(x =>
            _tenantContext.HasTenant && x.TenantId == _tenantContext.TenantId);

        modelBuilder.Entity<QrCode>().HasQueryFilter(x =>
            _tenantContext.HasTenant && x.TenantId == _tenantContext.TenantId);

        modelBuilder.Entity<RestaurantTable>().HasQueryFilter(x =>
            _tenantContext.HasTenant && x.TenantId == _tenantContext.TenantId);

        modelBuilder.Entity<Subscription>().HasQueryFilter(x =>
            _tenantContext.HasTenant && x.TenantId == _tenantContext.TenantId);

        modelBuilder.Entity<PaymentTransaction>().HasQueryFilter(x =>
            _tenantContext.HasTenant && x.TenantId == _tenantContext.TenantId);

        modelBuilder.Entity<AuditLog>().HasQueryFilter(x =>
            _tenantContext.HasTenant && x.TenantId == _tenantContext.TenantId);

        modelBuilder.Entity<AnalyticsEvent>().HasQueryFilter(x =>
            _tenantContext.HasTenant && x.TenantId == _tenantContext.TenantId);

        modelBuilder.Entity<Membership>().HasQueryFilter(x =>
            _tenantContext.HasTenant && x.TenantId == _tenantContext.TenantId);

        modelBuilder.Entity<UserPermission>().HasQueryFilter(x =>
            _tenantContext.HasTenant && x.TenantId == _tenantContext.TenantId);

        modelBuilder.Entity<BranchMenuItemOverride>().HasQueryFilter(x =>
            _tenantContext.HasTenant && x.TenantId == _tenantContext.TenantId);

        modelBuilder.Entity<BranchSpecificMenuItem>().HasQueryFilter(x =>
            _tenantContext.HasTenant && x.TenantId == _tenantContext.TenantId);

        modelBuilder.Entity<LookupValue>().HasQueryFilter(x =>
            x.IsGlobal || (_tenantContext.HasTenant && x.TenantId == _tenantContext.TenantId));

        modelBuilder.Entity<LookupType>().HasQueryFilter(x =>
            x.IsGlobal || (_tenantContext.HasTenant && x.TenantId == _tenantContext.TenantId));

        modelBuilder.Entity<PriceHistory>().HasQueryFilter(x =>
            _tenantContext.HasTenant && x.TenantId == _tenantContext.TenantId);

        modelBuilder.Entity<Modifier>().HasQueryFilter(x => _tenantContext.HasTenant && x.TenantId == _tenantContext.TenantId);
        modelBuilder.Entity<ModifierOption>().HasQueryFilter(x => _tenantContext.HasTenant && x.TenantId == _tenantContext.TenantId);
        modelBuilder.Entity<MenuItemModifier>().HasQueryFilter(x => _tenantContext.HasTenant && x.TenantId == _tenantContext.TenantId);

        modelBuilder.Entity<Order>().HasIndex(x => new { x.TenantId, x.OrderNumber }).IsUnique();
        modelBuilder.Entity<Order>().HasIndex(x => new { x.TenantId, x.IdempotencyKey }).IsUnique();
        modelBuilder.Entity<Order>().HasIndex(x => new { x.TenantId, x.BranchId, x.Status, x.CreatedAtUtc });
        modelBuilder.Entity<Order>().HasAlternateKey(x => new { x.TenantId, x.Id });
        modelBuilder.Entity<Order>().Property(x => x.OrderNumber).HasMaxLength(32).IsRequired();
        modelBuilder.Entity<Order>().Property(x => x.IdempotencyKey).HasMaxLength(120).IsRequired();
        modelBuilder.Entity<Order>().Property(x => x.CustomerName).HasMaxLength(160).IsRequired();
        modelBuilder.Entity<Order>().Property(x => x.CustomerPhone).HasMaxLength(40).IsRequired();
        modelBuilder.Entity<Order>().Property(x => x.Notes).HasMaxLength(500);
        modelBuilder.Entity<Order>().Property(x => x.Currency).HasMaxLength(3).IsRequired();
        modelBuilder.Entity<Order>().Property(x => x.Total).HasPrecision(18, 2);
        modelBuilder.Entity<Order>().Property(x => x.Status).HasConversion<string>().HasMaxLength(32);
        modelBuilder.Entity<Order>().Property(x => x.RowVersion).IsRowVersion();
        modelBuilder.Entity<Order>().HasQueryFilter(x => _tenantContext.HasTenant && x.TenantId == _tenantContext.TenantId);
        modelBuilder.Entity<Order>().HasOne(x => x.Branch).WithMany().HasForeignKey(x => new { x.TenantId, x.BranchId }).HasPrincipalKey(x => new { x.TenantId, x.Id }).OnDelete(DeleteBehavior.NoAction);
        modelBuilder.Entity<Order>().HasOne(x => x.Menu).WithMany().HasForeignKey(x => new { x.TenantId, x.MenuId }).HasPrincipalKey(x => new { x.TenantId, x.Id }).OnDelete(DeleteBehavior.NoAction);
        modelBuilder.Entity<Order>().HasOne(x => x.Table).WithMany(x => x.Orders).HasForeignKey(x => new { x.TenantId, x.TableId }).HasPrincipalKey(x => new { x.TenantId, x.Id }).OnDelete(DeleteBehavior.NoAction);
        modelBuilder.Entity<Order>().HasOne(x => x.QrCode).WithMany().HasForeignKey(x => new { x.TenantId, x.QrCodeId }).HasPrincipalKey(x => new { x.TenantId, x.Id }).OnDelete(DeleteBehavior.NoAction);
        modelBuilder.Entity<OrderItem>().Property(x => x.ProductName).HasMaxLength(160).IsRequired();
        modelBuilder.Entity<OrderItem>().Property(x => x.UnitPrice).HasPrecision(18, 2);
        modelBuilder.Entity<OrderItem>().Property(x => x.LineTotal).HasPrecision(18, 2);
        modelBuilder.Entity<OrderItem>().HasAlternateKey(x => new { x.TenantId, x.Id });
        modelBuilder.Entity<OrderItem>().HasQueryFilter(x => _tenantContext.HasTenant && x.TenantId == _tenantContext.TenantId);
        modelBuilder.Entity<OrderItem>().HasOne(x => x.Order).WithMany(x => x.Items).HasForeignKey(x => new { x.TenantId, x.OrderId }).HasPrincipalKey(x => new { x.TenantId, x.Id }).OnDelete(DeleteBehavior.Cascade);
        modelBuilder.Entity<OrderItem>().HasOne(x => x.MenuItem).WithMany().HasForeignKey(x => new { x.TenantId, x.MenuItemId }).HasPrincipalKey(x => new { x.TenantId, x.Id }).OnDelete(DeleteBehavior.NoAction);
        modelBuilder.Entity<OrderItemModifier>().Property(x => x.OptionName).HasMaxLength(160).IsRequired();
        modelBuilder.Entity<OrderItemModifier>().Property(x => x.PriceAdjustment).HasPrecision(18, 2);
        modelBuilder.Entity<OrderItemModifier>().HasAlternateKey(x => new { x.TenantId, x.Id });
        modelBuilder.Entity<OrderItemModifier>().HasQueryFilter(x => _tenantContext.HasTenant && x.TenantId == _tenantContext.TenantId);
        modelBuilder.Entity<OrderItemModifier>().HasOne(x => x.OrderItem).WithMany(x => x.Modifiers).HasForeignKey(x => new { x.TenantId, x.OrderItemId }).HasPrincipalKey(x => new { x.TenantId, x.Id }).OnDelete(DeleteBehavior.Cascade);
        modelBuilder.Entity<OrderItemModifier>().HasOne(x => x.ModifierOption).WithMany().HasForeignKey(x => new { x.TenantId, x.ModifierOptionId }).HasPrincipalKey(x => new { x.TenantId, x.Id }).OnDelete(DeleteBehavior.NoAction);
        modelBuilder.Entity<OrderStatusHistory>().Property(x => x.FromStatus).HasConversion<string>().HasMaxLength(32);
        modelBuilder.Entity<OrderStatusHistory>().Property(x => x.ToStatus).HasConversion<string>().HasMaxLength(32);
        modelBuilder.Entity<OrderStatusHistory>().Property(x => x.ActorDisplayName).HasMaxLength(160);
        modelBuilder.Entity<OrderStatusHistory>().HasQueryFilter(x => _tenantContext.HasTenant && x.TenantId == _tenantContext.TenantId);
        modelBuilder.Entity<OrderStatusHistory>().HasOne(x => x.Order).WithMany(x => x.StatusHistory).HasForeignKey(x => new { x.TenantId, x.OrderId }).HasPrincipalKey(x => new { x.TenantId, x.Id }).OnDelete(DeleteBehavior.Cascade);
    }

    public override int SaveChanges(bool acceptAllChangesOnSuccess)
    {
        ValidateTenantOwnership();
        return base.SaveChanges(acceptAllChangesOnSuccess);
    }

    public override Task<int> SaveChangesAsync(
        bool acceptAllChangesOnSuccess,
        CancellationToken cancellationToken = default)
    {
        ValidateTenantOwnership();
        return base.SaveChangesAsync(acceptAllChangesOnSuccess, cancellationToken);
    }

    private void ValidateTenantOwnership()
    {
        foreach (var entry in ChangeTracker.Entries<Tenant>())
        {
            if (entry.State is EntityState.Unchanged or EntityState.Detached)
                continue;

            if (_tenantContext.IsPublic)
                throw new InvalidOperationException("Public requests cannot modify tenant administration data.");

            if (entry.State == EntityState.Added && !_tenantContext.HasTenant)
                continue;

            if (!_tenantContext.TenantId.HasValue || entry.Entity.Id != _tenantContext.TenantId.Value)
                throw new InvalidOperationException("Tenant settings cannot cross tenant boundaries.");
        }

        foreach (var entry in ChangeTracker.Entries<TenantEntity>())
        {
            if (entry.State is EntityState.Unchanged or EntityState.Detached)
                continue;

            var publicOperationalWrite = entry.Entity is AnalyticsEvent or AuditLog or Order or OrderItem or OrderItemModifier or OrderStatusHistory;
            if (_tenantContext.IsPublic && !publicOperationalWrite)
                throw new InvalidOperationException("Public requests cannot modify tenant administration data.");

            if (entry.Entity is LookupValue lookup && lookup.IsGlobal && lookup.TenantId == Guid.Empty)
                continue;

            if (entry.Entity is LookupType lookupType && lookupType.IsGlobal && lookupType.TenantId == Guid.Empty)
                continue;

            if (!_tenantContext.TenantId.HasValue)
                throw new InvalidOperationException("Tenant context is required for tenant-owned data.");

            if (entry.Entity.TenantId != _tenantContext.TenantId.Value)
                throw new InvalidOperationException("Tenant-owned data cannot cross tenant boundaries.");
        }
    }
}
