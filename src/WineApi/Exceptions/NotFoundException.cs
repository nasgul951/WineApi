namespace WineApi.Exceptions;

public class NotFoundException : Exception
{
    public NotFoundException(string field) 
        : base($"{field} not found")
    { 
    }
}
