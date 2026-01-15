namespace WineApi.Extensions;

public static class StringEx
{
    /// <summary>
    /// Converts a string to PascalCase, source string is expected to be in camelCase
    /// </summary>
    /// <param name="value"></param>
    /// <returns></returns>
    public static string ToPascalCase(this string? value)
    {
        if (string.IsNullOrEmpty(value))
        {
            return string.Empty;
        }

        return char.ToUpper(value[0]) + value.Substring(1);
    }
}