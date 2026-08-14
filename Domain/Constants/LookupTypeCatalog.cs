namespace RestaurantMenuPlatform.Domain.Constants;

/// <summary>
/// Technical catalog of lookup identifiers supported by the application.
/// Display names and values remain database data; this catalog only prevents
/// an administrator from creating an unhandled configuration category.
/// </summary>
public static class LookupTypeCatalog
{
    public static bool IsSupported(string? code) => Normalize(code) switch
    {
        LookupTypes.Currency or
        LookupTypes.Language or
        LookupTypes.PricingOperation or
        LookupTypes.PricingScope or
        LookupTypes.MenuType or
        LookupTypes.MenuScope or
        LookupTypes.CategoryType or
        LookupTypes.ProductType => true,
        _ => false
    };

    public static bool IsGlobal(string? code) => Normalize(code) is
        LookupTypes.Currency or LookupTypes.Language;

    public static bool IsTenantManaged(string? code) =>
        IsSupported(code) && !IsGlobal(code);

    public static bool IsValueCodeAllowed(string? type, string? code)
    {
        var normalizedType = Normalize(type);
        var normalizedCode = code?.Trim().ToUpperInvariant() ?? string.Empty;
        return normalizedType switch
        {
            LookupTypes.PricingOperation => normalizedCode is
                PricingLookupCodes.PercentageIncrease or
                PricingLookupCodes.PercentageDecrease or
                PricingLookupCodes.FixedIncrease or
                PricingLookupCodes.FixedDecrease or
                PricingLookupCodes.SetExact,
            LookupTypes.PricingScope => normalizedCode is
                PricingLookupCodes.Product or PricingLookupCodes.Category or PricingLookupCodes.Branch,
            LookupTypes.MenuScope => normalizedCode is
                MenuLookupCodes.AllBranches or MenuLookupCodes.SelectedBranches,
            LookupTypes.Currency or LookupTypes.Language => false,
            _ => IsTenantManaged(normalizedType)
        };
    }

    public static bool IsGlobalValueCodeAllowed(string? type, string? code)
    {
        var normalizedType = Normalize(type);
        var normalizedCode = code?.Trim().ToUpperInvariant() ?? string.Empty;
        return normalizedType switch
        {
            LookupTypes.Currency => normalizedCode.Length == 3 && normalizedCode.All(char.IsLetter),
            LookupTypes.Language => normalizedCode.Length is >= 2 and <= 10 &&
                                    normalizedCode.All(character => char.IsLetter(character) || character == '-'),
            _ => false
        };
    }

    public static string Normalize(string? code) => code?.Trim() ?? string.Empty;
}
