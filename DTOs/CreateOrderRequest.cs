namespace OrderService.DTOs;

public class CreateOrderRequest
{
    public string ShippingAddress { get; set; } = string.Empty;
    public List<CreateOrderItemRequest> Items { get; set; } = new();
}