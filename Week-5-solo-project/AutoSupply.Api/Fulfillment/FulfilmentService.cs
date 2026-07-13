using AutoSupply.Api.Fulfillment;
using AutoSupply.Data;
using AutoSupply.Data.Entities;
using AutoSupply.Data.Enums;
using Microsoft.EntityFrameworkCore;

namespace AutoSupply.Api.Fulfillment;


public class FulfillmentService : IFulfillmentService 
{
    public readonly IDbContextFactory<AutoSupplyDbContext> _factory = default!;


    public FulfillmentService(IDbContextFactory<AutoSupplyDbContext> factory)
    {
        _factory = factory;
    }





    public async Task<FulfillmentResult> FulfillOrderAsync(int orderId, CancellationToken ct)
    {
        await using var db = await _factory.CreateDbContextAsync(ct);

        var order = await db.Orders
            .Include(o => o.OrderLines)
            .FirstAsync(o => o.Id == orderId, ct);
        
        var requested = order.OrderLines.ToDictionary(ol => ol.ProductId, ol => ol.Quantity); //pedning to check why...

        bool canFulfill = true;



        foreach(OrderLine orderLine in order.OrderLines)
        {
            InventoryItem inventory = await db.InventoryItems.FirstAsync(i => i.ProductId == orderLine.ProductId, ct);

            //validate if enough invetory stock
            if(inventory.QuantityOnHand < orderLine.Quantity)
            {
                canFulfill = false;
                break;
            }

            //if enough, will jump to here, and decrement inventory stock
            inventory.QuantityOnHand -= orderLine.Quantity;
        }


        if(!canFulfill)
        {
            order.Status = OrderStatus.Backordered;
            db.FulfillmentEvents.Add(new FulfillmentEvent {
                OrderId = orderId,
                Type = FulfillmentEventType.Backordered,
                Message = $"Order {orderId} is backordered due to insufficiente stock"
            });
            await db.SaveChangesAsync(ct);

            //LOG SERILOG PENDING..
            return FulfillmentResult.Backordered;
        }

        order.Status = OrderStatus.Fulfilled;
        order.CompletedAt = DateTime.UtcNow;
        db.FulfillmentEvents.Add(new FulfillmentEvent {
            OrderId = orderId,
            Type = FulfillmentEventType.Fulfilled,
            Message = $"Order {orderId} has been fulfilled"
        });

        

        //Adding retry save method
        if(!await SaveWithRetryAsync(db, requested, ct))
        {
            db.ChangeTracker.Clear();
            Order staleOrder = await db.Orders.FirstAsync(o => o.Id == orderId, ct);
            staleOrder.Status = OrderStatus.Backordered;

            await db.SaveChangesAsync(ct);
            return FulfillmentResult.Backordered;
        }

        

        //LOG SERILOG PENDING..
        return FulfillmentResult.Fulfilled;


    }




    // Retry and save method
    private static async Task<bool> SaveWithRetryAsync(
        AutoSupplyDbContext db,
        IReadOnlyDictionary<int, int> requestedByProductId,
        CancellationToken ct
    )
    {
        while(true)
        {
            try{
                await db.SaveChangesAsync(ct);
                return true;
            }
            catch(DbUpdateConcurrencyException ex)
            {
                //LOG SERILOG PENDING...
                //attempt retry
                foreach(var entry in ex.Entries)
                {
                    var current = await entry.GetDatabaseValuesAsync(ct);

                    if(current == null) return false;

                    entry.OriginalValues.SetValues(current);


                    if(entry.Entity is InventoryItem inventory)
                    {
                        int freshValue = current.GetValue<int>(nameof (InventoryItem.QuantityOnHand));

                        int desiredAmount = requestedByProductId[inventory.ProductId];


                        if(freshValue < desiredAmount) return false;
                        inventory.QuantityOnHand = freshValue - desiredAmount;
                    }
                }
            }
        }
    }
}