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
}