using Microsoft.EntityFrameworkCore;
using AutoSupply.Data.Entities;
using AutoSupply.Data;
using AutoSupply.Api.Seed;


var builder = WebApplication.CreateBuilder(args);

var conn_string = "Server=localhost,1434;Database=AutoSupplyDb;User Id=sa;Password=TestPass1!;TrustServerCertificate=true";

builder.Services.AddDbContext<AutoSupplyDbContext>(options => options.UseSqlServer(conn_string),
        ServiceLifetime.Scoped, ServiceLifetime.Singleton);
builder.Services.AddDbContextFactory<AutoSupplyDbContext>(options => options.UseSqlServer(conn_string));


//DI interfaces
builder.Services.AddScoped<ISeeder, Seeder>();


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





app.MapGet("/orders", () =>
{
    return "All current orders";
});


app.MapGet("/orders/{id}", (int id) =>
{
   return $"get order by id: {id}"; 
});


app.MapPost("/orders/burst", () =>
{
    return "Burst of many orders at once";
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

