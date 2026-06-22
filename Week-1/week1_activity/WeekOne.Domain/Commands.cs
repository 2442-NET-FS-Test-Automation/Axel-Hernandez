using Serilog;
using System.Text.RegularExpressions;

namespace WeekOne.Domain;

public partial class Commands
{


    private static ICarRepository _repository = null!;

    public static void Configure(ICarRepository repository)
    {
        _repository = repository;
    }


    //regex validation helper function
    private static bool IsValidId(string input)
    {
        return !string.IsNullOrWhiteSpace(input) &&
        Regex.IsMatch(input, @"^[A-Za-z]+$");
    }

    
    public static void PrintMenu()
    {
        Console.WriteLine("Menu options:");
        Console.WriteLine("1: Print inventory");
        Console.WriteLine("2: Add car to inventory");
        Console.WriteLine("3: Delete a car");
        Console.WriteLine("4: Rent a car"); 
        Console.WriteLine("5: Cancel a rent"); 
        Console.WriteLine("6: List of your rented cars");
        Console.WriteLine("7: Undo Last Action");
        Console.WriteLine("8: Waiting List");
        Console.WriteLine("9: Async method");
        Console.WriteLine("10: Search & Browse");
        Console.WriteLine("0: Exit");
        Console.WriteLine("Enter your choice:");
        Log.Information("Printed menu successfully");
    }
    
    public static void PrintInventory()
    {
        int idW = 5, brandW = 14, modelW = 20, costW = 9, periodW = 15, availW = 10;
        int totalWidth = idW + brandW + modelW + costW + periodW + availW + 7; 

        string border = new string('-', totalWidth);

        string FormatRow(string id, string brand, string model, string cost, string period, string avail)
        {
            return "|" + id.PadLeft((idW + id.Length) / 2).PadRight(idW)
                 + "|" + brand.PadLeft((brandW + brand.Length) / 2).PadRight(brandW)
                 + "|" + model.PadLeft((modelW + model.Length) / 2).PadRight(modelW)
                 + "|" + cost.PadLeft((costW + cost.Length) / 2).PadRight(costW)
                 + "|" + period.PadLeft((periodW + period.Length) / 2).PadRight(periodW)
                 + "|" + avail.PadLeft((availW + avail.Length) / 2).PadRight(availW)
                 + "|";
        }

        Console.WriteLine(border);
        Console.WriteLine(FormatRow("Id", "Brand", "Model", "Daycost", "Rental period", "Available"));
        Console.WriteLine(border);



        if (_repository.Count == 0)
        {
            Console.WriteLine(FormatRow("", "", "", "", "", ""));
        }
        else
        {
            foreach (var car in _repository.GetAll())
            {
                string avail = car.IsAvailable ? "Y" : "N";
                Console.WriteLine(FormatRow(
                    car.Id.ToString(),
                    car.Brand,
                    car.Model,
                    car.DayCost.ToString(),
                    car.RentalPeriod.ToString(),
                    avail
                ));
            }
        }

        Console.WriteLine(border);
        Log.Information("Printed Inventory successfully");
    }

    public static void PrintAvailableInventory()
    {
        Console.WriteLine("--------------------------------");
        Console.WriteLine("Printing available cars");
        Console.WriteLine("--------------------------------");
        Console.WriteLine("--------------------------------");

        foreach (var car in _repository.GetAll())
        {
            Console.WriteLine($"{car.Id}) {car.ToString()}");
            Console.WriteLine("--------------------------------");
        }
        Console.WriteLine("--------------------------------");
        Log.Information("Printed Available Inventory successfully");
    }

    public static void AddCar()
    {
        Console.WriteLine("--------------------------------");
        Console.WriteLine("Add a new Car");
        try
        {
            Console.WriteLine("Enter brand of the car:");
            string brand = Console.ReadLine() ?? "";

            if(!IsValidId(brand))
            {
                Console.WriteLine("Invalid brand. Use letters only (A-Z)");
                Console.WriteLine("--------------------------------");
                return;
            }



            
            Console.WriteLine("Enter model of the car:");
            string model = Console.ReadLine();

            Console.WriteLine("Enter daily cost of the car:");
            int dayCost = int.Parse(Console.ReadLine());
        
            Console.WriteLine("Enter minimum rental period of the car:");
            int rentalPeriod = int.Parse(Console.ReadLine());

            Console.WriteLine("Enter if the car is available (true/false):");
            bool isAvailable = bool.Parse(Console.ReadLine());
            CarStatus status = isAvailable ? CarStatus.Available : CarStatus.Rented;

            CarRental newCar = new CarRental(brand, model, dayCost, rentalPeriod, status);
            _repository.Add(newCar);
            Log.Information("Car added successfully");
        }catch(Exception ex)
        {
            Console.WriteLine("Error with an input: "+ex.Message);
            Log.Error("Error on AddCar {ex.Message}", ex.Message);
        }

        Console.WriteLine("2: Adding car to inventory:");
        Console.WriteLine("--------------------------------");
    }

    public static CarRental DeleteCar()
    {
        Console.WriteLine("--------------------------------");
        Console.WriteLine("Delete an existing car");
        Commands.PrintInventory();
        Console.WriteLine("Which car do you wish to delete? Type id or 0 to exit");

        int carToBeDeleted = 0;
        try
        {
            carToBeDeleted = Convert.ToInt32(Console.ReadLine());
        }catch(Exception ex)
        {
            Console.WriteLine("Error : "+ex.Message);
            Log.Error("Error on DeleteCar {ex.Message}",ex.Message);
        }
        

        CarRental coche = _repository.GetById(carToBeDeleted);

        if(coche is not null)
        {
            try
            {
                _repository.Remove(coche);
            }catch(NoCarFoundException ex)
            {

                Log.Error("Error at DeleteCar: {ex.Message}",ex.Message);
                Console.WriteLine("No car with that id");
            }
            
            
        }
        else if(carToBeDeleted == 0)
        {
            Console.WriteLine("No car deleted"); 
        }
        else
        {
            Console.WriteLine("No car with sush id");
        }

        return coche;
    }
    
    public static void RentCar(List<(CarRental coche, int dias)> rentedCars)
    {
        Console.WriteLine("--------------------------------");
        Console.WriteLine("Rent a car: ");

        Console.WriteLine("Choose one from the available stock:");

        bool inProgress = true;
        while(inProgress)
        {
            Commands.PrintInventory();
            Console.WriteLine("Select the car by its id or 0 to exit:");

            int choice = 0;
            CarRental selectedCar = null;
            try
            {
                choice = int.Parse(Console.ReadLine());
                selectedCar = _repository.GetById(choice);
            }catch(Exception ex)
            {
                Console.WriteLine("Exception : "+ex.Message);
                choice = 0;
                Log.Error("Error on RentCar: {ex.Message}",ex.Message);
            }

            if(selectedCar is not null && selectedCar.IsAvailable)
            {            
                selectedCar.ChangeStatus(CarStatus.Rented);
                Console.WriteLine($"Car selected for rental: {selectedCar.ToString()}");

                
                int days = 0;
                while(days < selectedCar.RentalPeriod)
                {
                    Console.WriteLine($"Enter the number of days to be rented:\nMinimum days to rent is: {selectedCar.RentalPeriod}");
                    try
                    {
                        days = int.Parse(Console.ReadLine());
                    }
                    catch(Exception ex)
                    {
                        Log.Error("Error input days to rent a car");
                        days = 0;
                    }
                    
                }
                // Console.WriteLine($"Total bill: ${selectedCar.CalculateFee(days, selectedCar)}");


                RentalQuote quote = new RentalQuote(days, selectedCar.DayCost);
                Console.WriteLine($"Total bill: {quote}");

                inProgress = false;
                rentedCars.Add((selectedCar, days));
                Log.Information("Car with id {selectedCar.Id} rented successfully", selectedCar.Id);
            }
            else if(selectedCar is not null && !selectedCar.IsAvailable)
            {
                Console.WriteLine($"Car {choice} is not available");
                Log.Warning("Car with id:{selectedCar.Id} is not available", selectedCar.Id);

                if (Inventory.waitingList.Contains(selectedCar))
                {
                    Console.WriteLine("Car already in waiting list");
                    return;
                }

                bool addingToWaitingList = true;
                while(addingToWaitingList)
                {
                    Console.WriteLine("Would you like to add this car to the waiting list? (y/n)");
                    string answer = Console.ReadLine();
                    if(answer == "y")
                    {
                        Inventory.waitingList.Add(selectedCar);
                        Console.WriteLine("----- Car Added to Waiting List -----");
                        addingToWaitingList = false;
                        Log.Information("Added to waiting list");
                        return;
                    }
                    else if(answer == "n")
                    {
                        Console.WriteLine("Not adding to waiting list...");
                        addingToWaitingList = false;
                        return;
                    }
                }
            }
            else if(choice == 0)
            {
                inProgress = false;
            }
            else
            {
                Console.WriteLine($"{choice} is not a valid option, select again");
            }
                        
        }

        Console.WriteLine("3: Renting car.....");
        Console.WriteLine("--------------------------------");
    }

    public static void RentedCarsInfo(List<(CarRental coche, int dias)> rentedCars)
    {
        Console.WriteLine("--------------------------------");
        Console.WriteLine("Rented cars information");
        int total = 0;
        int tmp = 0;
        Console.WriteLine("Your rented cars:");
        if(rentedCars.Count == 0)
        {
            Console.WriteLine("You have yet to rent a car");
            Log.Warning("You have no rented cars to display on RentedCarsInfo");
        }
        else
        {
            foreach (var rentado in rentedCars)
            {
                tmp = rentado.coche.CalculateFee(rentado.dias, rentado.coche);
                total += tmp;
                Console.WriteLine(
                    $"{rentado.coche.Id}) {rentado.coche.Brand}:{rentado.coche.Model} "
                    + $"=> rented for: {rentado.dias} days.\n\tCost:{tmp}"
                );
            }
            Log.Information("Rented cars info display successfully");
        }
        
        Console.WriteLine($"Total : {total}");
    }
   
    public static List<(CarRental coche, int dias)> UnRent(List<(CarRental coche, int dias)> rentedCars)
    {
        int num = 0;
        Console.WriteLine("--------------------------------");
        Console.WriteLine("Cancel a rent");

        List<(CarRental coche, int dias)> unRentedCars = new();

        if(rentedCars.Count == 0)
        {
            Console.WriteLine("You dont have any rented car");
            Log.Warning("No cars rented to unrent via UnRent");
        }
        else
        {
            Console.WriteLine("Rented car(s):");
            foreach(var item in rentedCars)
            {
                Console.WriteLine($"{item.coche.Id}) {item.coche.Brand}:{item.coche.Model}");
            }
            Console.WriteLine("Which car do you want to eliminate?");

            try
            {
                num = Convert.ToInt32(Console.ReadLine());
            }catch(Exception ex)
            {
                Console.WriteLine("Exception: "+ex.Message);
                num = 0;
                Log.Error("Input error on UnRent {ex.Message}",ex.Message);
            }
            
            CarRental coche = null;
            int dias = 0; 

            foreach(var item in rentedCars)
            {
                if(item.coche.Id == num)
                {
                    coche = item.coche;
                    dias = item.dias;
                    item.coche.ChangeStatus(CarStatus.Available);
                    unRentedCars.Add((coche, dias));
                }
            }

            if(rentedCars.Remove((coche,dias)))
            {
                Console.WriteLine($"Car with id:{coche.Id} is eliminated from your rents");
                Log.Information("Rented car unrented successfully");
            }
            else
            {
                Console.WriteLine($"Not a valid id, retry");
                Log.Warning("Unsecussfull to unrent a car via UnRent");
            }
        }

        return unRentedCars;
    }

    public static void ReorderWaitingList()
    {
        if (Inventory.waitingList.Count < 2)
        {
            Console.WriteLine("Need at least 2 cars in the waiting list to reorder.");
            return;
        }

        Console.WriteLine("Current order:");
        for (int i = 0; i < Inventory.waitingList.Count; i++)
        {
            var car = Inventory.waitingList[i];
            Console.WriteLine($"{i + 1}) {car.Brand} {car.Model}");
        }

        Console.WriteLine("Which item do you want to move? (position number)");
        int from = int.Parse(Console.ReadLine());

        Console.WriteLine("Move it to which position?");
        int to = int.Parse(Console.ReadLine());

        if (Inventory.TryMoveWaitingListItem(from, to))
        {
            Console.WriteLine("Updated order:");
            for (int i = 0; i < Inventory.waitingList.Count; i++)
            {
                var car = Inventory.waitingList[i];
                Console.WriteLine($"{i + 1}) {car.Brand} {car.Model}");
            }
        }
        else
        {
            Console.WriteLine("Invalid position. No changes made.");
        }
    }

    public static void WaitingListInfo()
    {
        bool inProgress = true;

        while (inProgress)
        {
            Console.WriteLine("------ Waiting list -------");

            if (Inventory.waitingList.Count == 0)
            {
                Console.WriteLine("No cars in waiting list");
            }
            else
            {
                for (int i = 0; i < Inventory.waitingList.Count; i++)
                {
                    var car = Inventory.waitingList[i];
                    Console.WriteLine($"{i + 1}) {car.Brand} {car.Model}");
                }
            }

            Console.WriteLine();
            Console.WriteLine("1: Change order");
            Console.WriteLine("0: Back to main menu");
            Console.WriteLine("Enter your choice:");

            int choice;
            try
            {
                choice = int.Parse(Console.ReadLine());
            }catch(Exception ex)
            {
                Log.Error("Input error at WaitingListInfo ");
                choice = 2;
            }
            

            switch (choice)
            {
                case 1:
                    ReorderWaitingList();
                    break;

                case 0:
                    inProgress = false;
                    break;

                default:
                    Console.WriteLine("Invalid option.");
                    break;
            }
        }
    }

    public static async Task AsyncHttpDemo()
    {
        OpenLibraryClient open = new();

        Console.WriteLine("Which id do you want to fetch? 3 digits. You can also leave blank");
        int id; 
        try
        {
            id = int.Parse(Console.ReadLine());
        }
        catch(Exception ex)
        {
            Log.Warning("Warning input id to fecth {ex.Message}",ex.Message);
            id = 440;
        }

        CarRental? foundCars = await open.FetchByIdAsync(id);

        if(foundCars is not null)
        {
            _repository.Add(foundCars);
            Log.Information("Car added via fetch api");
        }
        else
        {
            Log.Warning("No car was successfully fetch via api");
        }

    }

}
