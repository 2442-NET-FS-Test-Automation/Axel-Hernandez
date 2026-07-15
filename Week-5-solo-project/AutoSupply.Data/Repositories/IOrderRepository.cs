using AutoSupply.Data.Entities;

namespace AutoSupply.Data;

public interface IOrderRepository
{
    Task<IReadOnlyList<(int ProductId, string Sku, string Name, int QuantityOnHand)>> GetInventoryAsync (CancellationToken ct = default);

    Task<(IReadOnlyList<int> CustomerIds, IReadOnlyList<int> ProductIds)> GetCustomerAndProductIdsAsync(
        CancellationToken ct = default);


    // Task<(IReadOnlyList<int> CustomerIds, IReadOnlyList<int> ProductIds)> GetCustomerAndProductIdAsync(CancellationToken ct = default);

    // Task AddOrderAsync(IEnumerable<Order> orders, CancellationToken ct = default);




}