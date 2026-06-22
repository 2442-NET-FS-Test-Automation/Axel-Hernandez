namespace WeekOne.Domain;

public class InMemoryCarRepository : ICarRepository
{
    private readonly List<CarRental> _cars = new()
    {
        new CarRental("Honda", "Accord", 40, 3, CarStatus.Available),
        new CarRental("Toyota", "Camry", 50, 3, CarStatus.Available),
        new CarRental("Hunday", "Creta", 45, 2, CarStatus.Available),
        new CarRental("Kia", "Rio", 33, 5, CarStatus.Available)
    };

    public int Count => _cars.Count;

    public IReadOnlyList<CarRental> GetAll() => _cars;

    public CarRental? GetById(int id)
    {
        foreach(CarRental car in _cars)
        {
            if (car.Id == id)
            {
                return car;
            }
        }
        return null;
    }


    public void Add(CarRental car) => _cars.Add(car);

    public void Remove(CarRental car)
    {
        if (GetById(car.Id) is null)
        {
            throw new NoCarFoundException($"No car found with id {car.Id}");
        }

        _cars.Remove(car);
    }

    public void RemoveLast()
    {
        if (Count == 0)
        {
            throw new NoCarFoundException("No Last Car found to be removed");
        }

        _cars.RemoveAt(Count - 1);
    }


}