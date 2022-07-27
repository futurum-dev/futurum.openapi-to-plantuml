using System.Text;

namespace Futurum.OpenApiToPlantuml;

public record ClassField(string Title, string Type, bool IsRequired = true) : IPlantUmlWriter
{
    public override string ToString() =>
        $"{{field}} {Title} : {Type}{FieldRequired(IsRequired)}";

    private static string FieldRequired(bool required) =>
        required ? string.Empty : " {O}";
}