namespace TypescriptSourceGenerator;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;
using System.Text;

[Generator]
public class SourceGenerator : IIncrementalGenerator
{
    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
        var attribute = EmbeddedResource.RenderTemplate("Templates/Attribute.sbncs");
        context.RegisterPostInitializationOutput(ctx => ctx.AddSource("Attributes.g.cs", attribute));

        var cloneableClasses = context.SyntaxProvider.CreateSyntaxProvider(
        predicate: (x, _) => IsClassWithCloneable(x),
        transform: (x, _) => (symbol: (INamedTypeSymbol)x.SemanticModel.GetDeclaredSymbol(x.Node)!, node: (ClassDeclarationSyntax)x.Node)
        );

        context.RegisterSourceOutput(cloneableClasses, action: static (spc, source) => Execute(source.symbol, source.node, spc));
    }

    private static bool IsClassWithCloneable(SyntaxNode node)
    {
        return node is ClassDeclarationSyntax candidate && candidate.AttributeLists
            .SelectMany(list => list.Attributes)
            .Any(attribute => attribute.Name.ToString() is "TypescriptAttribute" or "Typescript");
    }

    private static void Execute(INamedTypeSymbol symbol, ClassDeclarationSyntax node, SourceProductionContext context)
    {
        var attributes = symbol.GetAttributes();
        // When the attribute only has the same name but lives in a different namespace then its a false positive
        if (attributes.All(x => x.AttributeClass?.ToString() != "TypescriptSourceGenerator.TypescriptAttribute"))
        {
            return;
        }

        var fields = node.ChildNodes().OfType<PropertyDeclarationSyntax>().Select(x => new Field(
        x.Identifier.ToString(),
        x.Type.ToString().ToLower()
        ));

        var content = EmbeddedResource.RenderTemplate("Templates/Typescript.sbntxt",
        new
        {
            Name = node.Identifier.ToString(),
            Fields = fields
        });

        /* language=csharp */
        var sourceGenerator = $$""""
namespace TypescriptSourceGenerator;

public static class {{node.Identifier}}TypescriptGenerator
{
    public static string Generate()
    {
        /* language=typescript */
        return """
{{content}}
""";
    }
}
"""";

        context.AddSource($"{node.Identifier}TypescriptGenerator.g.cs", SourceText.From(sourceGenerator, Encoding.UTF8));
    }
}

internal record Field(string Name, string Type);
