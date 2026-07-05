var builder = WebApplication.CreateBuilder(args);
var app = builder.Build();

app.MapGet("/", () => "Testing another thing");




// ---- project endpoints templates ----
app.MapPost("/seed", () =>
{
    //create products, customers and starting inventory
    return "Seeding of products, customers and starting inventory endpoint: POST";
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

