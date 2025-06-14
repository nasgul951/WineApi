namespace WineApi.Model.Attributes;

public class DefaultSortAttribute : Attribute
{
    public string Sort { get; }

    public DefaultSortAttribute(string sort)
    {
        Sort = sort;
    }
}