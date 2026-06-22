namespace WeekOne.Domain;
using Serilog;

public partial class Commands
{
    //Submenu for Search & Browse features
    public static void PrintSearchBrowseMenu()
    {
        Console.WriteLine("--------------------------------");
        Console.WriteLine("Search & Browse");
        Console.WriteLine("--------------------------------");
        Console.WriteLine("1: Lookup car by Id");
        Console.WriteLine("2: List distinct brands");
        Console.WriteLine("3: Search by price");
        Console.WriteLine("0: Back to main menu");
        Console.WriteLine("Enter your choice:");
    }

    public static void SearchBrowseMenuLoop()
    {
        bool inSubMenu = true;
        Log.Information("SearchBrowseMenuLoop print");
        while (inSubMenu)
        {
            PrintSearchBrowseMenu();
            if (!int.TryParse(Console.ReadLine(), out int subChoice))
            {
                Console.WriteLine("Not a valid option");
                continue;
            }

            switch (subChoice)
            {
                case 0: inSubMenu = false; break;
                case 1: LookupById(); break;
                case 2: ListDistinctBrands(); break;
                case 3: SearchByCondition(); break;
                default: Console.WriteLine("Not a valid option"); break;
            }
        }
    }

    //Instant lookup by key (Id)
    public static void LookupById()
    {
        Console.WriteLine("--------------------------------");
        Console.WriteLine("Lookup car by Id");
        Console.WriteLine("Enter the Id to search for:");

        if (!int.TryParse(Console.ReadLine(), out int searchId))
        {
            Console.WriteLine("Invalid input, please enter a numeric Id.");
            Console.WriteLine("--------------------------------");
            return;
        }

        CarRental found = null;
        Log.Information("Look up car by id {searchId}", searchId);
        foreach (var car in _repository.GetAll())
        {
            if (car.Id == searchId)
            {
                found = car;
                Log.Information("Car with id {searchId} found", searchId);
                break;
            }
        }

        if (found == null)
        {
            Console.WriteLine($"Car with Id {searchId}: not found");
        }
        else
        {
            Console.WriteLine($"Found: {found.ToString()}");
        }

        Console.WriteLine("--------------------------------");
        
    }

    //Lists each distinct brand once, no duplicates
    public static void ListDistinctBrands()
    {
        Console.WriteLine("--------------------------------");
        Console.WriteLine("Distinct brands in inventory");
        Console.WriteLine("--------------------------------");

        int entityCount = _repository.Count;

        if (entityCount == 0)
        {
            Console.WriteLine("There are no cars in the inventory.");
            Console.WriteLine("--------------------------------");
            return;
        }

        List<string> distinctBrands = new List<string>();

        foreach (var car in _repository.GetAll())
        {
            if (!distinctBrands.Contains(car.Brand))
            {
                distinctBrands.Add(car.Brand);
            }
        }

        Console.WriteLine($"We have {distinctBrands.Count} unique brands in our inventory");
        Console.WriteLine($"(out of {entityCount} total cars)");
        Console.WriteLine("--------------------------------");

        foreach (string brand in distinctBrands)
        {
            Console.WriteLine(brand);
        }

        Console.WriteLine("--------------------------------");
        Log.Information("Car brands listed");
    }

    //Filters cars by day cost (over X or under X)
    //Search by condition: filter cars by day cost (over X and under Y)
    public static void SearchByCondition()
    {
        Console.WriteLine("--------------------------------");
        Console.WriteLine("Search by day cost");

        // Asks over value or price
        int overValue;
        while (true)
        {
            Console.WriteLine("Enter minimum day cost (over):");
            string overInput = Console.ReadLine();

            if (!int.TryParse(overInput, out overValue))
            {
                Console.WriteLine("That's not a valid number, try again.");
                continue;
            }

            if (overValue <= 0)
            {
                Console.WriteLine("The value must be greater than 0, try again.");
                continue;
            }

            break;
        }

        // Asks under value or price
        int underValue;
        while (true)
        {
            Console.WriteLine("Enter maximum day cost (under):");
            string underInput = Console.ReadLine();

            if (!int.TryParse(underInput, out underValue))
            {
                Console.WriteLine("That's not a valid number, try again.");
                continue;
            }

            if (underValue <= 0)
            {
                Console.WriteLine("The value must be greater than 0, try again.");
                continue;
            }

            break;
        }

        // Filter cars matching both conditions
        // List<CarRental> results = Inventory.GetInventory().FindAll(
        //     car => car.DayCost > overValue && car.DayCost < underValue
        // );

        List<CarRental> results = _repository.GetAll()
            .Where(car => car.DayCost > overValue && car.DayCost < underValue)
            .ToList();

        Console.WriteLine($"Cars with day cost over {overValue} and under {underValue}:");
        Console.WriteLine($"Found {results.Count} matching car(s):");
        Console.WriteLine("--------------------------------");

        if (results.Count == 0)
        {
            Console.WriteLine("No cars matched your condition.");
        }
        else
        {
            foreach (var car in results)
            {
                Console.WriteLine($"{car.Id}) {car.ToString()}");
                Console.WriteLine("--------------------------------");
            }
        }

        Log.Information("Search car by condition. Min {overValue}, Max {underValue}", overValue, underValue);
    }

}