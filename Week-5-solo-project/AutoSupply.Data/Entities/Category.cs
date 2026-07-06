namespace AutoSupply.Data.Entities;

public class Category
{
    public int Id { get; set; }

    public string CategoryName { get; set; } = default!;


    //db schema relationship - 1:N - a Category can have maby produts, a product can have one Category
    public List<Product> Products { get; set; } = new();
}