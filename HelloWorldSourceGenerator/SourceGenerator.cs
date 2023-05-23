namespace HelloWorldSourceGenerator;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Text;
using System.Text;

[Generator]
public class SourceGenerator : IIncrementalGenerator
{
    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
        /*language=cs*/
        const string source = """
namespace HelloWorldApp;

public static class Hello
{
    public static void World()
    {
        System.Console.WriteLine("Hello world");
    }
}
""";
        context.RegisterPostInitializationOutput(ctx => ctx.AddSource("HelloWorld.g.cs", SourceText.From(source, Encoding.UTF8)));
    }
}
