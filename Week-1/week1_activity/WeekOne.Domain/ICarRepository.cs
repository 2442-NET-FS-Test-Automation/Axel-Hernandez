namespace WeekOne.Domain;

public interface ICarRepository
{
    int Count { get; }
    IEnumerable<CarRental> GetAll(); // was IReadOnlyList<CarRental>
    CarRental? GetById(int id);
    void Add(CarRental car);
    void Remove(CarRental car);
    void RemoveLast();

}