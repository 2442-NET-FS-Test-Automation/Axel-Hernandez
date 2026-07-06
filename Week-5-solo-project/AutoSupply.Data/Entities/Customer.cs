

namespace AutoSupply.Data.Entities;


public class Customer
{
    public int Id { get; set; }
    public string FirstName { get; set; } = default!;
    public string LastName { get; set; } = default!;
    public string Email { get; set; } = default!;


    //db schema relationship - 1:N - a Customer can have many Orders, an Order belongs to a Customer
    public List<Order> Orders { get; set; } = new();

}