namespace WeekOne.Domain;

public class NoCarFoundException: Exception
{
    public NoCarFoundException() : base("No car found exception")
    {
        
    }

    public NoCarFoundException(string message): base(message)
    {
        
    }
}