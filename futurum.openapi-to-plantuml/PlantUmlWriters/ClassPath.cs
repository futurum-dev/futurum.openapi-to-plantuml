using System.Text;

namespace Futurum.OpenApiToPlantuml;

public record ClassPath(string Title) : IPlantUmlWriter
{
    private readonly List<ClassField> _fields = new();

    public ClassPath AddField(ClassField field)
    {
        _fields.Add(field);

        return this;
    }
    
    public override string ToString()
    {
        var stringBuilder = new StringBuilder();

        stringBuilder.AppendLine($"class \"{Title}\" <<Path>> {{");

        foreach (var field in _fields)
        {
            stringBuilder.Write(field);
        }
        
        stringBuilder.AppendLine($"}}");
        
        return stringBuilder.ToString();
    }
}