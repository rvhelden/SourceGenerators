namespace Net8;

public class Person(string firstName, string lastName)
{
    public DateTime DateOfBirth { get; init; }

    public Person(string firstName, string lastName, DateTime dateOfBirth)
        : this(firstName, lastName)
    {
        DateOfBirth = dateOfBirth;
    }

    public override string ToString() => $"{firstName} {lastName}";
}
