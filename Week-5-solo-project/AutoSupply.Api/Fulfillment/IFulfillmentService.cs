namespace AutoSupply.Api.Fulfillment;

public interface IFulfillmentService
{
    Task<FulfillmentResult> FulfillOrderAsync (int orderId, CancellationToken ct);


    Task<BurstResult> FulfillBurstAsync(IEnumerable<int> orderIds, CancellationToken ct);
}



public enum FulfillmentResult { Fulfilled, Backordered }


public record BurstResult(int Fulfilled, int Backordered);