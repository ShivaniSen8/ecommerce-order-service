using OrderService.DTOs;

namespace OrderService.Services;

public interface IOrderService
{
    Task<OrderResponse> CreateOrderAsync(
        Guid userId,
        CreateOrderRequest request);

    Task<OrderResponse?> GetOrderByIdAsync(
        Guid orderId,
        Guid userId);

    Task<List<OrderResponse>> GetUserOrdersAsync(
        Guid userId);
}