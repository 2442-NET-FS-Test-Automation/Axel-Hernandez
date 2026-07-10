namespace AutoSupply.Api.Fulfillment;

public interface IFulfillmentService
{
    Task FulfillOrderAsync(int orderId, CancellationToken ct = default);

    Task FulfillOrdersAsync(IReadOnlyList<int> orderIds, CancellationToken ct = default);
}