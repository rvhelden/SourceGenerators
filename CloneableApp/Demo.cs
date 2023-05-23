namespace CloneableApp;

using CloneableSourceGenerator;

[Cloneable]
public partial class Demo
{
    public string Name { get; set; }
    public string LastName { get; set; }
    public string FirstName { get; set; }
}