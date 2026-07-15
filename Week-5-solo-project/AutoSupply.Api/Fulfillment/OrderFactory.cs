using AutoSupply.Data.Entities;
using AutoSupply.Data.Enums;

namespace AutoSupply.Api.Fulfillment;

public class OrderFactory
{
    public Order CreateOrder(
        int customerId,
        int productId,
        int quantity,
        bool expedited
    )
    {
        return new Order
        {
            CustomerId = customerId,
            CreatedAt = DateTime.UtcNow,
            // CompletedAt = DateTime.UtcNow,
            CompletedAt = null,
            Status = OrderStatus.Pending,
            Priority = expedited ? Priority.Expedited : Priority.Standard,
            OrderLines = 
            {
                new OrderLine
                {
                    ProductId = productId,
                    Quantity = quantity
                }
            }
        };
    }
}

