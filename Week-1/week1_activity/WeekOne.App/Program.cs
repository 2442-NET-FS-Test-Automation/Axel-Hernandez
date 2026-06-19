using System.Net.ServerSentEvents;
using WeekOne.Domain;
namespace WeekOne.App;


public class Program
{
    public static void Main()
    {

        bool running = true;
        List<(CarRental coche, int dias)> rentedCars = new(); //Lista de coches rentador por el usuario. Dupla para tener coche y dias
        

        Console.WriteLine("Welcome!");
        while (running)
        {            
            Commands.PrintMenu();
            int choice = int.Parse(Console.ReadLine());   // naive: may throw on bad input — fine for now
            switch (choice)
            {
                case 1: Commands.PrintInventory(); break;
                case 2: Commands.AddCar(); break;
                case 3: Commands.RentCar(rentedCars); break;
                case 4: Commands.UnRent(rentedCars); break;
                case 5: Commands.RentedCarsInfo(rentedCars); break;
                case 0: running = false; break;
            }
        }

        Commands.RentedCarsInfo(rentedCars);
        Console.WriteLine("Good Bye!");
    }
    
}