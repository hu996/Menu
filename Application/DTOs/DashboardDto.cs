namespace RestaurantMenuPlatform.Application.DTOs;

public sealed record DashboardDto(
    string RestaurantName,
    int Branches,
    int Menus,
    int MenuItems,
    int ActiveQrCodes,
    int PendingOrders,
    int PublishedMenus,
    int DraftMenus,
    EntitlementDto? Entitlements,
    AnalyticsSummaryDto Analytics,
    OnboardingProgressDto Onboarding,
    bool AnalyticsEnabled,
    string? RestaurantNameAr = null,
    IReadOnlyList<DashboardAttentionDto>? Attention = null,
    IReadOnlyList<DashboardActivityDto>? RecentActivity = null);

public sealed record DashboardAttentionDto(
    string Key,
    string Title,
    string Description,
    string Controller,
    string Action,
    string Tone,
    string? RequiredPermission = null);

public sealed record DashboardActivityDto(
    string Action,
    string EntityType,
    DateTime CreatedAtUtc,
    string? ActorDisplayName,
    string Label);

public sealed record OnboardingProgressDto(
    bool RestaurantConfigured,
    bool FirstBranchCreated,
    bool MenuCreated,
    bool CategoriesCreated,
    bool ProductsCreated,
    bool MenuPublished,
    bool QrCreated)
{
    public int TotalSteps => 7;
    public int CompletedSteps => new[]
    {
        RestaurantConfigured,
        FirstBranchCreated,
        MenuCreated,
        CategoriesCreated,
        ProductsCreated,
        MenuPublished,
        QrCreated
    }.Count(x => x);

    public bool IsComplete => CompletedSteps == TotalSteps;
    public string NextStepTitle => !RestaurantConfigured ? "Complete restaurant identity" : !FirstBranchCreated ? "Create your first branch" : !MenuCreated ? "Create your first menu" : !CategoriesCreated ? "Create your first category" : !ProductsCreated ? "Add your first product" : !MenuPublished ? "Preview and publish your menu" : !QrCreated ? "Create your first QR code" : "Your restaurant is ready for daily management";
    public string NextStepDescription => !RestaurantConfigured ? "Review the restaurant settings before building the guest experience." : !FirstBranchCreated ? "Branches are saved independently and can be added whenever your restaurant grows." : !MenuCreated ? "Create a menu before adding categories and products to it." : !CategoriesCreated ? "Organize the menu into categories before adding products." : !ProductsCreated ? "Add a product inside a menu category, then enrich its content separately." : !MenuPublished ? "Preview the saved menu and publish it when the guest experience is ready." : !QrCreated ? "Create a QR code so guests can enter the published experience." : "Use the workspace normally; setup progress will keep reflecting saved business state.";
    public string NextStepLabel => !RestaurantConfigured ? "Review restaurant" : !FirstBranchCreated ? "Add branch" : !MenuCreated ? "Create menu" : !CategoriesCreated ? "Open menus" : !ProductsCreated ? "Open products" : !MenuPublished ? "Open menus" : !QrCreated ? "Create QR code" : "Explore workspace";
    public string NextStepController => !RestaurantConfigured ? "Restaurant" : !FirstBranchCreated ? "Branches" : !MenuCreated ? "Menus" : !CategoriesCreated ? "Menus" : !ProductsCreated ? "Products" : !MenuPublished ? "Menus" : !QrCreated ? "QrCodes" : "Dashboard";
    public string NextStepAction => !RestaurantConfigured ? "Index" : !FirstBranchCreated ? "Create" : !MenuCreated ? "Create" : !CategoriesCreated ? "Index" : !ProductsCreated ? "Index" : !MenuPublished ? "Index" : !QrCreated ? "Index" : "Index";
}
