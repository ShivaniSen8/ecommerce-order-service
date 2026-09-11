using OrderService.Entities;

namespace OrderService.Repositories;

public interface IOrderRepository
{
    Task<Order> CreateAsync(Order order);

    Task<Order?> GetByIdAsync(Guid id);

    Task<List<Order>> GetByUserIdAsync(int userId);

    Task UpdateAsync(Order order);
}