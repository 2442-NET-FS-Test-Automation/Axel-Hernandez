namespace WeekOne.Domain;

public class Commands
{
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
        Console.WriteLine("0: Exit");
        Console.WriteLine("Enter your choice:");
    }

    //Print initial inventory
    //public static void PrintInventory()
    //{
        // Console.WriteLine("--------------------------------");
        // Console.WriteLine("1: Printing inventory:");
        // Console.WriteLine("--------------------------------");
        // Console.WriteLine("--------------------------------");

        // foreach (var car in Inventory.CarsInStock)
        // {
            
        //     Console.WriteLine($"{car.Id}) {car.ToString()}");
        //     Console.WriteLine("--------------------------------");

        // }
        // Console.WriteLine("--------------------------------");
        //Print initial inventory as a compact grid (Lot x Row), one car per cell
        //}
    
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

        if (Inventory.CarsInStock.Count == 0)
        {
            Console.WriteLine(FormatRow("", "", "", "", "", ""));
        }
        else
        {
            foreach (var car in Inventory.CarsInStock)
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
        try
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
        }catch(Exception ex)
        {
            Console.WriteLine("Error with an input: "+ex.Message);
        }

        Console.WriteLine("2: Adding car to inventory:");
        Console.WriteLine("--------------------------------");
    }

    //Add car via undo
    public static void UndoAddCar(CarRental coche)
    {
        if(coche is not null)
        {
            CarRental newCar = new CarRental(coche.Brand, coche.Model, coche.DayCost, coche.RentalPeriod, coche.IsAvailable);
            Inventory.CarsInStock.Add(newCar);
        }
        else
        {
            Console.WriteLine("No car to be added");
        }
        
    }

    //Function to delete an existing car
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
        }
        

        CarRental coche = Inventory.GetCarById(carToBeDeleted);

        if(carToBeDeleted > 0 && carToBeDeleted <= Inventory.CarsInStock.Count)
        {
            Inventory.CarsInStock.Remove(coche);
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
    
    //Function to delete a car via undo action
    public static void UndoDeleteCar()
    {
        if(Inventory.CarsInStock.Count > 0)
        {
            Inventory.CarsInStock.RemoveAt(Inventory.CarsInStock.Count-1);
        }
        else
        {
            Console.WriteLine("No car to be deleted");
        }
        
      
    }
    
    //Renew car rental period
    public static void RentCar(List<(CarRental coche, int dias)> rentedCars)
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
            int choice = 0;
            CarRental selectedCar = null;
            try
            {
                choice = int.Parse(Console.ReadLine());
                selectedCar = Inventory.GetCarById(choice);
            }catch(Exception ex)
            {
                Console.WriteLine("Exception : "+ex.Message);
                choice = 0;
            }

            //CarRental coche = Inventory.GetCarById(carToBeDeleted);
            if(selectedCar is not null && selectedCar.IsAvailable)
            {            
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
                rentedCars.Add((selectedCar, days));
            }
            else if(selectedCar is not null && !selectedCar.IsAvailable)
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

    //Undo unrent car
    public static void UndoUnRentCar(List<(CarRental coche, int dias)> rentedCars, List<(CarRental coche, int dias)> ToRentCar)
    {
        //Se añade el coche que se quito previamente
        if(ToRentCar.Count > 0)
        {
            ToRentCar[0].coche.ChangeStatus(false);
            rentedCars.Add((ToRentCar[0].coche,ToRentCar[0].dias));
        }
        else
        {
            Console.WriteLine("No car to rent again");
        }
        
    }


    //Funcion para listar los coches rentados y calcular total
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
        }
        
        Console.WriteLine($"Total : {total}");
    }
   
   //Funcion para eliminar una renta
   public static List<(CarRental coche, int dias)> UnRent(List<(CarRental coche, int dias)> rentedCars)
    {
        int num = 0;
        Console.WriteLine("--------------------------------");
        Console.WriteLine("Cancel a rent");

        List<(CarRental coche, int dias)> unRentedCars = new();

        if(rentedCars.Count == 0)
        {
            Console.WriteLine("You dont have any rented car");
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
            }
            
            CarRental coche = null;
            int dias = 0; 

            foreach(var item in rentedCars)
            {
                if(item.coche.Id == num)
                {
                    coche = item.coche;
                    dias = item.dias;
                    item.coche.ChangeStatus(true);
                    unRentedCars.Add((coche, dias));
                }
            }

            if(rentedCars.Remove((coche,dias)))
            {
                Console.WriteLine($"Car with id:{coche.Id} is eliminated from your rents");
            }
            else
            {
                Console.WriteLine($"Not a valid id, retry");
            }
        }

        return unRentedCars;
    }

    //Funcion para eliminar la ultima renta via undo action
   public static void UndoUnRent(List<(CarRental coche, int dias)> rentedCars)
    {
        if(rentedCars.Count > 0)
        {
            CarRental coche = rentedCars[rentedCars.Count-1].coche;
            coche.ChangeStatus(true);
            rentedCars.RemoveAt(rentedCars.Count-1);
        }
        else
        {
            Console.WriteLine("No car to delete from rent");
        }
        
    }
    public static void UndoLastAction(int lastAction, List<(CarRental coche, int dias)> rentedCars, CarRental car, 
    List<(CarRental coche, int dias)> deletedRentedCar)
    {
                
        switch(lastAction)
            {
                case 0: Console.WriteLine("No action that can be undone"); break;
                case 2: UndoDeleteCar(); break; //Borra el ultimo coche, el del indice más grande
                case 3: UndoAddCar(car); break; //Añade el ultimo coche que se elimino
                case 4: UndoUnRent(rentedCars); break; //Quita la renta del ultimo coche de la lista
                case 5: UndoUnRentCar(rentedCars, deletedRentedCar); break; //Añade el ultimo coche borrado de la lista de coches rentados
            }

        Console.WriteLine("Last action undone");
    }
}