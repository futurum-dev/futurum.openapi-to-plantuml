using System.Text;

namespace Futurum.OpenApiToPlantuml;

public record ClassField(string Title, string Type, bool IsRequired = true) : IPlantUmlWriter
{
    public override string ToString()
    {
        var stringBuilder = new StringBuilder();
        
        stringBuilder.Append($"{{field}} {Title} : {Type}{FieldRequired(IsRequired)}");

        return stringBuilder.ToString();
    }

    private static string FieldRequired(bool required) =>
        required ? "" : " {O}";
}