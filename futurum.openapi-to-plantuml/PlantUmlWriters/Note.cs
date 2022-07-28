using System.Text;

namespace Futurum.OpenApiToPlantuml;

public record Note(string Title, string Description) : IPlantUmlWriter
{
    public override string ToString()
    {
        var stringBuilder = new StringBuilder();

        stringBuilder.AppendLine($"note bottom of {Title}");
        stringBuilder.AppendLine(Description);
        stringBuilder.AppendLine("end note");

        return stringBuilder.ToString();
    }
}