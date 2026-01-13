namespace WineApi.Attributes;

[AttributeUsage(AttributeTargets.Property)]
public class DefaultSortAttribute : Attribute
{
    public string Sort { get; }

    public DefaultSortAttribute(string sort)
    {
        Sort = sort;
    }
}