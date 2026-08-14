using Microsoft.EntityFrameworkCore;
using RestaurantMenuPlatform.Domain.Entities;
using RestaurantMenuPlatform.Domain.Constants;
using RestaurantMenuPlatform.Domain.Enums;
using RestaurantMenuPlatform.Domain.Interfaces;
using RestaurantMenuPlatform.Infrastructure.Identity;

namespace RestaurantMenuPlatform.Infrastructure.Persistence;

public sealed record DevelopmentLoginSeed(
    string? Email,
    string? Password,
    string? TenantSlug);

public sealed record DevelopmentPlatformAdminSeed(
    string? Email,
    string? Password);

public static class DbInitializer
{
    private const string DemoTenantSlug = "demo-restaurant";
    public const string PlatformSystemTenantSlug = "platform-system";

    public static async Task InitializeAsync(
        AppDbContext db,
        ITenantContext tenantContext,
        DevelopmentLoginSeed? developmentLogin = null,
        DevelopmentPlatformAdminSeed? developmentPlatformAdmin = null,
        bool allowDevelopmentSeed = false,
        CancellationToken cancellationToken = default)
    {
        await EnsurePlatformCatalogAsync(db, tenantContext, cancellationToken);

        if (!allowDevelopmentSeed)
            return;

        if (await db.Tenants.IgnoreQueryFilters().AnyAsync(cancellationToken))
        {
            var tenantIds = await db.Tenants
                .IgnoreQueryFilters()
                .Select(x => x.Id)
                .ToListAsync(cancellationToken);
            foreach (var tenantId in tenantIds)
            {
                tenantContext.SetTenant(tenantId);
                var tenantSlug = await db.Tenants
                    .IgnoreQueryFilters()
                    .Where(x => x.Id == tenantId)
                    .Select(x => x.Slug)
                    .SingleAsync(cancellationToken);
                // The demo catalog is an explicit local bootstrap and is never
                // executed outside Development.
                if (string.Equals(tenantSlug, DemoTenantSlug, StringComparison.OrdinalIgnoreCase))
                    await EnsureTenantLookupsAsync(db, tenantId, cancellationToken);
                await EnsureTenantContentDefaultsAsync(db, tenantId, cancellationToken);
            }

            var existingTenant = await db.Tenants
                .IgnoreQueryFilters()
                .SingleOrDefaultAsync(x => x.Slug == DemoTenantSlug, cancellationToken);

            if (existingTenant is not null)
            {
                tenantContext.SetTenant(existingTenant.Id);
                var branchesWithoutSlugs = await db.Branches
                    .Where(x => x.Slug == "")
                    .ToListAsync(cancellationToken);
                foreach (var branchWithoutSlug in branchesWithoutSlugs)
                    branchWithoutSlug.Slug = ToSlug(branchWithoutSlug.Name, branchWithoutSlug.Id);

                if (branchesWithoutSlugs.Count > 0)
                    await db.SaveChangesAsync(cancellationToken);

            }

            await EnsureDevelopmentLoginAsync(db, tenantContext, developmentLogin, cancellationToken);
            await EnsureDevelopmentPlatformAdminAsync(db, tenantContext, developmentPlatformAdmin, cancellationToken);

            return;
        }

        var plan = await db.Plans
            .Include(x => x.Features)
            .OrderBy(x => x.MonthlyPrice)
            .FirstAsync(cancellationToken);

        var tenant = new Tenant
        {
            Name = "Demo Restaurant",
            Slug = DemoTenantSlug,
            DefaultLanguage = "AR",
            SubscriptionStatus = SubscriptionStatus.Trial
        };

        var branch = new Branch
        {
            TenantId = tenant.Id,
            Name = "Main Branch",
            Slug = "main-branch",
            Address = "Cairo, Egypt"
        };

        var menu = new Menu
        {
            TenantId = tenant.Id,
            Name = "Main Menu",
            Slug = "main-menu",
            IsGlobal = true,
            Status = MenuStatus.Published
        };

        var category = new MenuCategory
        {
            TenantId = tenant.Id,
            MenuId = menu.Id,
            Name = "Burgers",
            SortOrder = 1
        };

        var item = new MenuItem
        {
            TenantId = tenant.Id,
            MenuCategoryId = category.Id,
            Name = "Classic Burger",
            Description = "Beef burger with cheese and fresh vegetables.",
            Price = 180,
            Currency = "EGP",
            SortOrder = 1
        };

        var branchMenu = new BranchMenu
        {
            TenantId = tenant.Id,
            BranchId = branch.Id,
            MenuId = menu.Id
        };

        var qr = new QrCode
        {
            TenantId = tenant.Id,
            BranchId = branch.Id,
            Code = $"demo-{Guid.NewGuid():N}",
            TargetType = "branch-menu"
        };

        var subscription = new Subscription
        {
            TenantId = tenant.Id,
            PlanId = plan.Id,
            Status = SubscriptionStatus.Trial,
            StartsAtUtc = DateTime.UtcNow,
            EndsAtUtc = DateTime.UtcNow.AddDays(14)
        };

        db.Tenants.Add(tenant);
        await db.SaveChangesAsync(cancellationToken);

        tenantContext.SetTenant(tenant.Id);
        await EnsureTenantLookupsAsync(db, tenant.Id, cancellationToken);
        await EnsureTenantContentDefaultsAsync(db, tenant.Id, cancellationToken);

        db.Branches.Add(branch);
        db.Menus.Add(menu);
        db.MenuCategories.Add(category);
        db.MenuItems.Add(item);
        db.BranchMenus.Add(branchMenu);
        db.QrCodes.Add(qr);
        db.Subscriptions.Add(subscription);
        await db.SaveChangesAsync(cancellationToken);
        await EnsureDevelopmentLoginAsync(db, tenantContext, developmentLogin, cancellationToken);
        await EnsureDevelopmentPlatformAdminAsync(db, tenantContext, developmentPlatformAdmin, cancellationToken);
    }

    private static async Task EnsurePlatformCatalogAsync(
        AppDbContext db,
        ITenantContext tenantContext,
        CancellationToken cancellationToken)
    {
        await EnsureGlobalOnboardingLookupsAsync(db, cancellationToken);
        await EnsureGlobalLookupTypesAsync(db, cancellationToken);
        await EnsurePermissionCatalogAsync(db, cancellationToken);
        await RemoveObsoleteTenantGlobalCopiesAsync(db, tenantContext, cancellationToken);

        var starterPlan = await db.Plans
            .Include(x => x.Features)
            .OrderBy(x => x.MonthlyPrice)
            .FirstOrDefaultAsync(cancellationToken);
        if (starterPlan is null)
        {
            starterPlan = new Plan
            {
                Name = "Starter",
                MonthlyPrice = 299,
                Currency = "EGP",
                MaxBranches = 2,
                MaxMenuItems = 100,
                MaxUsers = 3,
                AdvancedAnalytics = false,
                CustomBranding = false
            };
            db.Plans.Add(starterPlan);
            await db.SaveChangesAsync(cancellationToken);
        }

        await EnsurePlanFeaturesAsync(db, cancellationToken);
    }

    private static async Task EnsurePermissionCatalogAsync(AppDbContext db, CancellationToken cancellationToken)
    {
        var existingDefinitions = await db.PermissionDefinitions.ToDictionaryAsync(x => x.Code, cancellationToken);
        foreach (var definition in PermissionCatalog.Definitions)
        {
            if (!existingDefinitions.TryGetValue(definition.Code, out var entity))
            {
                db.PermissionDefinitions.Add(new PermissionDefinition
                {
                    Code = definition.Code,
                    GroupCode = definition.GroupCode,
                    NameEn = definition.NameEn,
                    NameAr = definition.NameAr,
                    SortOrder = definition.SortOrder,
                    IsActive = true
                });
            }
            else
            {
                entity.GroupCode = definition.GroupCode;
                entity.NameEn = definition.NameEn;
                entity.NameAr = definition.NameAr;
                entity.SortOrder = definition.SortOrder;
                entity.IsActive = true;
            }
        }

        var existingRoles = await db.RolePermissions.ToListAsync(cancellationToken);
        foreach (var role in Enum.GetValues<MembershipRole>())
        {
            foreach (var code in PermissionCatalog.Preset(role))
            {
                if (existingRoles.Any(x => x.Role == role && x.PermissionCode == code))
                    continue;
                db.RolePermissions.Add(new RolePermission { Role = role, PermissionCode = code });
            }
        }

        await db.SaveChangesAsync(cancellationToken);
    }

    private static async Task EnsureDevelopmentLoginAsync(
        AppDbContext db,
        ITenantContext tenantContext,
        DevelopmentLoginSeed? developmentLogin,
        CancellationToken cancellationToken)
    {
        if (developmentLogin is null ||
            string.IsNullOrWhiteSpace(developmentLogin.Email) ||
            string.IsNullOrWhiteSpace(developmentLogin.Password) ||
            string.IsNullOrWhiteSpace(developmentLogin.TenantSlug))
            return;

        PasswordService.ValidateStrength(developmentLogin.Password);

        var tenant = await db.Tenants
            .IgnoreQueryFilters()
            .SingleOrDefaultAsync(
                x => x.Slug == developmentLogin.TenantSlug && x.IsActive,
                cancellationToken);
        if (tenant is null)
            return;

        tenantContext.SetTenant(tenant.Id);

        var email = developmentLogin.Email.Trim();
        var normalizedEmail = email.ToUpperInvariant();
        var passwordService = new PasswordService();
        var user = await db.Users
            .IgnoreQueryFilters()
            .SingleOrDefaultAsync(x => x.NormalizedEmail == normalizedEmail, cancellationToken);

        if (user is null)
        {
            user = new User
            {
                Email = email,
                NormalizedEmail = normalizedEmail,
                DisplayName = "Development Admin",
                PasswordHash = passwordService.Hash(developmentLogin.Password)
            };
            db.Users.Add(user);
        }
        else
        {
            var passwordNeedsRepair = string.IsNullOrWhiteSpace(user.PasswordHash) ||
                !passwordService.Verify(developmentLogin.Password, user.PasswordHash) ||
                passwordService.NeedsRehash(user.PasswordHash);

            if (passwordNeedsRepair)
            {
                user.PasswordHash = passwordService.Hash(developmentLogin.Password);
                user.SecurityStamp = Guid.NewGuid().ToString("N");
                user.UpdatedAtUtc = DateTime.UtcNow;
            }

            if (user.FailedLoginCount != 0 || user.LockoutEndUtc.HasValue)
            {
                user.FailedLoginCount = 0;
                user.LockoutEndUtc = null;
                user.UpdatedAtUtc = DateTime.UtcNow;
            }

            if (!user.IsActive)
            {
                user.IsActive = true;
                user.UpdatedAtUtc = DateTime.UtcNow;
            }
        }

        var membership = await db.Memberships
            .IgnoreQueryFilters()
            .SingleOrDefaultAsync(
                x => x.UserId == user.Id && x.TenantId == tenant.Id,
                cancellationToken);
        if (membership is null)
        {
            db.Memberships.Add(new Membership
            {
                TenantId = tenant.Id,
                UserId = user.Id,
                Role = MembershipRole.TenantOwner,
                IsActive = true
            });
        }
        else
        {
            // A development bootstrap must repair the local login account, but
            // it must never silently downgrade an explicitly provisioned
            // platform operator to a tenant owner.
            if (membership.Role == MembershipRole.PlatformAdmin)
            {
                if (membership.BranchId.HasValue || !membership.IsActive)
                {
                    membership.BranchId = null;
                    membership.IsActive = true;
                    membership.UpdatedAtUtc = DateTime.UtcNow;
                }
            }
            else if (membership.Role != MembershipRole.TenantOwner ||
                     membership.BranchId.HasValue ||
                     !membership.IsActive)
            {
                membership.Role = MembershipRole.TenantOwner;
                membership.BranchId = null;
                membership.IsActive = true;
                membership.UpdatedAtUtc = DateTime.UtcNow;
            }
        }

        await db.SaveChangesAsync(cancellationToken);
    }

    private static async Task EnsureDevelopmentPlatformAdminAsync(
        AppDbContext db,
        ITenantContext tenantContext,
        DevelopmentPlatformAdminSeed? seed,
        CancellationToken cancellationToken)
    {
        if (seed is null ||
            string.IsNullOrWhiteSpace(seed.Email) ||
            string.IsNullOrWhiteSpace(seed.Password))
            return;

        PasswordService.ValidateStrength(seed.Password);

        var platformTenant = await db.Tenants
            .IgnoreQueryFilters()
            .SingleOrDefaultAsync(x => x.Slug == PlatformSystemTenantSlug, cancellationToken);
        if (platformTenant is null)
        {
            platformTenant = new Tenant
            {
                Name = "Platform Administration",
                NameEn = "Platform Administration",
                Slug = PlatformSystemTenantSlug,
                Currency = "EGP",
                DefaultLanguage = "AR",
                SubscriptionStatus = SubscriptionStatus.Trial,
                IsActive = true
            };
            db.Tenants.Add(platformTenant);
            tenantContext.SetTenant(platformTenant.Id);
            await db.SaveChangesAsync(cancellationToken);
        }
        else if (!platformTenant.IsActive)
        {
            platformTenant.IsActive = true;
            platformTenant.UpdatedAtUtc = DateTime.UtcNow;
            await db.SaveChangesAsync(cancellationToken);
        }

        tenantContext.SetTenant(platformTenant.Id);

        var email = seed.Email.Trim();
        var normalizedEmail = email.ToUpperInvariant();
        var passwordService = new PasswordService();
        var user = await db.Users
            .IgnoreQueryFilters()
            .SingleOrDefaultAsync(x => x.NormalizedEmail == normalizedEmail, cancellationToken);

        if (user is null)
        {
            user = new User
            {
                Email = email,
                NormalizedEmail = normalizedEmail,
                DisplayName = "Platform Administrator",
                PasswordHash = passwordService.Hash(seed.Password)
            };
            db.Users.Add(user);
        }
        else
        {
            var passwordNeedsRepair = string.IsNullOrWhiteSpace(user.PasswordHash) ||
                !passwordService.Verify(seed.Password, user.PasswordHash) ||
                passwordService.NeedsRehash(user.PasswordHash);
            if (passwordNeedsRepair)
            {
                user.PasswordHash = passwordService.Hash(seed.Password);
                user.SecurityStamp = Guid.NewGuid().ToString("N");
                user.UpdatedAtUtc = DateTime.UtcNow;
            }

            user.DisplayName = "Platform Administrator";
            user.IsActive = true;
            user.FailedLoginCount = 0;
            user.LockoutEndUtc = null;
            user.UpdatedAtUtc = DateTime.UtcNow;
        }

        await db.Memberships
            .IgnoreQueryFilters()
            .Where(x => x.UserId == user.Id && x.IsActive && x.TenantId != platformTenant.Id)
            .ExecuteUpdateAsync(
                setters => setters
                    .SetProperty(x => x.IsActive, false)
                    .SetProperty(x => x.UpdatedAtUtc, DateTime.UtcNow),
                cancellationToken);

        var platformMembership = await db.Memberships
            .IgnoreQueryFilters()
            .SingleOrDefaultAsync(
                x => x.UserId == user.Id && x.TenantId == platformTenant.Id,
                cancellationToken);
        if (platformMembership is null)
        {
            db.Memberships.Add(new Membership
            {
                TenantId = platformTenant.Id,
                UserId = user.Id,
                Role = MembershipRole.PlatformAdmin,
                BranchId = null,
                IsActive = true
            });
        }
        else
        {
            platformMembership.Role = MembershipRole.PlatformAdmin;
            platformMembership.BranchId = null;
            platformMembership.IsActive = true;
            platformMembership.UpdatedAtUtc = DateTime.UtcNow;
        }

        await db.SaveChangesAsync(cancellationToken);
    }

    private static async Task EnsurePlanFeaturesAsync(
        AppDbContext db,
        CancellationToken cancellationToken)
    {
        var plans = await db.Plans
            .Include(x => x.Features)
            .ToListAsync(cancellationToken);

        var changed = false;
        foreach (var plan in plans)
        {
            changed |= AddFeatureIfMissing(db, plan, FeatureKeys.AdvancedAnalytics, plan.AdvancedAnalytics);
            changed |= AddFeatureIfMissing(db, plan, FeatureKeys.CustomBranding, plan.CustomBranding);
            changed |= AddFeatureIfMissing(db, plan, FeatureKeys.BranchOverrides, true);
            changed |= AddFeatureIfMissing(db, plan, FeatureKeys.MenuImages, true);
        }

        if (changed)
            await db.SaveChangesAsync(cancellationToken);
    }

    public static async Task EnsureTenantLookupsAsync(
        AppDbContext db,
        Guid tenantId,
        CancellationToken cancellationToken)
    {
        await EnsureTenantLookupTypeAsync(db, tenantId, LookupTypes.PricingOperation, "Pricing operations", "Pricing operations", "Administrator-managed pricing operations.", 1, cancellationToken);
        await EnsureTenantLookupTypeAsync(db, tenantId, LookupTypes.PricingScope, "Pricing scopes", "Pricing scopes", "Where a pricing operation applies.", 2, cancellationToken);
        await EnsureTenantLookupTypeAsync(db, tenantId, LookupTypes.MenuType, "Menu types", "Menu types", "Tenant-managed menu classifications.", 3, cancellationToken);
        await EnsureTenantLookupTypeAsync(db, tenantId, LookupTypes.MenuScope, "Menu scopes", "Menu scopes", "Branch visibility scopes for menus.", 4, cancellationToken);
        await EnsureTenantLookupTypeAsync(db, tenantId, LookupTypes.ProductType, "Product types", "Product types", "Tenant-managed product classifications.", 5, cancellationToken);
        await EnsureTenantLookupTypeAsync(db, tenantId, LookupTypes.CategoryType, "Category types", "Category types", "Tenant-managed category classifications.", 6, cancellationToken);

        await EnsureLookupAsync(db, tenantId, LookupTypes.PricingOperation, PricingLookupCodes.PercentageIncrease, "Percentage increase", "زيادة نسبية", 1, cancellationToken);
        await EnsureLookupAsync(db, tenantId, LookupTypes.PricingOperation, PricingLookupCodes.PercentageDecrease, "Percentage decrease", "خفض نسبي", 2, cancellationToken);
        await EnsureLookupAsync(db, tenantId, LookupTypes.PricingOperation, PricingLookupCodes.FixedIncrease, "Fixed amount increase", "زيادة بقيمة ثابتة", 3, cancellationToken);
        await EnsureLookupAsync(db, tenantId, LookupTypes.PricingOperation, PricingLookupCodes.FixedDecrease, "Fixed amount decrease", "خفض بقيمة ثابتة", 4, cancellationToken);
        await EnsureLookupAsync(db, tenantId, LookupTypes.PricingOperation, PricingLookupCodes.SetExact, "Set exact price", "تحديد سعر ثابت", 5, cancellationToken);
        await EnsureLookupAsync(db, tenantId, LookupTypes.PricingScope, PricingLookupCodes.Product, "Individual products", "منتجات محددة", 1, cancellationToken);
        await EnsureLookupAsync(db, tenantId, LookupTypes.PricingScope, PricingLookupCodes.Category, "Category", "فئة", 2, cancellationToken);
        await EnsureLookupAsync(db, tenantId, LookupTypes.PricingScope, PricingLookupCodes.Branch, "Branch", "فرع", 3, cancellationToken);
        await EnsureLookupAsync(db, tenantId, LookupTypes.MenuType, "GENERAL", "General menu", "قائمة عامة", 1, cancellationToken);
        await EnsureLookupAsync(db, tenantId, LookupTypes.MenuType, "BREAKFAST", "Breakfast", "إفطار", 2, cancellationToken);
        await EnsureLookupAsync(db, tenantId, LookupTypes.MenuType, "SEASONAL", "Seasonal menu", "قائمة موسمية", 3, cancellationToken);
        await EnsureLookupAsync(db, tenantId, LookupTypes.MenuScope, MenuLookupCodes.AllBranches, "All branches", "كل الفروع", 1, cancellationToken);
        await EnsureLookupAsync(db, tenantId, LookupTypes.MenuScope, MenuLookupCodes.SelectedBranches, "Selected branches", "فروع محددة", 2, cancellationToken);
        await EnsureLookupAsync(db, tenantId, LookupTypes.ProductType, "DISH", "Dish", "طبق", 1, cancellationToken);
        await EnsureLookupAsync(db, tenantId, LookupTypes.ProductType, "BEVERAGE", "Beverage", "مشروب", 2, cancellationToken);
        await EnsureLookupAsync(db, tenantId, LookupTypes.ProductType, "DESSERT", "Dessert", "حلوى", 3, cancellationToken);
        await EnsureLookupAsync(db, tenantId, LookupTypes.CategoryType, "MAINS", "Main dishes", "Main dishes", 1, cancellationToken);
        await EnsureLookupAsync(db, tenantId, LookupTypes.CategoryType, "SIDES", "Side dishes", "Side dishes", 2, cancellationToken);
        await EnsureLookupAsync(db, tenantId, LookupTypes.CategoryType, "DESSERTS", "Desserts", "Desserts", 3, cancellationToken);
    }

    private static async Task EnsureGlobalOnboardingLookupsAsync(AppDbContext db, CancellationToken cancellationToken)
    {
        await EnsureGlobalLookupAsync(db, LookupTypes.Currency, "EGP", "Egyptian pound", "جنيه مصري", 1, cancellationToken);
        await EnsureGlobalLookupAsync(db, LookupTypes.Currency, "USD", "US dollar", "دولار أمريكي", 2, cancellationToken);
        await EnsureGlobalLookupAsync(db, LookupTypes.Currency, "EUR", "Euro", "يورو", 3, cancellationToken);
        await EnsureGlobalLookupAsync(db, LookupTypes.Language, "EN", "English", "الإنجليزية", 1, cancellationToken);
        await EnsureGlobalLookupAsync(db, LookupTypes.Language, "AR", "Arabic", "العربية", 2, cancellationToken);
    }

    private static async Task EnsureGlobalLookupTypesAsync(AppDbContext db, CancellationToken cancellationToken)
    {
        await EnsureGlobalLookupTypeAsync(db, LookupTypes.Currency, "Currencies", "Currencies", "Platform-supported currencies.", 1, cancellationToken);
        await EnsureGlobalLookupTypeAsync(db, LookupTypes.Language, "Languages", "Languages", "Platform-supported interface languages.", 2, cancellationToken);
    }

    private static async Task EnsureGlobalLookupTypeAsync(
        AppDbContext db,
        string code,
        string nameEn,
        string nameAr,
        string description,
        int sortOrder,
        CancellationToken cancellationToken)
    {
        var entity = await db.LookupTypes
            .IgnoreQueryFilters()
            .SingleOrDefaultAsync(x => x.IsGlobal && x.TenantId == Guid.Empty && x.Code == code, cancellationToken);
        if (entity is null)
        {
            db.LookupTypes.Add(new LookupType
            {
                TenantId = Guid.Empty,
                IsGlobal = true,
                Code = code,
                NameEn = nameEn,
                NameAr = nameAr,
                Description = description,
                SortOrder = sortOrder,
                IsActive = true
            });
        }
        else
        {
            entity.NameEn = nameEn;
            entity.NameAr = nameAr;
            entity.Description = description;
            entity.SortOrder = sortOrder;
            entity.IsActive = true;
            entity.UpdatedAtUtc = DateTime.UtcNow;
        }

        await db.SaveChangesAsync(cancellationToken);
    }

    private static async Task EnsureTenantLookupTypeAsync(
        AppDbContext db,
        Guid tenantId,
        string code,
        string nameEn,
        string nameAr,
        string description,
        int sortOrder,
        CancellationToken cancellationToken)
    {
        if (await db.LookupTypes.AnyAsync(
                x => !x.IsGlobal && x.TenantId == tenantId && x.Code == code,
                cancellationToken))
            return;

        db.LookupTypes.Add(new LookupType
        {
            TenantId = tenantId,
            Code = code,
            NameEn = nameEn,
            NameAr = nameAr,
            Description = description,
            SortOrder = sortOrder,
            IsActive = true
        });
        await db.SaveChangesAsync(cancellationToken);
    }

    private static async Task EnsureGlobalLookupAsync(AppDbContext db, string type, string code, string nameEn, string nameAr, int sortOrder, CancellationToken cancellationToken)
    {
        if (await db.LookupValues.IgnoreQueryFilters().AnyAsync(x => x.IsGlobal && x.Type == type && x.Code == code, cancellationToken)) return;
        db.LookupValues.Add(new LookupValue
        {
            TenantId = Guid.Empty,
            IsGlobal = true,
            Type = type,
            Code = code,
            NameEn = nameEn,
            NameAr = nameAr,
            SortOrder = sortOrder,
            IsActive = true
        });
        await db.SaveChangesAsync(cancellationToken);
    }

    private static async Task RemoveObsoleteTenantGlobalCopiesAsync(
        AppDbContext db,
        ITenantContext tenantContext,
        CancellationToken cancellationToken)
    {
        var globalCodes = await db.LookupValues
            .IgnoreQueryFilters()
            .Where(x => x.IsGlobal &&
                        (x.Type == LookupTypes.Currency || x.Type == LookupTypes.Language))
            .Select(x => new { x.Type, x.Code })
            .ToListAsync(cancellationToken);
        var globalCodeSet = globalCodes
            .Select(x => $"{x.Type}:{x.Code}")
            .ToHashSet(StringComparer.Ordinal);
        var obsoleteTenantCopies = await db.LookupValues
            .IgnoreQueryFilters()
            .Where(x => !x.IsGlobal &&
                        (x.Type == LookupTypes.Currency || x.Type == LookupTypes.Language))
            .ToListAsync(cancellationToken);
        var copiesToRemove = obsoleteTenantCopies
            .Where(x => globalCodeSet.Contains($"{x.Type}:{x.Code}"))
            .ToList();
        foreach (var tenantCopies in copiesToRemove.GroupBy(x => x.TenantId))
        {
            tenantContext.SetTenant(tenantCopies.Key);
            db.LookupValues.RemoveRange(tenantCopies);
            await db.SaveChangesAsync(cancellationToken);
        }
    }

    private static async Task EnsureLookupAsync(
        AppDbContext db,
        Guid tenantId,
        string type,
        string code,
        string nameEn,
        string nameAr,
        int sortOrder,
        CancellationToken cancellationToken)
    {
        if (await db.LookupValues.AnyAsync(
                x => x.TenantId == tenantId && x.Type == type && x.Code == code,
                cancellationToken))
            return;

        db.LookupValues.Add(new LookupValue
        {
            TenantId = tenantId,
            Type = type,
            Code = code,
            NameEn = nameEn,
            NameAr = nameAr,
            SortOrder = sortOrder,
            IsActive = true
        });
        await db.SaveChangesAsync(cancellationToken);
    }

    private static async Task EnsureTenantContentDefaultsAsync(
        AppDbContext db,
        Guid tenantId,
        CancellationToken cancellationToken)
    {
        var menuType = await db.LookupValues
            .Where(x => x.Type == LookupTypes.MenuType && x.IsActive)
            .OrderBy(x => x.SortOrder)
            .Select(x => x.Code)
            .FirstOrDefaultAsync(cancellationToken);
        var productType = await db.LookupValues
            .Where(x => x.Type == LookupTypes.ProductType && x.IsActive)
            .OrderBy(x => x.SortOrder)
            .Select(x => x.Code)
            .FirstOrDefaultAsync(cancellationToken);
        var allBranchesScope = await db.LookupValues
            .Where(x => x.Type == LookupTypes.MenuScope && x.IsActive && x.Code == MenuLookupCodes.AllBranches)
            .Select(x => x.Code)
            .FirstOrDefaultAsync(cancellationToken);
        var selectedBranchesScope = await db.LookupValues
            .Where(x => x.Type == LookupTypes.MenuScope && x.IsActive && x.Code == MenuLookupCodes.SelectedBranches)
            .Select(x => x.Code)
            .FirstOrDefaultAsync(cancellationToken);
        var currency = await db.LookupValues
            .Where(x => x.Type == LookupTypes.Currency && x.IsActive)
            .OrderBy(x => x.SortOrder)
            .Select(x => x.Code)
            .FirstOrDefaultAsync(cancellationToken);

        var tenant = await db.Tenants.IgnoreQueryFilters().SingleAsync(x => x.Id == tenantId, cancellationToken);
        tenant.IsActive = true;
        tenant.NameEn ??= tenant.Name;
        tenant.Currency ??= currency;
        foreach (var branch in await db.Branches.ToListAsync(cancellationToken))
            branch.NameEn ??= branch.Name;
        foreach (var menu in await db.Menus.ToListAsync(cancellationToken))
        {
            menu.NameEn ??= menu.Name;
            menu.ScopeCode ??= menu.IsGlobal ? allBranchesScope : selectedBranchesScope;
            menu.MenuTypeCode ??= menuType;
        }
        foreach (var category in await db.MenuCategories.ToListAsync(cancellationToken))
        {
            category.NameEn ??= category.Name;
            category.IsActive = true;
        }
        foreach (var item in await db.MenuItems.ToListAsync(cancellationToken))
        {
            item.NameEn ??= item.Name;
            item.DescriptionEn ??= item.Description;
            item.ProductTypeCode ??= productType;
        }
        await db.SaveChangesAsync(cancellationToken);
    }

    private static bool AddFeatureIfMissing(AppDbContext db, Plan plan, string key, bool enabled)
    {
        if (plan.Features.Any(x => string.Equals(x.FeatureKey, key, StringComparison.OrdinalIgnoreCase)))
            return false;

        var feature = new PlanFeature
        {
            PlanId = plan.Id,
            FeatureKey = key,
            Enabled = enabled
        };
        plan.Features.Add(feature);
        db.PlanFeatures.Add(feature);
        return true;
    }

    private static string ToSlug(string value, Guid id)
    {
        var slug = System.Text.RegularExpressions.Regex
            .Replace(value.Trim().ToLowerInvariant(), "[^a-z0-9]+", "-")
            .Trim('-');
        return string.IsNullOrWhiteSpace(slug) ? $"branch-{id:N}" : slug;
    }
}
