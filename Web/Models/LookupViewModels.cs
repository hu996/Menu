using System.ComponentModel.DataAnnotations;
using RestaurantMenuPlatform.Application.DTOs;

namespace RestaurantMenuPlatform.Web.Models;

public sealed class LookupIndexViewModel
{
    public LookupValuePageDto Page { get; set; } = null!;
    public IReadOnlyList<LookupTypeDto> Types { get; set; } = Array.Empty<LookupTypeDto>();
}

public sealed class LookupValueViewModel
{
    public Guid Id { get; set; }

    [Required, StringLength(64, MinimumLength = 2)]
    public string Type { get; set; } = string.Empty;

    [Required, StringLength(64, MinimumLength = 1)]
    public string Code { get; set; } = string.Empty;

    [Required, StringLength(160, MinimumLength = 2)]
    [Display(Name = "English name")]
    public string NameEn { get; set; } = string.Empty;

    [StringLength(160)]
    [Display(Name = "Arabic name")]
    public string? NameAr { get; set; }

    [StringLength(500)]
    public string? Description { get; set; }

    [Range(0, 10000)]
    public int SortOrder { get; set; }

    public bool IsActive { get; set; } = true;
    public bool IsGlobal { get; set; }
    public IReadOnlyList<LookupTypeDto> Types { get; set; } = Array.Empty<LookupTypeDto>();
}

public sealed class LookupTypeIndexViewModel
{
    public LookupTypePageDto Page { get; set; } = null!;
}

public sealed class LookupTypeViewModel
{
    public Guid Id { get; set; }

    [Required, StringLength(64, MinimumLength = 2)]
    [Display(Name = "Type code")]
    public string Code { get; set; } = string.Empty;

    [Required, StringLength(160, MinimumLength = 2)]
    [Display(Name = "English name")]
    public string NameEn { get; set; } = string.Empty;

    [StringLength(160)]
    [Display(Name = "Arabic name")]
    public string? NameAr { get; set; }

    [StringLength(500)]
    public string? Description { get; set; }

    [Range(0, 10000)]
    public int SortOrder { get; set; }

    public bool IsActive { get; set; } = true;
    public bool IsGlobal { get; set; }
    public int ValueCount { get; set; }
}
