namespace WeekOne.Domain;

public class Inventory
{
    //List of cars, with initial items
    public static List<CarRental> CarsInStock = new()
    {
        new CarRental("Honda", "Accord", 40, 3, true),
        new CarRental("Toyota", "Camry", 50, 3, true),
        new CarRental("Hunday", "Creta", 45, 2, true),
        new CarRental("Kia", "Rio", 33, 5, true)
    };

    public static CarRental GetCarById(int id)
    {
        foreach(CarRental car in CarsInStock)
        {
            if(car.Id == id)
            {
                return car;
            }
        }
        return null;
    }

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