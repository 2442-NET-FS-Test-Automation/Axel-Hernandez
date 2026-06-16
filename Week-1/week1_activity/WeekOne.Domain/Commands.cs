namespace WeekOne.Domain;


public class Commands
{
    public static void PrintMenu()
    {
        Console.WriteLine("1: Print inventory");
        Console.WriteLine("2: Add car to inventory");
        Console.WriteLine("3: Rent a car");
        Console.WriteLine("0: Exit");
        Console.WriteLine("Enter your choice:");
    }


    //Print initial inventory
    public static void PrintInventory()
    {
        Console.WriteLine("1: Printing inventory:");
        Console.WriteLine("--------------------------------");

        foreach (var car in Inventory.CarsInStock)
        {
            if(car.IsAvailable)
            {
                Console.WriteLine($"{car.Id}: {car.ToString()}");  

            }
        }
        Console.WriteLine("--------------------------------");
    }


    public static void AddCar()
    {
        Console.WriteLine("Enter brand of the car:");
        string brand = Console.ReadLine();
        
        Console.WriteLine("Enter model of the car:");
        string model = Console.ReadLine();

        Console.WriteLine("Enter daily cost of the car:");
        int dayCost = int.Parse(Console.ReadLine());
    
        Console.WriteLine("Enter minimum rental period of the car:");
        int rentalPeriod = int.Parse(Console.ReadLine());

        Console.WriteLine("Enter if the car is available (true/false):");
        bool isAvailable = bool.Parse(Console.ReadLine());

        //Create nre car with data gathered
        CarRental newCar = new CarRental(brand, model, dayCost, rentalPeriod, isAvailable);
        Inventory.CarsInStock.Add(newCar);


        Console.WriteLine("2: Adding car to inventory:");
        Console.WriteLine("--------------------------------");
    }

    //Renew car rental period
    public static void RentCar()
    {

        Console.WriteLine("Choose one from the available stock:");

        bool inProgress = true;
        while(inProgress)
        {
            Commands.PrintInventory();
            Console.WriteLine("Select the car, by it's numbered option:");

            //Select number of days

            //choice
            int choice = int.Parse(Console.ReadLine());
            var selectedCar = Inventory.CarsInStock[choice - 1];
            selectedCar.ChangeStatus(false);;
            Console.WriteLine($"Car selected for rental: {selectedCar.ToString()}");

            Console.WriteLine("Enter the number of days to be rented:");
            int days = int.Parse(Console.ReadLine());



            Console.WriteLine($"$Total bill: {selectedCar.CalculateFee(days, selectedCar)}");

            inProgress = false;
        }



        Console.WriteLine("3: Renting car.....");
        Console.WriteLine("--------------------------------");
    }


    // public static void CalculateFee(int days)
    // {
    //     Console.WriteLine("Chose one of the options:");

    //     bool inProgress = true;
    //     while(inProgress)
    //     {
    //         Console.WriteLine("Available cars:");
    //         Commands.PrintInventory();

    //         Console.WriteLine("Select byt its number:");
    //         int choice = int.Parse(Console.ReadLine());
    //         var selectedCar = Inventory.CarsInStock[choice - 1];
    //         Console.WriteLine($"Car selected: {selectedCar.ToString()}");

    //         Console.ForegroundColor = ConsoleColor.Green;
    //         Console.WriteLine($"Daily cost: {selectedCar.DayCost}");
    //         Console.ResetColor();

    //         Console.WriteLine("Enter the number of days you want to rent the car:");

            
    //     }
    // }
}