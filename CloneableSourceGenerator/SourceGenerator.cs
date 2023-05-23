namespace CloneableSourceGenerator;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

[Generator]
public class SourceGenerator : IIncrementalGenerator
{
    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
        var attribute = EmbeddedResource.RenderTemplate("Templates/Attribute.sbncs");
        context.RegisterPostInitializationOutput(ctx => ctx.AddSource("Attributes.g.cs", attribute));
        
        var cloneableClasses = context.SyntaxProvider.CreateSyntaxProvider(
        (x, _) => IsClassWithCloneable(x),
        (x, _) => (symbol: (INamedTypeSymbol)x.SemanticModel.GetDeclaredSymbol(x.Node)!, node: (ClassDeclarationSyntax)x.Node)
        );
        
        context.RegisterSourceOutput(cloneableClasses, static (spc, source) => Execute(source.symbol, source.node, spc));
    }

    private static bool IsClassWithCloneable(SyntaxNode node)
    {
        return node is ClassDeclarationSyntax candidate && candidate.AttributeLists
            .SelectMany(list => list.Attributes)
            .Any(attribute => attribute.Name.ToString() is "CloneableAttribute" or "Cloneable");
    }

    static void Execute(INamedTypeSymbol symbol, ClassDeclarationSyntax node, SourceProductionContext context)
    {
        var attributes = symbol.GetAttributes();
        // When the attribute only has the same name but lives in a different namespace then its a false positive
        if (attributes.All(x => x.AttributeClass?.ToString() != "CloneableSourceGenerator.CloneableAttribute"))
        {
            return;
        }

        var content = EmbeddedResource.RenderTemplate("Templates/Cloneable.sbncs", new
        {
            RootNamespace = symbol.ContainingNamespace.ToString(),
            Entity = node.Identifier.ToString(),
            Properties = node.ChildNodes().OfType<PropertyDeclarationSyntax>().Select(x => x.Identifier.ToString())
        });

        context.AddSource($"{node.Identifier}Cloneable.g.cs", content);
    }
}
