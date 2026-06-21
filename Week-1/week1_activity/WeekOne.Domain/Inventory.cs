using System.ComponentModel;
using System.Reflection.Metadata.Ecma335;

namespace WeekOne.Domain;

public class Inventory
{
    //List of cars, with initial items
    readonly static List<CarRental> _CarsInStock = new()
    {
        new CarRental("Honda", "Accord", 40, 3, true),
        new CarRental("Toyota", "Camry", 50, 3, true),
        new CarRental("Hunday", "Creta", 45, 2, true),
        new CarRental("Kia", "Rio", 33, 5, true)
    };

    public static int Count => _CarsInStock.Count;
    public CarRental this[int index] => _CarsInStock[index];

    public static void Add(CarRental car)
    {
        _CarsInStock.Add(car);
    }
    
    public static bool Remove(CarRental car)
    {
        if(GetCarById(car.Id) is null)
        {
            throw new NoCarFoundException($"No car found with id {car.Id}");
        }
        else
        {
            _CarsInStock.Remove(car);
            return true;
        }
        
    } 

    public static bool RemoveLast()
    {
        if(Count > 0)
        {
            _CarsInStock.RemoveAt(Count-1);
            return true;
        }
        else
        {
            throw new NoCarFoundException("No Last Car found to be RemoveLast");
            return false;
        }
    }

    public static bool isEmpty => _CarsInStock.Count == 0;
    

    public static CarRental GetCarById(int id)
    {
        foreach(CarRental car in _CarsInStock)
        {
            if(car.Id == id)
            {
                return car;
            }
        }
        return null;
    }

    public static List<CarRental> GetInventory() =>  _CarsInStock;

    public static bool TryMoveWaitingListItem(int fromPosition, int toPosition)
    {
        int fromIndex = fromPosition - 1;
        int toIndex = toPosition - 1;

        if (fromIndex < 0 || fromIndex >= waitingList.Count)
            return false;

        if (toIndex < 0 || toIndex >= waitingList.Count)
            return false;

        if (fromIndex == toIndex)
            return true;

        CarRental car = waitingList[fromIndex];
        waitingList.RemoveAt(fromIndex);
        waitingList.Insert(toIndex, car);

        return true;
    }

    //List of waiting list
    public static List<CarRental> waitingList = new();
}
