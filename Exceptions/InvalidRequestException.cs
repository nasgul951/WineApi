namespace WineApi.Exceptions;

public class InvalidRequestException : Exception
{
    public InvalidRequestException(string field) 
    : base($"Invalid request for {field}.")
    {
    }
}