namespace Library;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;
using Scriban;
using System.Collections.Immutable;
using System.Text;

[Generator]
public class SourceGenerator : IIncrementalGenerator
{
    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
        var content = EmbeddedResource.GetContent("Templates/Attribute.sbncs");
        var template = Template.Parse(content);
        
        var attribute = template.Render();
        context.RegisterPostInitializationOutput(ctx => ctx.AddSource("EntityControllerAttribute.g.cs", SourceText.From(attribute, Encoding.UTF8)));
        
        // Do a simple filter for enums
        IncrementalValuesProvider<ClassDeclarationSyntax> classDeclarations = context.SyntaxProvider
            .CreateSyntaxProvider(
            predicate: static (s, _) => IsSyntaxTargetForGeneration(s), // select enums with attributes
            transform: static (ctx, _) => GetSemanticTargetForGeneration(ctx)) // sect the enum with the [EnumExtensions] attribute
            .Where(static m => m is not null)!; // filter out attributed enums that we don't care about

        // Combine the selected enums with the `Compilation`
        IncrementalValueProvider<(Compilation, ImmutableArray<ClassDeclarationSyntax>)> compilationAndClasses
            = context.CompilationProvider.Combine(classDeclarations.Collect());

        // Generate the source using the compilation and enums
        context.RegisterSourceOutput(compilationAndClasses, static (spc, source) => Execute(source.Item1, source.Item2, spc));
    }
    
    private static bool IsSyntaxTargetForGeneration(SyntaxNode node)
    {
        return node is ClassDeclarationSyntax
        {
            AttributeLists.Count: > 0
        } candidate && candidate.AttributeLists
            .SelectMany(list => list.Attributes)
            .Any(attribute => attribute.Name.ToString() is "CloneableAttribute" or "Cloneable");
    }
    
    static ClassDeclarationSyntax? GetSemanticTargetForGeneration(GeneratorSyntaxContext context)
    {
        // we know the node is a EnumDeclarationSyntax thanks to IsSyntaxTargetForGeneration
        var declaration = (ClassDeclarationSyntax)context.Node;

        // loop through all the attributes on the method
        foreach (AttributeListSyntax attributeListSyntax in declaration.AttributeLists)
        {
            foreach (AttributeSyntax attributeSyntax in attributeListSyntax.Attributes)
            {
                if (context.SemanticModel.GetSymbolInfo(attributeSyntax).Symbol is not IMethodSymbol attributeSymbol)
                {
                    // weird, we couldn't get the symbol, ignore it
                    continue;
                }

                INamedTypeSymbol attributeContainingTypeSymbol = attributeSymbol.ContainingType;
                string fullName = attributeContainingTypeSymbol.ToDisplayString();

                if (fullName == "Library.CloneableAttribute")
                {
                    // return the enum
                    return declaration;
                }
            }
        }

        // we didn't find the attribute we were looking for
        return null;
    }   
    
    static void Execute(Compilation compilation, ImmutableArray<ClassDeclarationSyntax> targets, SourceProductionContext context)
    {
        if (targets.IsDefaultOrEmpty)
        {
            // nothing to do yet
            return;
        }

        foreach (var target in targets)
        {
            var semanticModel = compilation.GetSemanticModel(target.SyntaxTree);
            var symbol = semanticModel.GetDeclaredSymbol(target);
            var content = EmbeddedResource.GetContent("Templates/Cloneable.sbncs");
            var template = Template.Parse(content);
            
            var attribute = symbol.GetAttributes().First(x => x.AttributeClass.Name == "EntityControllerAttribute");
            var basePath = attribute.ConstructorArguments.First().Value?.ToString() ?? "";
            
            var routeFactory = template.Render(new Model
            {
                RootNamespace = symbol.ContainingNamespace.ToString(),
                Entity = target.Identifier.ToString(),
                BasePath = basePath
            }, member => member.Name);
            context.AddSource($"{target.Identifier}RouteFactory.g.cs", SourceText.From(routeFactory, Encoding.UTF8));
        }
    }
}

public class Model
{
    public string RootNamespace { get; set; }
    public string Entity { get; set; }
    public string BasePath { get; set; }
}