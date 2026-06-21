namespace WeekOne.Domain;

public readonly struct RentalQuote
{
    public int Days { get; }
    public int DailyRate { get; }
    public int TotalFee { get; }

    public RentalQuote(int days, int dailyRate)
    {
        Days = days;
        DailyRate = dailyRate;
        TotalFee = days * dailyRate;
    }

    public override string ToString()
    {
        return $"{Days} days x ${DailyRate}/day = ${TotalFee}";
    }
}