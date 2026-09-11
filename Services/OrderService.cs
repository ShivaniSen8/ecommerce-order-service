
using OrderService.DTOs;
using OrderService.Entities;
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
        int userId,
        CreateOrderRequest request)
    {
        if (request.Items == null || !request.Items.Any())
        {
            throw new ArgumentException("Order must contain at least one item.");
        }

        var order = new Order
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            Status = "Pending",
            ShippingAddress = request.ShippingAddress,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow,
            OrderItems = new List<OrderItem>()
        };

        decimal totalAmount = 0;

        foreach (var item in request.Items)
        {
            if (item.Quantity <= 0)
            {
                throw new ArgumentException(
                    "Product quantity must be greater than zero.");
            }

            // For now the price comes from the request.
            // Later we will get the price from Product Service.
            var orderItem = new OrderItem
            {
                Id = Guid.NewGuid(),
                OrderId = order.Id,
                ProductId = item.ProductId,
                Quantity = item.Quantity,
                UnitPrice = item.UnitPrice,
                TotalPrice = item.UnitPrice * item.Quantity
            };

            totalAmount += orderItem.TotalPrice;

            order.OrderItems.Add(orderItem);
        }

        order.TotalAmount = totalAmount;

        await _orderRepository.CreateAsync(order);

        return MapToResponse(order);
    }

    public async Task<List<OrderResponse>> GetUserOrdersAsync(int userId)
    {
        var orders = await _orderRepository.GetByUserIdAsync(userId);

        return orders
            .Select(MapToResponse)
            .ToList();
    }

    public async Task<OrderResponse?> GetOrderByIdAsync(
        Guid orderId,
        int userId)
    {
        var order = await _orderRepository.GetByIdAsync(orderId);

        if (order == null)
        {
            return null;
        }

        // User can only see their own order.
        if (order.UserId != userId)
        {
            return null;
        }

        return MapToResponse(order);
    }

    public async Task<OrderResponse?> CancelOrderAsync(
        Guid orderId,
        int userId)
    {
        var order = await _orderRepository.GetByIdAsync(orderId);

        if (order == null)
        {
            return null;
        }

        if (order.UserId != userId)
        {
            return null;
        }

        if (order.Status == "Cancelled")
        {
            throw new InvalidOperationException(
                "Order is already cancelled.");
        }

        if (order.Status == "Shipped")
        {
            throw new InvalidOperationException(
                "Shipped orders cannot be cancelled.");
        }

        if (order.Status == "Delivered")
        {
            throw new InvalidOperationException(
                "Delivered orders cannot be cancelled.");
        }

        order.Status = "Cancelled";
        order.UpdatedAt = DateTime.UtcNow;

        await _orderRepository.UpdateAsync(order);

        return MapToResponse(order);
    }

    public async Task<OrderResponse?> UpdateOrderStatusAsync(
        Guid orderId,
        string status)
    {
        var order = await _orderRepository.GetByIdAsync(orderId);

        if (order == null)
        {
            return null;
        }

        if (string.IsNullOrWhiteSpace(status))
        {
            throw new ArgumentException(
                "Order status is required.");
        }

        var validStatuses = new[]
        {
            "Pending",
            "Confirmed",
            "Shipped",
            "Delivered",
            "Cancelled"
        };

        if (!validStatuses.Contains(
                status,
                StringComparer.OrdinalIgnoreCase))
        {
            throw new ArgumentException(
                $"Invalid order status: {status}");
        }

        if (order.Status == "Cancelled" &&
            !status.Equals(
                "Cancelled",
                StringComparison.OrdinalIgnoreCase))
        {
            throw new InvalidOperationException(
                "Cancelled order status cannot be changed.");
        }

        order.Status = status;
        order.UpdatedAt = DateTime.UtcNow;

        await _orderRepository.UpdateAsync(order);

        return MapToResponse(order);
    }

    private static OrderResponse MapToResponse(Order order)
    {
        return new OrderResponse
        {
            Id = order.Id,
            UserId = order.UserId,
            Status = order.Status,
            TotalAmount = order.TotalAmount,
            ShippingAddress = order.ShippingAddress,
            CreatedAt = order.CreatedAt,
            UpdatedAt = order.UpdatedAt,
            Items = order.OrderItems
                .Select(item => new OrderItemResponse
                {
                    Id = item.Id,
                    ProductId = item.ProductId,
                    Quantity = item.Quantity,
                    UnitPrice = item.UnitPrice,
                    TotalPrice = item.TotalPrice
                })
                .ToList()
        };
    }
}
