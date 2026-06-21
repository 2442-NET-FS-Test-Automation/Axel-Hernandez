using System.Net.ServerSentEvents;
using WeekOne.Domain;
using Serilog;
namespace WeekOne.App;


public class Program
{
    public static async Task Main()
    {

        bool running = true;
        List<(CarRental coche, int dias)> rentedCars = new(); //Lista de coches rentador por el usuario. Dupla para tener coche y dias
        List<(CarRental coche, int dias)> UnRentedCars = new();
        int lastAction = 0;
        int[] possibleAction = {2,3,4,5};
        CarRental coche = null;
        Console.WriteLine("Welcome!");

        //Comenzar serilog
        Log.Logger = new LoggerConfiguration()
            .MinimumLevel.Information()
            .WriteTo.Console()
            .WriteTo.File("logs/log.txt")
            .CreateLogger();
        Log.Information($"Start of App");
        while (running)
        {            
            Commands.PrintMenu();
            int choice;
            try
            {
                choice = int.Parse(Console.ReadLine());
            }catch(Exception ex)
            {
                Console.WriteLine("Exception: "+ex.Message);
                choice = 10;
            }
            
            Log.Information($"Menu with option {choice}");
            switch (choice)
            {
                case 0: running = false; break;
                case 1: Commands.PrintInventory(); break;
                case 2: Commands.AddCar(); break;
                case 3: coche = Commands.DeleteCar(); break;
                case 4: Commands.RentCar(rentedCars); break;
                case 5: UnRentedCars = Commands.UnRent(rentedCars); break;
                case 6: Commands.RentedCarsInfo(rentedCars); break;
                case 7: Commands.UndoLastAction(lastAction, rentedCars, coche, UnRentedCars); lastAction = 0; break;
                case 8: Commands.WaitingListInfo(); break;
                case 9: await Commands.AsyncHttpDemo(); break; 
                case 10: Commands.SearchBrowseMenuLoop(); break;
                default: Console.WriteLine("Not a valid option"); break;
            }
            if(possibleAction.Contains(choice)) //Si es de las acciones que se pueden deshacer se guarda
                lastAction = choice;
        }

        Commands.RentedCarsInfo(rentedCars);
        
        Console.WriteLine("Good Bye!");
        Log.Information("Closed App");
        Log.CloseAndFlush();
    }
    
}
