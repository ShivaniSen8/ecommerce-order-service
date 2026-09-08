using OrderService.DTOs;
using OrderService.Models;
using OrderService.Repositories;

namespace OrderService.Services;

public class OrderService : IOrderService
{
    private readonly IOrderRepository _orderRepository;

    public OrderService(IOrderRepository orderRepository)
    {
        _orderRepository = orderRepository;
    }

    public async Task<OrderResponse> CreateOrderAsync(
        Guid userId,
        CreateOrderRequest request)
    {
        if (request.Items == null || request.Items.Count == 0)
        {
            throw new ArgumentException(
                "Order must contain at least one item.");
        }

        var order = new Order
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            Status = "Pending",
            CreatedAt = DateTime.UtcNow
        };

        decimal totalAmount = 0;

        foreach (var itemRequest in request.Items)
        {
            if (itemRequest.Quantity <= 0)
            {
                throw new ArgumentException(
                    "Item quantity must be greater than zero.");
            }

            // TEMPORARY
            // Later this price will come from Product Service.
            decimal unitPrice = 1000;

            var orderItem = new OrderItem
            {
                Id = Guid.NewGuid(),

                ProductId = itemRequest.ProductId,

                Quantity = itemRequest.Quantity,

                UnitPrice = unitPrice,

                TotalPrice = unitPrice * itemRequest.Quantity
            };

            order.OrderItems.Add(orderItem);

            totalAmount += orderItem.TotalPrice;
        }

        order.TotalAmount = totalAmount;

        var createdOrder =
            await _orderRepository.CreateAsync(order);

        return MapToResponse(createdOrder);
    }

    public async Task<OrderResponse?> GetOrderByIdAsync(
        Guid orderId,
        Guid userId)
    {
        var order =
            await _orderRepository.GetByIdAsync(orderId);

        if (order == null)
        {
            return null;
        }

        // Prevent one customer from accessing
        // another customer's order.
        if (order.UserId != userId)
        {
            return null;
        }

        return MapToResponse(order);
    }

    public async Task<List<OrderResponse>> GetUserOrdersAsync(
        Guid userId)
    {
        var orders =
            await _orderRepository.GetByUserIdAsync(userId);

        return orders
            .Select(MapToResponse)
            .ToList();
    }

    private static OrderResponse MapToResponse(Order order)
    {
        return new OrderResponse
        {
            Id = order.Id,

            UserId = order.UserId,

            TotalAmount = order.TotalAmount,

            Status = order.Status,

            CreatedAt = order.CreatedAt,

            Items = order.OrderItems
                .Select(item => new OrderItemResponse
                {
                    ProductId = item.ProductId,

                    Quantity = item.Quantity,

                    UnitPrice = item.UnitPrice,

                    TotalPrice = item.TotalPrice
                })
                .ToList()
        };
    }
}