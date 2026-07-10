namespace AutoSupply.Api.Contracts;

public record BurstOrderRequest(
    int Count,
    bool Expedited
);