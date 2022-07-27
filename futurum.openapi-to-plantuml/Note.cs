using System.Text;

namespace Futurum.OpenApiToPlantuml;

public record Note(string Title, string Description) : IPlantUmlWriter
{
    public override string ToString() =>
        $"note bottom of {Title}{Environment.NewLine}{Description}{Environment.NewLine}end note";
}