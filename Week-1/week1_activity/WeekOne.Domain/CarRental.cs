namespace WeekOne.Domain;
using WeekOne.Domain;

public class CarRental : CarBase
{
    public int DayCost { get; private set; }
    public int RentalPeriod { get; private set; }
    public CarStatus Status { get; private set; }
    public bool IsAvailable => Status == CarStatus.Available;
    //define constructor with inheritance
    public CarRental(string brand, string model, int dayCost, int rentalPeriod, CarStatus status) : base(brand, model){
        DayCost = dayCost;
        RentalPeriod = rentalPeriod;
        Status = status;
    }


    //Override Describe()
    public override string Describe()
    {
        return $"CAR INFO: \n\tBrand: {Brand}, \n\tModel: {Model}, \n\tDaily cost: {DayCost}, \n\tMinimum rental period: {RentalPeriod}, \n\tStatus: {Status}";
    }

    public override int CalculateFee(int days, CarRental car)
    {
        return days * car.DayCost;
    }

    public void ChangeStatus(CarStatus status){
        // IsAvailable = isAvailable;
        Status = status;
    }


}