using AutoSupply.Data.Enums;

namespace AutoSupply.Data.Entities;


public class Order
{
    public int Id { get; set; }

    public int CustomerId { get; set; } //FK Customer id
    public Customer Customer { get; set; } = default!;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? CompletedAt { get; set; }
    public OrderStatus Status { get; set; } = OrderStatus.Pending;
    public Priority Priority { get; set; } = Priority.Standard;


    //pending define list of relation ship of Order with OrderLines

    //db schema relationship - 1:N - an Order can have many FulfillmentEvents, a FulfillmentEvent belongs to an order
    public List<FulfillmentEvent> FulfillmentEvents { get; set; } = new();

    //db schema relationship - 1:N - an Order can have many OrderLines
    public List<OrderLine> OrderLines { get; set; } = new();


}

