using RestaurantMenuPlatform.Domain.Enums;

namespace RestaurantMenuPlatform.Domain.Constants;

public static class PermissionCatalog
{
    public const string RestaurantView = "Restaurant.View";
    public const string RestaurantEdit = "Restaurant.Edit";
    public const string BranchView = "Branch.View";
    public const string BranchCreate = "Branch.Create";
    public const string BranchEdit = "Branch.Edit";
    public const string BranchDelete = "Branch.Delete";
    public const string MenuView = "Menu.View";
    public const string MenuCreate = "Menu.Create";
    public const string MenuEdit = "Menu.Edit";
    public const string MenuPublish = "Menu.Publish";
    public const string CategoryView = "Category.View";
    public const string CategoryCreate = "Category.Create";
    public const string CategoryEdit = "Category.Edit";
    public const string CategoryDelete = "Category.Delete";
    public const string ProductView = "Product.View";
    public const string ProductCreate = "Product.Create";
    public const string ProductEdit = "Product.Edit";
    public const string ProductDelete = "Product.Delete";
    public const string ProductImages = "Product.Images";
    public const string IngredientView = "Ingredient.View";
    public const string IngredientManage = "Ingredient.Manage";
    public const string AllergenView = "Allergen.View";
    public const string AllergenManage = "Allergen.Manage";
    public const string ModifierView = "Modifier.View";
    public const string ModifierManage = "Modifier.Manage";
    public const string PricingView = "Pricing.View";
    public const string PricingEdit = "Pricing.Edit";
    public const string PricingBulkUpdate = "Pricing.BulkUpdate";
    public const string QrView = "QR.View";
    public const string QrCreate = "QR.Create";
    public const string QrEdit = "QR.Edit";
    public const string QrDeactivate = "QR.Deactivate";
    public const string UserView = "User.View";
    public const string UserCreate = "User.Create";
    public const string UserEdit = "User.Edit";
    public const string UserDeactivate = "User.Deactivate";
    public const string UserAssignPermissions = "User.AssignPermissions";
    public const string AuditView = "Audit.View";
    public const string AnalyticsView = "Analytics.View";
    public const string SubscriptionView = "Subscription.View";
    public const string SubscriptionManage = "Subscription.Manage";
    public const string OrdersView = "Orders.View";
    public const string OrdersAccept = "Orders.Accept";
    public const string OrdersPrepare = "Orders.Prepare";
    public const string OrdersReady = "Orders.Ready";
    public const string OrdersComplete = "Orders.Complete";
    public const string OrdersReject = "Orders.Reject";
    public const string OrdersCancel = "Orders.Cancel";

    public static IReadOnlyList<Definition> Definitions { get; } =
    [
        new(RestaurantView, "Restaurant", "View", "Restaurant", 1), new(RestaurantEdit, "Restaurant", "Edit", "Restaurant", 2),
        new(BranchView, "Branch", "View", "Branch", 10), new(BranchCreate, "Branch", "Create", "Branch", 11), new(BranchEdit, "Branch", "Edit", "Branch", 12), new(BranchDelete, "Branch", "Delete", "Branch", 13),
        new(MenuView, "Menu", "View", "Menu", 20), new(MenuCreate, "Menu", "Create", "Menu", 21), new(MenuEdit, "Menu", "Edit", "Menu", 22), new(MenuPublish, "Menu", "Publish", "Menu", 23),
        new(CategoryView, "Category", "View", "Category", 30), new(CategoryCreate, "Category", "Create", "Category", 31), new(CategoryEdit, "Category", "Edit", "Category", 32), new(CategoryDelete, "Category", "Delete", "Category", 33),
        new(ProductView, "Product", "View", "Product", 40), new(ProductCreate, "Product", "Create", "Product", 41), new(ProductEdit, "Product", "Edit", "Product", 42), new(ProductDelete, "Product", "Delete", "Product", 43), new(ProductImages, "Product", "Images", "Product", 44),
        new(IngredientView, "Ingredient", "View", "Ingredient", 50), new(IngredientManage, "Ingredient", "Manage", "Ingredient", 51),
        new(AllergenView, "Allergen", "View", "Allergen", 60), new(AllergenManage, "Allergen", "Manage", "Allergen", 61),
        new(ModifierView, "Modifier", "View", "Modifier", 70), new(ModifierManage, "Modifier", "Manage", "Modifier", 71),
        new(PricingView, "Pricing", "View", "Pricing", 80), new(PricingEdit, "Pricing", "Edit", "Pricing", 81), new(PricingBulkUpdate, "Pricing", "Bulk Update", "Pricing", 82),
        new(QrView, "QR", "View", "QR", 90), new(QrCreate, "QR", "Create", "QR", 91), new(QrEdit, "QR", "Edit", "QR", 92), new(QrDeactivate, "QR", "Deactivate", "QR", 93),
        new(UserView, "User", "View", "User", 100), new(UserCreate, "User", "Create", "User", 101), new(UserEdit, "User", "Edit", "User", 102), new(UserDeactivate, "User", "Deactivate", "User", 103), new(UserAssignPermissions, "User", "Assign Permissions", "User", 104),
        new(AuditView, "Audit", "View", "Audit", 110), new(AnalyticsView, "Analytics", "View", "Analytics", 120), new(SubscriptionView, "Subscription", "View", "Subscription", 130), new(SubscriptionManage, "Subscription", "Manage", "Subscription", 131),
        new(OrdersView, "Orders", "View", "\u0639\u0631\u0636 \u0627\u0644\u0637\u0644\u0628\u0627\u062a", 140), new(OrdersAccept, "Orders", "Accept", "\u0642\u0628\u0648\u0644 \u0627\u0644\u0637\u0644\u0628\u0627\u062a", 141), new(OrdersPrepare, "Orders", "Prepare", "\u062a\u062d\u0636\u064a\u0631 \u0627\u0644\u0637\u0644\u0628\u0627\u062a", 142), new(OrdersReady, "Orders", "Ready", "\u062c\u0627\u0647\u0632 \u0644\u0644\u062a\u0633\u0644\u064a\u0645", 143), new(OrdersComplete, "Orders", "Complete", "\u0625\u0643\u0645\u0627\u0644 \u0627\u0644\u0637\u0644\u0628\u0627\u062a", 144), new(OrdersReject, "Orders", "Reject", "\u0631\u0641\u0636 \u0627\u0644\u0637\u0644\u0628\u0627\u062a", 145), new(OrdersCancel, "Orders", "Cancel", "\u0625\u0644\u063a\u0627\u0621 \u0627\u0644\u0637\u0644\u0628\u0627\u062a", 146)
    ];

    public static IReadOnlyCollection<string> AllCodes => Definitions.Select(x => x.Code).ToArray();

    public static IReadOnlyCollection<string> Preset(MembershipRole role) => role switch
    {
        MembershipRole.PlatformAdmin or MembershipRole.TenantOwner or MembershipRole.TenantAdmin => AllCodes,
        MembershipRole.MenuEditor => Definitions.Where(x => x.GroupCode is "Restaurant" or "Menu" or "Category" or "Product" or "Ingredient" or "Allergen" or "Modifier" or "Pricing").Select(x => x.Code).ToArray(),
        MembershipRole.BranchManager => [RestaurantView, BranchView, MenuView, CategoryView, ProductView, ProductEdit, OrdersView, OrdersAccept, OrdersPrepare, OrdersReady, OrdersComplete, OrdersReject],
        MembershipRole.Kitchen => [RestaurantView, BranchView, MenuView, ProductView, OrdersView, OrdersAccept, OrdersPrepare, OrdersReady, OrdersReject, OrdersCancel],
        MembershipRole.Waiter => [RestaurantView, BranchView, MenuView, OrdersView, OrdersAccept, OrdersComplete, OrdersReject, OrdersCancel],
        _ => [RestaurantView, BranchView, MenuView, CategoryView, ProductView]
    };

    public sealed record Definition(string Code, string GroupCode, string NameEn, string NameAr, int SortOrder);
}
