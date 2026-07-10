using Microsoft.EntityFrameworkCore;
using AutoSupply.Data.Entities;
using AutoSupply.Data;
using AutoSupply.Api.Seed;
using AutoSupply.Api.Fulfillment;
using AutoSupply.Api.Contracts;


var builder = WebApplication.CreateBuilder(args);

var conn_string = "Server=localhost,1434;Database=AutoSupplyDb;User Id=sa;Password=TestPass1!;TrustServerCertificate=true";

builder.Services.AddDbContext<AutoSupplyDbContext>(options => options.UseSqlServer(conn_string),
        ServiceLifetime.Scoped, ServiceLifetime.Singleton);
builder.Services.AddDbContextFactory<AutoSupplyDbContext>(options => options.UseSqlServer(conn_string));


// /Seed -----------------------------
builder.Services.AddScoped<ISeeder, Seeder>();

// /Fulfillment -----------------------------
builder.Services.AddScoped<OrderFactory>();


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
app.MapGet("/inventory", async (AutoSupplyDbContext db, CancellationToken ct) => {
    var inventory = await db.InventoryItems
        // .Include(i => i.Product)
        // .ThenInclude(i => i.Category)
        // .Select(i => new {
        //     Sku =i.Product.Sku,
        //     ProductName = i.Product.Name,
        //     CategoryName = i.Product.Category.CategoryName,
        //     QuantityOnHand = i.QuantityOnHand,
        //     Price = i.Product.Price
        // })
        // .ToListAsync(ct);

        .Select(i => new {
            Sku =i.Product.Sku,
            ProductName = i.Product.Name,
            CategoryName = i.Product.Category.CategoryName,
            QuantityOnHand = i.QuantityOnHand,
            Price = i.Product.Price
        })
        .ToListAsync(ct);

        return Results.Ok(inventory);
});





app.MapPost("/orders/burst", async (BurstOrderRequest request, AutoSupplyDbContext db, OrderFactory orderFactory, CancellationToken ct) =>
{
    if(request.Count <= 0)
    {
        return Results.BadRequest("Count must be greater than zero");
    }

    var customerIds = await db.Customers
        .Select(c => c.Id)
        .ToListAsync(ct);
    
    var productIds = await db.Products
        .Select(p => p.Id)
        .ToListAsync(ct);
        
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

    db.Orders.AddRange(orders);
    await db.SaveChangesAsync(ct);

    return Results.Accepted($"/orders", new
    {
        message = "Orders created and queued for fulfillment",
        OrdersCreated = orders.Count,
        request.Expedited
    });


});





app.MapGet("/orders", () =>
{
    return "All current orders";
});


app.MapGet("/orders/{id}", (int id) =>
{
   return $"get order by id: {id}"; 
});





app.MapGet("/reports", () =>
{
    // divide later into these endpoints for reports:
    // /reports/top-products
    // /reports/top-customers
    // /reports/fulfillment-rate

    return "top products: { data } || top customers: { data } || fulfillment rate: { data }";
});


app.MapPost("/benchmark", () =>
{
    return "Benchmark of performance";
});






app.Run();

