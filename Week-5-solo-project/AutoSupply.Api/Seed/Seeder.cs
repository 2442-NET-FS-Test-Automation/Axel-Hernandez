using AutoSupply.Data; //brings data models in here
using AutoSupply.Data.Entities;
using Microsoft.EntityFrameworkCore;

namespace AutoSupply.Api.Seed;

public class Seeder : ISeeder
{
    private readonly IDbContextFactory<AutoSupplyDbContext> _factory; //factory type variable hold dbcontext to create contexts based on this

    public Seeder(IDbContextFactory<AutoSupplyDbContext> factory) //method to seed things, based on dbcontext factory
    {
        _factory = factory;
    }



    public async Task<SeedResult> SeedCatalogAsync(CancellationToken ct = default)
    {
        await using var db = await _factory.CreateDbContextAsync(ct);

        var categoriesCreated = 0;

        var brakes = await GetOrCreateCategoryAsync(db, "Brakes", ct);
        var engine = await GetOrCreateCategoryAsync(db, "Engine", ct);
        var tires = await GetOrCreateCategoryAsync(db, "Tires", ct);
        var fluids = await GetOrCreateCategoryAsync(db, "Fluids", ct);


        //implement list later on, instead od manually adding / summing 1
        categoriesCreated += brakes.Created ? 1 : 0;
        categoriesCreated += engine.Created ? 1 : 0;
        categoriesCreated += tires.Created ? 1 : 0;
        categoriesCreated += fluids.Created ? 1 : 0;


        // await db.SaveChangesAsync(ct);


        // -------- Customers section -------------
        var customersCreated = 0;
        var axel = await GetOrCreateCustomerAsync(
            db,
            "Axel",
            "Hernandez",
            "axel@email.com",
            ct
        );
        var luis = await GetOrCreateCustomerAsync(
            db,
            "Luis",
            "Santino",
            "luis@email.com",
            ct
        );
        var paola = await GetOrCreateCustomerAsync(
            db,
            "Paola",
            "Felix",
            "paola@email.com",
            ct
        );
        var victor = await GetOrCreateCustomerAsync(
            db,
            "Victor",
            "Perez",
            "victor@email.com",
            ct
        );

        customersCreated += axel.Created ? 1 : 0;
        customersCreated += luis.Created ? 1 : 0;
        customersCreated += paola.Created ? 1 : 0;
        customersCreated += victor.Created ? 1 : 0;

        // await db.SaveChangesAsync(ct);



        // -------- Products + Inventory (Quantity) section -------------
        var productAndQtyCreated = 0;
        var cerBrake = await GetOrCreateProductAsync(
            db,
            brakes.Category,
            "BRK-001",
            "Cermic breakes",
            59.99m,
            20,
            ct
        );
        var oil50 = await GetOrCreateProductAsync(
            db,
            fluids.Category,
            "FLU-001",
            "Synthetic motor oil 5W-30",
            34.99m,
            15,
            ct
        );
        var engineHeaders = await GetOrCreateProductAsync(
            db,
            engine.Category,
            "ENG-001",
            "Headers titanium",
            129.99m,
            4,
            ct
        );
        var tiresMich = await GetOrCreateProductAsync(
            db,
            tires.Category,
            "TIR-001",
            "Michelling 02",
            69.99m,
            10,
            ct
        );

        productAndQtyCreated += cerBrake.Created ? 1 : 0;
        productAndQtyCreated += oil50.Created ? 1 : 0;
        productAndQtyCreated += engineHeaders.Created ? 1 : 0;
        productAndQtyCreated += tiresMich.Created ? 1 : 0;


        await db.SaveChangesAsync(ct);


        return new SeedResult
        {
            CategoriesCreated = categoriesCreated,
            ProductsCreated = productAndQtyCreated,
            CustomersCreated = customersCreated
        };
    }



    //Seed category - helper function
    private static async Task<(Category Category, bool Created)> GetOrCreateCategoryAsync(
        AutoSupplyDbContext db, string categoryName, CancellationToken ct
    )
    {
        var existingCategory = await db.Categories
            .FirstOrDefaultAsync(c => c.CategoryName == categoryName, ct);

        if(existingCategory is not null)
        {
            return (existingCategory, false);
        }



        var category = new Category
        {
            CategoryName = categoryName
        };


        db.Categories.Add(category);
        return (category, true);
    }




    //seed customers - helper function
    private static async Task<(Customer Customer, bool Created)> GetOrCreateCustomerAsync(
        AutoSupplyDbContext db,
        string firstName,
        string lastName,
        string email,
        CancellationToken ct
    )
    {
        var existingCustomer = await db.Customers
            .FirstOrDefaultAsync(c => c.Email == email, ct);


        if(existingCustomer is not null)
        {
            return (existingCustomer, false);
        }


        var customer = new Customer
        {
            FirstName = firstName,
            LastName = lastName,
            Email = email
        };

        db.Customers.Add(customer);
        return (customer, true);
    }




    //seed - products + inventory quantity
    private static async Task<(Product Product, bool Created)> GetOrCreateProductAsync(
        AutoSupplyDbContext db,
        Category category,
        string sku,
        string name,
        decimal price,
        int quantityOnHand,
        CancellationToken ct
    )
    {
        var existingProduct = await db.Products
            .FirstOrDefaultAsync(p => p.Sku == sku, ct);

        if(existingProduct is not null)
        {
            return (existingProduct, false);
        }



        var product = new Product
        {
            Category = category,
            Sku = sku,
            Name = name,
            Price = price,
            InventoryItem = new InventoryItem
            {
                QuantityOnHand = quantityOnHand
            }
        };

        db.Products.Add(product);
        return (product, true);
    }
    
}
