namespace OrderService.DTOs;

public class OrderResponse { 
    public Guid Id { get; set; } 
public int UserId { get; set; }
public string Status { get; set; } = string.Empty; 
public decimal TotalAmount { get; set; } 
public string ShippingAddress { get; set; } = string.Empty; 
public DateTime CreatedAt { get; set; } 
public DateTime? UpdatedAt { get; set; } 
public List<OrderItemResponse> Items { get; set; } = new(); 

}