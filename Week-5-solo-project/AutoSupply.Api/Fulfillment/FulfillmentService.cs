using AutoSupply.Data;
using AutoSupply.Data.Entities;
using AutoSupply.Data.Enums;
using Microsoft.EntityFrameworkCore;

namespace AutoSupply.Api.Fulfillment;

public class FulfillmentService : IFulfillmentService
{
    private readonly IDbContextFactory<AutoSupplyDbContext> _factory;


    public FulfillmentService(IDbContextFactory<AutoSupplyDbContext> factory)
    {
        _factory = factory;
    }



    // public async Task FulfillOrderAsync(int orderId, CancellationToken ct = default)
    // {
    //     await using var db = await _factory.CreateDbContextAsync(ct);

    //     var order = await db.Orders
    //         .Include(o => o.OrderLines)
    //         .ThenInclude(ol => ol.Product)
    //         .ThenInclude(p => p.InventoryItem)
    //         .FirstOrDefaultAsync(o => o.Id == orderId, ct);


    //     if(order is null)
    //     {
    //         return;
    //     }


    //     if(order.Status != OrderStatus.Pending)
    //     {
    //         return;
    //     }


    //     await using var transaction = await db.Database.BeginTransactionAsync(ct);

    //         var canFulfill = order.OrderLines.All(ol =>
    //             ol.Product.InventoryItem.QuantityOnHand >= ol.Quantity);

    //         if(canFulfill)
    //         {
    //             foreach(var line in order.OrderLines)
    //             {
    //                 line.Product.InventoryItem.QuantityOnHand -= line.Quantity;
    //             }

    //             order.Status = OrderStatus.Fulfilled;
    //             order.CompletedAt = DateTime.UtcNow;


    //             order.FulfillmentEvents.Add(new FulfillmentEvent
    //             {
    //                 Type = FulfillmentEventType.Fulfilled,
    //                 Message = "Order fulfilled from available inventory",
    //                 OccurredAt = DateTime.UtcNow
    //             });
    //         }
    //         else
    //         {
    //             order.Status = OrderStatus.Backordered;
    //             order.CompletedAt = DateTime.UtcNow;


    //             order.FulfillmentEvents.Add(new FulfillmentEvent
    //             {
    //                 Type = FulfillmentEventType.Backordered,
    //                 Message = "Order backordered due to insufficient stock in inventory",
    //                 OccurredAt = DateTime.UtcNow
    //             });
    //         }


    //         await db.SaveChangesAsync(ct);

    //     await transaction.CommitAsync(ct);



    // }

    public async Task FulfillOrderAsync(int orderId, CancellationToken ct = default)
    {
        const int maxAttempts = 3;

        for(var attempt = 1; attempt <= maxAttempts; attempt++)
        {
            try
            {
                await using var db = await _factory.CreateDbContextAsync(ct);

                var order = await db.Orders
                    .Include(o => o.OrderLines)
                    .ThenInclude(ol => ol.Product)
                    .ThenInclude(p => p.InventoryItem)
                    .FirstOrDefaultAsync(o => o.Id == orderId, ct);


                if(order is null)
                {
                    return;
                }


                if(order.Status != OrderStatus.Pending)
                {
                    return;
                }


                await using var transaction = await db.Database.BeginTransactionAsync(ct);

                var canFulfill = order.OrderLines.All(ol =>
                    ol.Product.InventoryItem.QuantityOnHand >= ol.Quantity);

                if(canFulfill)
                {
                    foreach(var line in order.OrderLines)
                    {
                        line.Product.InventoryItem.QuantityOnHand -= line.Quantity;
                    }

                    order.Status = OrderStatus.Fulfilled;
                    order.CompletedAt = DateTime.UtcNow;


                    order.FulfillmentEvents.Add(new FulfillmentEvent
                    {
                        Type = FulfillmentEventType.Fulfilled,
                        Message = "Order fulfilled from available inventory",
                        OccurredAt = DateTime.UtcNow
                    });
                }
                else
                {
                    order.Status = OrderStatus.Backordered;
                    order.CompletedAt = DateTime.UtcNow;


                    order.FulfillmentEvents.Add(new FulfillmentEvent
                    {
                        Type = FulfillmentEventType.Backordered,
                        Message = "Order backordered due to insufficient stock in inventory",
                        OccurredAt = DateTime.UtcNow
                    });
                }


                await db.SaveChangesAsync(ct);

                await transaction.CommitAsync(ct);




                return;
            }
            catch(DbUpdateConcurrencyException) when (attempt < maxAttempts)
            {
                // Retry with a fresh DbContext on the next loop iteration.
            }
            catch(DbUpdateConcurrencyException)
            {
                await MarkOrderFailedAsync(orderId, ct);
                return;
            }
        }
    }

    private async Task MarkOrderFailedAsync(int orderId, CancellationToken ct)
    {
        await using var db = await _factory.CreateDbContextAsync(ct);

        var order = await db.Orders
            .Include(o => o.FulfillmentEvents)
            .FirstOrDefaultAsync(o => o.Id == orderId, ct);

        if (order is null || order.Status != OrderStatus.Pending)
        {
            return;
        }

        order.Status = OrderStatus.Failed;
        order.CompletedAt = DateTime.UtcNow;

        order.FulfillmentEvents.Add(new FulfillmentEvent
        {
            Type = FulfillmentEventType.Failed,
            Message = "Order failed after repeated inventory concurrency conflicts.",
            OccurredAt = DateTime.UtcNow
        });

        await db.SaveChangesAsync(ct);
    }

    public async Task FulfillOrdersAsync(IReadOnlyList<int> orderIds, CancellationToken ct = default)
    {
        var tasks = orderIds.Select(orderId =>
            FulfillOrderAsync(orderId, ct));

        await Task.WhenAll(tasks);
    }
}
