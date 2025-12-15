namespace CakeShopApi.DTOs;
public record CakeCreateDto(string Name, string? Description, decimal Price, int? CategoryId, int StockQuantity, string? ImageUrl);
public record CakeUpdateDto(string? Name, string? Description, decimal? Price, int? CategoryId, int? StockQuantity, string? ImageUrl, bool? IsActive);
