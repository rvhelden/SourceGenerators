namespace Net8;

using SampleAlias = (string FirstName, string LastName);

public class TypeAlias
{
    public TypeAlias()
    {
        SampleAlias person = ("Ronald", "van Helden");
        Console.WriteLine($"Type alias: {person.FirstName} {person.LastName}");
    }
}
