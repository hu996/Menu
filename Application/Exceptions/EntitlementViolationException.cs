namespace RestaurantMenuPlatform.Application.Exceptions;

public sealed class EntitlementViolationException : InvalidOperationException
{
    public EntitlementViolationException(string message, string entitlementKey, int? limit = null, int? usage = null)
        : base(message)
    {
        EntitlementKey = entitlementKey;
        Limit = limit;
        Usage = usage;
    }

    public string EntitlementKey { get; }
    public int? Limit { get; }
    public int? Usage { get; }
}
