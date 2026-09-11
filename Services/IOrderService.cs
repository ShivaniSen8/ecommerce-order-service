using OrderService.DTOs;

namespace OrderService.Services;

public interface IOrderService
{
    Task<OrderResponse> CreateOrderAsync(
        int userId,
        CreateOrderRequest request);

    Task<List<OrderResponse>> GetUserOrdersAsync(
        int userId);

    Task<OrderResponse?> GetOrderByIdAsync(
        Guid orderId,
        int userId);

    Task<OrderResponse?> CancelOrderAsync(
        Guid orderId,
        int userId);

    Task<OrderResponse?> UpdateOrderStatusAsync(
        Guid orderId,
        string status);
}
