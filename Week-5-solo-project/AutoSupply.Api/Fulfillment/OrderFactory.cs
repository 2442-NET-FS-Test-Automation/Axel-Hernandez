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
            Priority = expedited ? Priority.Expedited : Priority.Standard,
            Status = OrderStatus.Pending,
            CreatedAt = DateTime.UtcNow,
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

