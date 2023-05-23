namespace JsonSourceGenerator;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System.Text.Json;

[Generator]
public class SourceGenerator : IIncrementalGenerator
{
    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
        var entitiesJson = context.AdditionalTextsProvider.Where(x => x.Path.EndsWith("Entities.json"));

        context.RegisterSourceOutput(entitiesJson, GenerateClasses);
    }

    static void GenerateClasses(SourceProductionContext context, AdditionalText entitiesSource)
    {
        var content = entitiesSource.GetText().ToString();
        var entities = JsonSerializer.Deserialize<Entity[]>(content, new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        });

        foreach (var entity in entities)
        {
            var entityContent = EmbeddedResource.RenderTemplate("Templates/JsonClass.sbncs", entity);
            context.AddSource($"{entity.Name}.g.cs", entityContent);
        }
    }
}

record Entity(string Name, Property[] Properties);
record Property(string Name, string Type);