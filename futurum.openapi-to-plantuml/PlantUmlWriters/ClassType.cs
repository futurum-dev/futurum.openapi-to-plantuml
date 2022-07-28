using System.Text;

namespace Futurum.OpenApiToPlantuml;

public record ClassType(string Title) : IPlantUmlWriter
{
    private readonly List<ClassField> _fields = new();

    public ClassType AddField(ClassField field)
    {
        _fields.Add(field);

        return this;
    }
    
    public override string ToString()
    {
        var stringBuilder = new StringBuilder();

        stringBuilder.AppendLine($"class \"{Title}\" {{");

        foreach (var field in _fields)
        {
            stringBuilder.Write(field);
        }
        
        stringBuilder.AppendLine($"}}");
        
        return stringBuilder.ToString();
    }
}