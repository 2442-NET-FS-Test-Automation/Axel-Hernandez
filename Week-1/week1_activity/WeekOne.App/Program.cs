using System.Net.ServerSentEvents;
using WeekOne.Domain;
namespace WeekOne.App;


public class Program
{
    public static void Main()
    {

        var running = true;
        while (running)
        {
            //welcome message to menu- pending to add
            // -- add sumary of final rental - total bill, days, car
            // -- not to rent unavailable cars
            // -- make sure car is available - otheriwse throw exception
            //  not to rent below the minimum rental period
            

            Commands.PrintMenu();
            int choice = int.Parse(Console.ReadLine());   // naive: may throw on bad input — fine for now
            switch (choice)
            {
                case 1: Commands.PrintInventory(); break;
                case 2: Commands.AddCar(); break;
                case 3: Commands.RentCar(); break;
                case 0: running = false; break;
            }
        }

    }
    
}