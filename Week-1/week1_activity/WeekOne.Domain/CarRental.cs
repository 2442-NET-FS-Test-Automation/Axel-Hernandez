namespace WeekOne.Domain;
using WeekOne.Domain;

public class CarRental : CarBase
{
    public int DayCost { get; private set; }
    public int RentalPeriod { get; private set; }
    public bool IsAvailable { get; private set; }
    //define constructor with inheritance
    public CarRental(string brand, string model, int dayCost, int rentalPeriod, bool isAvailable) : base(brand, model){
        DayCost = dayCost;
        RentalPeriod = rentalPeriod;
        IsAvailable = isAvailable;
    }


    //Override Describe()
    public override string Describe()
    {
        return $"CAR INFO: Brand: {Brand}, Model: {Model}, Daily cost: {DayCost}, Minimum rental period: {RentalPeriod}, Is available: {IsAvailable}";
    }

    public override int CalculateFee(int days, CarRental car)
    {
        return days * car.DayCost;
    }

    public void ChangeStatus(bool isAvailable){
        IsAvailable = isAvailable;
    }


}