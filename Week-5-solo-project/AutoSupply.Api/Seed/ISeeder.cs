namespace AutoSupply.Api.Seed;

public interface ISeeder
{
    Task<SeedResult> SeedCatalogAsync(CancellationToken ct = default);

}



public record SeedResult
{
    public int CategoriesCreated { get; init; }
    public int ProductsCreated { get; init; }
    public int CustomersCreated { get; init; }
}