namespace AutoSupply.Api.Fulfillment;

public interface IFulfillmentService
{
    Task<FulfillmentResult> FulfillOrderAsync (int orderId, CancellationToken ct);
}


public enum FulfillmentResult { Fulfilled, Backordered }