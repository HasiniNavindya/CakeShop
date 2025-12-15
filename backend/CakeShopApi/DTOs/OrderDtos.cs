namespace CakeShopApi.DTOs;
public record OrderItemDto(int CakeId, int Quantity);
public record CreateOrderDto(List<OrderItemDto> Items, string Address);
