using AutoSupply.Data.Entities;
using Microsoft.EntityFrameworkCore;

namespace AutoSupply.Data;

public class OrderRepository : IOrderRepository
{
    private readonly IDbContextFactory<AutoSupplyDbContext> _factory;

    public OrderRepository(IDbContextFactory<AutoSupplyDbContext> factory)
    {
        _factory = factory;
    } 



    public async Task<IReadOnlyList<(int ProductId, string Sku, string Name, int QuantityOnHand)>> GetInventoryAsync (CancellationToken ct = default)
    {
        //create db context
        await using var db = await _factory.CreateDbContextAsync(ct);

        var result = await db.InventoryItems
            .Select(item => new
            {
                item.ProductId,
                Sku = item.Product.Sku,
                Name = item.Product.Name,
                item.QuantityOnHand
            }).ToListAsync(ct);



        return result
            .Select(row => (row.ProductId, row.Sku, row.Name, row.QuantityOnHand))
            .ToList();
    }




    public async Task<(IReadOnlyList<int> CustomerIds, IReadOnlyList<int> ProductIds)> GetCustomerAndProductIdsAsync(CancellationToken ct = default)
    {
        await using var db = await _factory.CreateDbContextAsync(ct);

        var customerIds = await db.Customers
            .Select(customer => customer.Id)
            .ToListAsync(ct);

        var productIds = await db.Products
            .Select(product => product.Id)
            .ToListAsync(ct);

        return (customerIds, productIds);
    }
}