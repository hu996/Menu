namespace RestaurantMenuPlatform.Domain.Constants;

public static class LookupTypes
{
    public const string Currency = "Currency";
    public const string PricingOperation = "PricingOperation";
    public const string PricingScope = "PricingScope";
    public const string MenuType = "MenuType";
    public const string MenuScope = "MenuScope";
    public const string CategoryType = "CategoryType";
    public const string ProductType = "ProductType";
    public const string Language = "Language";
}

public static class PricingLookupCodes
{
    public const string PercentageIncrease = "PERCENTAGE_INCREASE";
    public const string PercentageDecrease = "PERCENTAGE_DECREASE";
    public const string FixedIncrease = "FIXED_INCREASE";
    public const string FixedDecrease = "FIXED_DECREASE";
    public const string SetExact = "SET_EXACT";
    public const string Product = "PRODUCT";
    public const string Category = "CATEGORY";
    public const string Branch = "BRANCH";
}

public static class MenuLookupCodes
{
    public const string AllBranches = "ALL_BRANCHES";
    public const string SelectedBranches = "SELECTED_BRANCHES";
}
