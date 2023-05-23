namespace SwaggerSourceGenerator;

using Microsoft.CodeAnalysis;
using System.Text.Json;

[Generator]
public class SourceGenerator : IIncrementalGenerator
{
    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
        var entitiesJson = context.AdditionalTextsProvider.Where(x => x.Path.EndsWith("openapi.json"));

        context.RegisterSourceOutput(entitiesJson, GenerateClasses);
    }

    static void GenerateClasses(SourceProductionContext context, AdditionalText openapi)
    {
        var content = openapi.GetText().ToString();
        var document = JsonDocument.Parse(content);

        var paths = document.RootElement.GetProperty("paths");
        var schemas = document.RootElement.GetProperty("components").GetProperty("schemas");

        var classes = new List<Class>();
        var operations = new List<Operation>();
        foreach (var path in paths.EnumerateObject())
        {
            foreach (var method in path.Value.EnumerateObject())
            {
                var operationId = method.Value.GetProperty("operationId").GetString();
                var argument = string.Empty;

                if (method.Value.TryGetProperty("requestBody", out var requestBody))
                {
                    var schemaRef = requestBody.GetProperty("content")
                        .GetProperty("application/json")
                        .GetProperty("schema")
                        .GetProperty("$ref")
                        .GetString();

                    var element = schemaRef.Split('/').Last();
                    var schema = schemas.GetProperty(element);

                    var properties = new List<Property>();
                    foreach (var property in schema.GetProperty("properties").EnumerateObject())
                    {
                        var type = "object";
                        if (property.Value.TryGetProperty("type", out var openApiType))
                        {
                            type = openApiType.GetString() switch
                            {
                                "integer" => "int",
                                "array" => "object[]",
                                "string" => "string",
                            };
                        }
                        properties.Add(new Property(property.Name, type));
                    }
            
                    classes.Add(new Class(element, properties));
                    argument = element;
                }
        
                operations.Add(new Operation(operationId, path.Name, method.Name, argument));
            }
        }

        foreach (var @class in classes.GroupBy(x => x.Name).Select(x => x.First()))
        {
            var pocoContent = EmbeddedResource.RenderTemplate("Templates/PocoClass.sbncs", @class);
            context.AddSource($"{@class.Name}.Poco.g.cs", pocoContent);
        }
        
        var clientContent = EmbeddedResource.RenderTemplate("Templates/SwaggerClient.sbncs", new { Operations = operations });
        context.AddSource("SwaggerClient.g.cs", clientContent);
    }
}

record Operation(string Name, string Url, string Method, string Argument);
record Class(string Name, IList<Property> Properties);
record Property(string Name, string Type);