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






// ---- project endpoints templates ----
// app.MapPost("/seed/categories", async (AutoSupplyDbContext db) =>
// {
//     //create products, customers and starting inventory
//     var categories = new List<Category>
//     {
//         new Category { CategoryName = "Engine Parts" },
//         new Category { CategoryName = "Brakes" },
//         new Category { CategoryName = "Tires" },
//         new Category { CategoryName = "Fluids" }
//     };

//     db.Categories.AddRange(categories);
//     await db.SaveChangesAsync();

//     return Results.Ok(new
//     {
//         message = "Seed completed",
//         categories = categories.Count
//     });
// });


//testing seeding products
// app.MapPost("/seed/products", async (AutoSupplyDbContext db) => 
// {

//     var brakes = await db.Categories.SingleOrDefaultAsync(c => c.CategoryName == "Brakes");

//     var products = new List<Product>
//     {
//         new Product { 
//             Sku  = "BRAKE-PAD-001", 
//             Name = "Ceramic brake pad set", Price = 59.99m, 
//             Category = brakes, 
//             InventoryItem = new InventoryItem { QuantityOnHand = 13 }
//         },
//         new Product { 
//             Sku  = "BRAKE-PAD-002", 
//             Name = "Normal brake pad set", Price = 39.99m, 
//             Category = brakes, 
//             InventoryItem = new InventoryItem { QuantityOnHand = 13 }
//         },
//         new Product { 
//             Sku  = "BRAKE-PAD-003", 
//             Name = "Carbon brake pad set", Price = 109.99m, 
//             Category = brakes, 
//             InventoryItem = new InventoryItem { QuantityOnHand = 13 }
//         },
//     };

//     db.Products.AddRange(products);
//     await db.SaveChangesAsync();

//     return Results.Ok(new
//     {
//         message = "Seed products completed",
//         products = products.Count
//     });
// });








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


app.MapGet("/inventory", () =>
{
    return "Inventory data: { data, data, data }";
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

