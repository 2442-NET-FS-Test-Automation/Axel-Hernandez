using Serilog;
using System.Diagnostics;
using Microsoft.EntityFrameworkCore;
using AutoSupply.Data.Entities;
using AutoSupply.Data.Enums;
using AutoSupply.Data;
using AutoSupply.Api.Exceptions;
using AutoSupply.Api.Seed;
using AutoSupply.Api.Fulfillment;
using AutoSupply.Api.Contracts;



var builder = WebApplication.CreateBuilder(args);
var conn_string = "Server=localhost,1434;Database=AutoSupplyDb;User Id=sa;Password=TestPass1!;TrustServerCertificate=true";

Log.Logger = new LoggerConfiguration()
    .WriteTo.Console()
    .WriteTo.File("logs/fulfillment-log.log", rollingInterval: RollingInterval.Day)
    .CreateLogger();

builder.Services.AddDbContext<AutoSupplyDbContext>(options => options.UseSqlServer(conn_string),
        ServiceLifetime.Scoped, ServiceLifetime.Singleton);
builder.Services.AddDbContextFactory<AutoSupplyDbContext>(options => options.UseSqlServer(conn_string));


// /Seed -----------------------------
builder.Services.AddScoped<ISeeder, Seeder>();

// /Fulfillment -----------------------------
builder.Services.AddScoped<OrderFactory>();
builder.Services.AddScoped<IFulfillmentService, FulfillmentService>();
builder.Services.AddScoped<BurstPlanner>();

//Repositories -----------------------------
builder.Services.AddScoped<IOrderRepository, OrderRepository>();


var app = builder.Build();




app.MapGet("/", () => "Testing another thing");




//seed endpoint
app.MapPost("/seed", async (ISeeder seeder, CancellationToken ct) => {
    var result = await seeder.SeedCatalogAsync(ct);

    return Results.Ok(new {
        message = "Seed completed",
        result.CategoriesCreated,
        result.ProductsCreated,
        result.CustomersCreated
    });
});



//inventory endpoint
// app.MapGet("/inventory", async (AutoSupplyDbContext db, CancellationToken ct) => {
//     var inventory = await db.InventoryItems
//         .Select(i => new {
//             Sku =i.Product.Sku,
//             ProductName = i.Product.Name,
//             CategoryName = i.Product.Category.CategoryName,
//             QuantityOnHand = i.QuantityOnHand,
//             Price = i.Product.Price
//         })
//         .ToListAsync(ct);

//         return Results.Ok(inventory);
// });


app.MapGet("/inventory", async (IOrderRepository orderRepository, CancellationToken ct) => {
    var inventory = await orderRepository.GetInventoryAsync(ct);
        
        return Results.Ok(new {
            inventoryItems = inventory.Select(item => new {
                item.ProductId,
                item.Sku,
                item.Name,
                item.QuantityOnHand
            })
        });
});





app.MapPost("/orders/fulfill/{orderId}", async (int orderId, IFulfillmentService fulfillmentService, CancellationToken ct) =>
{
    var result = await fulfillmentService.FulfillOrderAsync(orderId, ct);
    return Results.Ok(new {
        message = result == FulfillmentResult.Fulfilled ? "Order fulfilled" : "Order backordered",
        result = result.ToString()
    });
});





app.MapPost("/orders/burst", async (
    BurstOrderRequest request, 
    IServiceScopeFactory scopes, 
    IHostApplicationLifetime lifetime,
    AutoSupplyDbContext db,
    IOrderRepository orderRepository, 
    OrderFactory orderFactory, 
    CancellationToken ct) =>
{
    if(request.Count <= 0)
    {
        return Results.BadRequest("Count must be greater than zero");
    }

    var (customerIds, productIds) = await orderRepository.GetCustomerAndProductIdsAsync(ct);

    // var customerIdsList = customerIds.ToList();
    // var productIdsList = productIds.ToList();

    // var customerIds = await db.Customers
    //     .Select(c => c.Id)
    //     .ToListAsync(ct);
    
    // var productIds = await db.Products
    //     .Select(p => p.Id)
    //     .ToListAsync(ct);
        
    if(customerIds.Count == 0 || productIds.Count == 0)
    {
        return Results.BadRequest("Seed the catalog before creating orders");
    }

    var orders = new List<Order>(request.Count);

    for(var i = 0; i < request.Count; i++)
    {
        var customerId = customerIds[i % customerIds.Count];
        var productId = productIds[i % productIds.Count];

        var order = orderFactory.CreateOrder(
            customerId,
            productId,
            quantity: 1,
            expedited: request.Expedited
        );

        orders.Add(order);
    }

    db.Orders.AddRange(orders); //add the whole list of orders into dbset Orders
    await db.SaveChangesAsync(ct);


    // --- create fulfillment plan process ---
    var ids = orders.Select(o => o.Id).ToList();
    var appStopping = lifetime.ApplicationStopping;



    _ = Task.Run(async () =>
    {
        try
        {
            using var scope = scopes.CreateScope();
            var service = scope.ServiceProvider.GetRequiredService<IFulfillmentService>();
            await service.FulfillBurstAsync(ids, appStopping);
        }
        catch(Exception ex)
        {
            
            //LOG SERILOG PENDING...
            Log.Error(ex, "Burst fulfillment process failed");
        }
    }, appStopping);



    return Results.Accepted($"/orders", new
    {
        message = "Orders created and queued for fulfillment",
        OrdersCreated = orders.Count,
        request.Expedited
    });


});





app.MapGet("/orders", async (AutoSupplyDbContext db, CancellationToken ct) =>
{
    var orders = await db.Orders
        .OrderByDescending(o => o.CreatedAt)
        .Select(o => new {
            OrderId = o.Id,
            CustomerName = $"{o.Customer.FirstName} {o.Customer.LastName}",
            Priority = o.Priority.ToString(),
            Status = o.Status.ToString(),
            CreatedAt = o.CreatedAt,
            Lines = o.OrderLines.Select(ol => new {
                Sku = ol.Product.Sku,
                ProductName = ol.Product.Name,
                Quantity = ol.Quantity
            })

        }).ToListAsync(ct);

        return Results.Ok(orders);
});


app.MapGet("/orders/{id}", async (int id, AutoSupplyDbContext db, CancellationToken ct) =>
{
   var orderById = await db.Orders
    .Where(o => o.Id == id)
    .Select(o => new {
        OrderId = o.Id,
        CustomerName = $"{o.Customer.FirstName} {o.Customer.LastName}",
        Priority = o.Priority.ToString(),
        Status = o.Status.ToString(),
        CreatedAt = o.CreatedAt,
        Lines = o.OrderLines.Select(ol => new {
            Sku = ol.Product.Sku,
            ProductName = ol.Product.Name,
            Quantity = ol.Quantity
        })

    }).FirstOrDefaultAsync(ct);

    if(orderById is null)
    {
        return Results.NotFound();
    }


    return Results.Ok(orderById);
});





app.MapGet("/reports/completed-orders", async (AutoSupplyDbContext db, CancellationToken ct) =>
{
    // divide later into these endpoints for reports:
    // /reports/top-products
    // /reports/top-customers
    // /reports/fulfillment-rate
    var completedOrders = await db.Orders
        .Where(order => order.Status == OrderStatus.Fulfilled)
        .ToListAsync(ct);

    int count = completedOrders.Count;


    return Results.Ok(new {
        message = "Completed orders:",
        count
    });

});


app.MapGet("/reports/top-products", async (AutoSupplyDbContext db, CancellationToken ct) => {
    var ranked = await db.FulfillmentEvents
        .Where(e => e.Type == FulfillmentEventType.Fulfilled)
        .Join(
            db.OrderLines,
            e => e.OrderId,
            line => line.OrderId,
            (e, line) => line)
        .GroupBy(line => line.ProductId)
        .Select(g => new
        {
            ProductId = g.Key,
            Units = g.Sum(line => line.Quantity)
        })
        .OrderByDescending(row => row.Units)
        .ToListAsync(ct);

        return Results.Ok(ranked);
});



app.MapGet("/reports/rank-of/{units:int}", async (int units, AutoSupplyDbContext db, CancellationToken ct) => {
    var unitsDesc = await db.FulfillmentEvents
        .Where(e => e.Type == FulfillmentEventType.Fulfilled)
        .Join(
            db.OrderLines,
            e => e.OrderId,
            line => line.OrderId,
            (e, line) => line)
        .GroupBy(line => line.ProductId)
        .Select(g => g.Sum(line => line.Quantity))
        .OrderByDescending(row => row)
        .ToArrayAsync(ct);

        //Binary search
        var index = Array.BinarySearch(
            unitsDesc,
            units,
            Comparer<int>.Create((a, b) => b.CompareTo(a))
        );



        return Results.Ok(new
        {
            units,
            rank = index >= 0 ? index + 1 : -1
        });
});




//--- Concurrent dictionary lookup endpoint ---
app.MapGet("/products/by-sku/{sku}", (string sku, IFulfillmentService fulfillmentService) => {

    try{
        var productId = fulfillmentService.ResolveProductId(sku);

        return Results.Ok(new {
            sku,
            productId
        });
    }
    catch(UnknownSkuException ex)
    {
        return Results.NotFound(new {
            error = ex.Message,
            sku = ex.Sku
        });
    }



});









app.MapPost("/benchmark", async (
    BenchmarkRequest request,
    ISeeder seeder,
    IFulfillmentService fulfillmentService,
    CancellationToken ct
) =>
{
    //Sequential benchmark
    var ids1 = await seeder.ResetAndCreateOrderAsync(request.Number, ct);

    var sw1 = Stopwatch.StartNew();
    foreach(var id in ids1)
    {
        await fulfillmentService.FulfillOrderAsync(id, ct);
    }
    sw1.Stop();


    //Burst / Concurrent benchmark
    var ids2 = await seeder.ResetAndCreateOrderAsync(request.Number, ct);

    var sw2 = Stopwatch.StartNew();
    await fulfillmentService.FulfillBurstAsync(ids2, ct);
    sw2.Stop();


    return Results.Ok(new {
        sequentialTime = sw1.ElapsedMilliseconds,
        concurrentTime = sw2.ElapsedMilliseconds,
        speedUp = $"Speed up factor: {(double)sw1.ElapsedMilliseconds / sw2.ElapsedMilliseconds}"
    });
});






app.Run();

