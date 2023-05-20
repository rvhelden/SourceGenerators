using Library;

namespace Api;

[EntityController("/Demo2")]
[Cloneable]
public partial class Demo
{
    public string Name { get; set; }
}