namespace JsonSourceGenerator;

using Microsoft.CodeAnalysis.Text;
using Scriban;
using System.Reflection;
using System.Text;

public static class EmbeddedResource
{
    public static string GetContent(string relativePath)
    {
        var baseName = Assembly.GetExecutingAssembly().GetName().Name;
        var resourceName = relativePath
            .TrimStart('.')
            .Replace(Path.DirectorySeparatorChar, '.')
            .Replace(Path.AltDirectorySeparatorChar, '.');

        using var stream = Assembly.GetExecutingAssembly().GetManifestResourceStream(baseName + "." + resourceName);

        if (stream == null)
        {
            throw new NotSupportedException();
        }

        using var reader = new StreamReader(stream);
        return reader.ReadToEnd();
    }

    public static SourceText RenderTemplate(string relativePath, object? model = null)
    {
        var content = EmbeddedResource.GetContent(relativePath);
        var template = Template.Parse(content);
        
        var renderedContent = template.Render(model, x => x.Name);
        return SourceText.From(renderedContent, Encoding.UTF8);
    }
}
