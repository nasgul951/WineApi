namespace WineApi.Exceptions;
public class ConflictException : Exception
{
    public ConflictException(string field)
        : base($"Duplicate {field}")
    {
    }
}
