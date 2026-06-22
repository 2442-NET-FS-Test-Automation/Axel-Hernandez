using Serilog;

namespace WeekOne.Domain;

public partial class Commands
{
    public static void UndoLastAction(int lastAction, List<(CarRental coche, int dias)> rentedCars, CarRental car, 
    List<(CarRental coche, int dias)> deletedRentedCar)
    {
        Log.Information("Undo Last action =>{lastAction}", lastAction);
        switch(lastAction)
            {
                case 0: Console.WriteLine("No action that can be undone"); break;
                case 2: UndoDeleteCar(); break;
                case 3: UndoAddCar(car); break;
                case 4: UndoRent(rentedCars); break;
                case 5: UndoUnRentCar(rentedCars, deletedRentedCar); break;
            }

        Console.WriteLine("Last action undone");
        Log.Information("Action Undone");
    }

    public static void UndoAddCar(CarRental coche)
    {
        if(coche is not null)
        {
            CarRental newCar = new CarRental(coche.Brand, coche.Model, coche.DayCost, coche.RentalPeriod, coche.Status);
            _repository.Add(newCar);
            Log.Information("Add Car via UndoAddCar");
        }
        else
        {
            Console.WriteLine("No car to be added");
            Log.Warning("No car to be added via UndoAddCar");
        }
        
    }

    public static void UndoDeleteCar()
    {
        
        try
        {
            _repository.RemoveLast();
        }catch(NoCarFoundException ex)
        {
            Console.WriteLine(ex.Message);
            Log.Error("Error at UndoDeleteCar: {ex.Message}",ex.Message);
            return;
        }
            
        Log.Information("Last car deleted via UndoDeleteCar");
        Console.WriteLine("Car deleted successfully");
    
    }
    
    public static void UndoUnRentCar(List<(CarRental coche, int dias)> rentedCars, List<(CarRental coche, int dias)> ToRentCar)
    {
        if(ToRentCar.Count > 0)
        {
            ToRentCar[0].coche.ChangeStatus(CarStatus.Rented);
            rentedCars.Add((ToRentCar[0].coche,ToRentCar[0].dias));
            Log.Information("Rerent a car with id {coche.Id} via UndoUnrentCar", ToRentCar[0].coche.Id);
        }
        else
        {
            Console.WriteLine("No car to rent again");
            Log.Warning("Cant UndoUnRentCar");
        }
        
    }

    public static void UndoRent(List<(CarRental coche, int dias)> rentedCars)
    {
        if(rentedCars.Count > 0)
        {
            CarRental coche = rentedCars[rentedCars.Count-1].coche;
            coche.ChangeStatus(CarStatus.Available);
            rentedCars.RemoveAt(rentedCars.Count-1);
            Log.Information("Undo rent a car via UndoRent");
        }
        else
        {
            Console.WriteLine("No car to delete from rent");
            Log.Warning("No cars to unrent via UndoRent");
        }
        
    }

}