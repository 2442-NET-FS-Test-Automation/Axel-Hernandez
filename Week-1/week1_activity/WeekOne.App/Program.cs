using System.Net.ServerSentEvents;
using WeekOne.Domain;
namespace WeekOne.App;


public class Program
{
    public static void Main()
    {

        bool running = true;
        List<(CarRental coche, int dias)> rentedCars = new(); //Lista de coches rentador por el usuario. Dupla para tener coche y dias
        int lastAction = 0;
        int[] possibleAction = {2,3,4,5};
        CarRental coche = new CarRental("","",0,0,true);
        List<(CarRental coche, int dias)> UnRentedCars = new();
        
        //list<int> acciones;
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
                //case 10, perform actions
            }
            if(possibleAction.Contains(choice)) //Si es de las acciones que se pueden deshacer se guarda
                lastAction = choice;
        }

        Commands.RentedCarsInfo(rentedCars);
        Console.WriteLine("Good Bye!");
    }
    
}