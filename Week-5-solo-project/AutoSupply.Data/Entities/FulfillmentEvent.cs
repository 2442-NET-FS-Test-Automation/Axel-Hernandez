using AutoSupply.Data.Enums;

namespace AutoSupply.Data.Entities;

public class FulfillmentEvent
{
    public int Id { get; set; }
    public int OrderId { get; set; }
    public Order Order { get; set; } = default!;
    public FulfillmentEventType Type { get; set; }
    public string Message { get; set; } = default!;

    public DateTime OccurredAt { get; set; } = DateTime.UtcNow;


    
}