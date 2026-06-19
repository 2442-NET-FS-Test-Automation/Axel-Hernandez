namespace WeekOne.Domain;

public class Commands
{
    public static void PrintMenu()
    {
        Console.WriteLine("Menu options:");
        Console.WriteLine("1: Print inventory");
        Console.WriteLine("2: Add car to inventory");
        Console.WriteLine("3: Rent a car"); 
        Console.WriteLine("4: Cancel a rent"); 
        Console.WriteLine("5: List of your rented cars");
        Console.WriteLine("0: Exit");
        Console.WriteLine("Enter your choice:");
    }

    //Print initial inventory
    public static void PrintInventory()
    {
        Console.WriteLine("--------------------------------");
        Console.WriteLine("1: Printing inventory:");
        Console.WriteLine("--------------------------------");
        Console.WriteLine("--------------------------------");

        foreach (var car in Inventory.CarsInStock)
        {
            
            Console.WriteLine($"{car.Id}) {car.ToString()}");
            Console.WriteLine("--------------------------------");

        }
        Console.WriteLine("--------------------------------");
    }

    public static void PrintAvailableInventory()
    {
        Console.WriteLine("--------------------------------");
        Console.WriteLine("Printing available cars");
        Console.WriteLine("--------------------------------");
        Console.WriteLine("--------------------------------");

        foreach (var car in Inventory.CarsInStock)
        {
            if(car.IsAvailable)
            {
                Console.WriteLine($"{car.Id}) {car.ToString()}");
                Console.WriteLine("--------------------------------");
            }     
        }
        Console.WriteLine("--------------------------------");
    }


    public static void AddCar()
    {
        Console.WriteLine("--------------------------------");
        Console.WriteLine("Add a new Car");
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
    public static void RentCar(List<(CarRental coche, int dias)> rentados)
    {
        Console.WriteLine("--------------------------------");
        Console.WriteLine("Rent a car: ");

        Console.WriteLine("Choose one from the available stock:"); //Comment

        bool inProgress = true;
        while(inProgress)
        {
            Commands.PrintAvailableInventory();
            Console.WriteLine("Select the car, by it's numbered option or 0 to exit:");

            //Select number of days

            //choice
            int choice = int.Parse(Console.ReadLine());
            if(choice > 0 && choice -1 < Inventory.CarsInStock.Count && Inventory.CarsInStock[choice - 1].IsAvailable)
            {
                var selectedCar = Inventory.CarsInStock[choice - 1];
            
                selectedCar.ChangeStatus(false);
                Console.WriteLine($"Car selected for rental: {selectedCar.ToString()}");

                
                int days = 0;
                while(days < selectedCar.RentalPeriod)
                {
                    Console.WriteLine($"Enter the number of days to be rented:\nMinimum days to rent is: {selectedCar.RentalPeriod}");
                    days = int.Parse(Console.ReadLine());
                }
                Console.WriteLine($"Total bill: ${selectedCar.CalculateFee(days, selectedCar)}");

                inProgress = false;
                rentados.Add((selectedCar, days));
            }
            else if(choice > 0 && choice -1 < Inventory.CarsInStock.Count && !Inventory.CarsInStock[choice - 1].IsAvailable)
            {
                Console.WriteLine($"Car {choice} is not available");
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

    //Funcion para listar los coches rentados y calcular total
    public static void RentedCarsInfo(List<(CarRental coche, int dias)> rentados)
    {
        Console.WriteLine("--------------------------------");
        Console.WriteLine("Rented cars information");
        int total = 0;
        int tmp = 0;
        Console.WriteLine("Your rented cars:");
        if(rentados.Count == 0)
        {
            Console.WriteLine("You have yet to rent a car");
        }
        else
        {
            foreach (var rentado in rentados)
            {
                tmp = rentado.coche.CalculateFee(rentado.dias, rentado.coche);
                total += tmp;
                Console.WriteLine(
                    $"{rentado.coche.Id}) {rentado.coche.Brand}:{rentado.coche.Model} "
                    + $"=> rented for: {rentado.dias} days.\n\tCost:{tmp}"
                );
            }
        }
        
        Console.WriteLine($"Total : {total}");
    }
   
   //Funcion para eliminar una renta
   public static void UnRent(List<(CarRental coche, int dias)> rentados)
    {
        int num = 0;
        Console.WriteLine("--------------------------------");
        Console.WriteLine("Cancel a rent");
        if(rentados.Count == 0)
        {
            Console.WriteLine("You dont have any rented car");
        }
        else
        {
            Console.WriteLine("Rented car(s):");
            foreach(var item in rentados)
            {
                Console.WriteLine($"{item.coche.Id}) {item.coche.Brand}:{item.coche.Model}");
            }
            Console.WriteLine("Which car do you want to eliminate?");

            num = Convert.ToInt32(Console.ReadLine());
            CarRental coche = null;
            int dias = 0; 
            foreach(var item in rentados)
            {
                if(item.coche.Id == num)
                {
                    coche = item.coche;
                    dias = item.dias;
                    item.coche.ChangeStatus(true);
                }
            }

            if(rentados.Remove((coche,dias)))
            {
                Console.WriteLine($"Car with id:{coche.Id} is eliminated from your rents");
            }
            else
            {
                Console.WriteLine($"Not a valid id, retry");
            }
        }
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