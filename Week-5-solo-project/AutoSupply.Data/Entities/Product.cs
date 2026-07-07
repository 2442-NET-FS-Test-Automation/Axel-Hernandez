namespace AutoSupply.Data.Entities;


public class Product
{
    public int Id { get; set; }
    public int CategoryId { get; set; }
    public Category Category { get; set; } = default!;
    public InventoryItem InventoryItem { get; set; } = default!;
    public string Sku { get; set; } = default!;
    public string Name { get; set; } = default!;
    public decimal Price { get; set; }



    //db schema relationship
    public List<OrderLine> OrderLines { get; set; } = new();


}