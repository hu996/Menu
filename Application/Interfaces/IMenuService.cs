using RestaurantMenuPlatform.Application.DTOs;

namespace RestaurantMenuPlatform.Application.Interfaces;

public interface IMenuService
{
    Task<IReadOnlyList<MenuListDto>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<MenuDetailsDto?> GetAsync(Guid id, CancellationToken cancellationToken = default);
    Task<MenuListDto> CreateAsync(MenuInput input, CancellationToken cancellationToken = default);
    Task<MenuListDto?> UpdateAsync(Guid id, MenuInput input, CancellationToken cancellationToken = default);
    Task<MenuListDto?> SetStatusAsync(Guid id, RestaurantMenuPlatform.Domain.Enums.MenuStatus status, CancellationToken cancellationToken = default);
    Task<MenuCategoryDto?> CreateCategoryAsync(Guid menuId, MenuCategoryInput input, CancellationToken cancellationToken = default);
    Task<MenuCategoryDto?> UpdateCategoryAsync(Guid categoryId, MenuCategoryInput input, CancellationToken cancellationToken = default);
    Task<bool> MoveCategoryAsync(Guid categoryId, bool moveUp, CancellationToken cancellationToken = default);
    Task<MenuItemDto?> CreateItemAsync(Guid categoryId, MenuItemInput input, CancellationToken cancellationToken = default);
    Task<MenuItemDto?> GetItemAsync(Guid itemId, CancellationToken cancellationToken = default);
    Task<MenuItemDto?> UpdateItemAsync(Guid itemId, MenuItemInput input, CancellationToken cancellationToken = default);
    Task<bool> SetItemAvailabilityAsync(Guid itemId, bool isAvailable, CancellationToken cancellationToken = default);
    Task<bool> MoveItemAsync(Guid itemId, bool moveUp, CancellationToken cancellationToken = default);
}
