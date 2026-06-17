namespace WeekOne.Domain;

public abstract class CarBase
{
    
    public string Brand { get; private set; }
    public string Model { get; private set; }
    

    //Static properties
    private static int _nextId = 1;
    public int Id { get; private set; }

    //----- Constructor -----
    public CarBase(string brand, string model)
    {
        
        Brand = brand;
        Model = model;
        Id = _nextId++;
    }


    // ----- Abstract Methods -----
    public abstract string Describe();

    public abstract int CalculateFee(int days, CarRental car);


    //Override ToString() to show account's info
    public override string ToString()
    {
        return Describe();
    }

}