namespace WeekOne.Domain;

public interface ICarRepository
{
    int Count { get; }
    IReadOnlyList<CarRental> GetAll();
    CarRental? GetById(int id);
    void Add(CarRental car);
    void Remove(CarRental car);
    void RemoveLast();

}