using OrderService.Models;

namespace OrderService.Repositories;

public interface IOrderRepository
{
    Task<Order> CreateAsync(Order order);

    Task<Order?> GetByIdAsync(Guid id);

    Task<List<Order>> GetByUserIdAsync(Guid userId);

    Task UpdateAsync(Order order);
}