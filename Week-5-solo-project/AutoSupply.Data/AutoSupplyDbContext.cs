using AutoSupply.Data.Entities;
using AutoSupply.Data.Enums;
using Microsoft.EntityFrameworkCore;

namespace AutoSupply.Data;


public class AutoSupplyDbContext : DbContext
{
    //Constructor
    public AutoSupplyDbContext(DbContextOptions<AutoSupplyDbContext> options) : base(options) { }


    //Telling csharp about our tables in db
    public DbSet<Customer> Customers => Set<Customer>();
    public DbSet<Order> Orders => Set<Order>();
    public DbSet<OrderLine> OrderLines => Set<OrderLine>();
    public DbSet<Category> Categories => Set<Category>();
    public DbSet<Product> Products => Set<Product>();
    public DbSet<FulfillmentEvent> FulfillmentEvents => Set<FulfillmentEvent>();
    public DbSet<InventoryItem> InventoryItems => Set<InventoryItem>();


    //on model creating - to give shape to our database in csharp, with EntityFramework
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);


        modelBuilder.Entity<Customer>(customer =>
        {
            customer.Property(c => c.FirstName)
                .IsRequired()
                .HasMaxLength(100);
            customer.Property(c => c.LastName)
                .IsRequired()
                .HasMaxLength(100);
            customer.Property(c => c.Email)
                .HasMaxLength(255)
                .IsRequired();
            customer.HasIndex(c => c.Email)
                .IsUnique();

        });


        modelBuilder.Entity<Category>(category =>
        {
            category.Property(c => c.CategoryName)
                .IsRequired()
                .HasMaxLength(100);
            category.HasIndex(c => c.CategoryName)
                .IsUnique();
        });


        modelBuilder.Entity<Product>(product =>
        {
            product.Property(p => p.Sku)
                .IsRequired()
                .HasMaxLength(255);
            product.HasIndex(p => p.Sku)
                .IsUnique();
            product.Property(p => p.Name)
                .IsRequired()
                .HasMaxLength(100);
            product.Property(p => p.Price)
                .HasPrecision(18, 2);
            product.HasOne(p => p.Category)
                .WithMany(c => c.Products)
                .HasForeignKey(p => p.CategoryId);

        });


        modelBuilder.Entity<InventoryItem>(inventoryItem =>
        {
            inventoryItem.HasOne(i => i.Product)
                .WithOne(p => p.InventoryItem)
                //for 1:1 one to one relation, you need to setup the type of item holding the FK, in this case InventoryItem
                .HasForeignKey<InventoryItem>(i => i.ProductId);
            inventoryItem.Property(i => i.QuantityOnHand)
                .IsRequired();
        });


        modelBuilder.Entity<Order>(order =>
        {
            order.HasOne(o => o.Customer)
                .WithMany(c => c.Orders)
                .HasForeignKey(o => o.CustomerId);

            order.Property(o => o.CreatedAt)
                .HasColumnType("datetime2")
                .IsRequired();

            order.Property(o => o.CompletedAt)
                .HasColumnType("datetime2");

            order.Property(o => o.Status)
                .IsRequired()
                // EF knows Status is an enum from the property type and stores it as a string.
                .HasConversion<string>()
                .HasMaxLength(50);
            order.HasIndex(o => o.Status);

            order.Property(o => o.Priority)
                .IsRequired()
                .HasConversion<string>()
                .HasMaxLength(50);
        });


        modelBuilder.Entity<OrderLine>(orderLine =>
        {
            orderLine.HasOne(ol => ol.Product)
                .WithMany(p => p.OrderLines)
                .HasForeignKey(ol => ol.ProductId);
            orderLine.HasOne(ol => ol.Order)
                .WithMany(o => o.OrderLines)
                .HasForeignKey(ol => ol.OrderId);
            orderLine.Property(ol => ol.Quantity)
                .IsRequired();
        });


        modelBuilder.Entity<FulfillmentEvent>(fulfillmentEvent =>
        {
            fulfillmentEvent.HasOne(fe => fe.Order)
                .WithMany(o => o.FulfillmentEvents)
                .HasForeignKey(fe => fe.OrderId);
            fulfillmentEvent.Property(fe => fe.Type)
                .IsRequired()
                .HasConversion<string>()
                .HasMaxLength(100);
            fulfillmentEvent.Property(fe => fe.Message)
                .IsRequired()
                .HasMaxLength(500);
            fulfillmentEvent.Property(fe => fe.OccurredAt)
                .IsRequired()
                .HasColumnType("datetime2");
        });



    }

}