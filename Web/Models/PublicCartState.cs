using System.Text.Json;
using Microsoft.AspNetCore.Http;
using RestaurantMenuPlatform.Application.DTOs;

namespace RestaurantMenuPlatform.Web.Models;

public sealed class PublicCartState
{
    public Guid BranchId { get; set; }
    public Guid? MenuId { get; set; }
    public Guid? TableId { get; set; }
    public Guid? QrCodeId { get; set; }
    public string? QrCodeCode { get; set; }
    public string? TableName { get; set; }
    public string? TableNameAr { get; set; }
    public List<CartLineInput> Lines { get; set; } = [];
}

public static class PublicCartSession
{
    public const string Key = "public-order-cart";

    public static PublicCartState Read(ISession session)
    {
        var json = session.GetString(Key);
        if (string.IsNullOrWhiteSpace(json))
            return new PublicCartState();

        try
        {
            return JsonSerializer.Deserialize<PublicCartState>(json) ?? new PublicCartState();
        }
        catch (JsonException)
        {
            session.Remove(Key);
            return new PublicCartState();
        }
    }

    public static void Write(ISession session, PublicCartState state) =>
        session.SetString(Key, JsonSerializer.Serialize(state));

    public static string BuildKey(Guid itemId, IReadOnlyList<Guid> modifierOptionIds) =>
        $"{itemId:N}:{string.Join(',', modifierOptionIds.Distinct().OrderBy(x => x).Select(x => x.ToString("N")))}";
}
