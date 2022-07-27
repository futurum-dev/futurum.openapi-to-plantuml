namespace Futurum.OpenApiToPlantuml;

public record Footer(string DocumentType) : IPlantUmlWriter
{
    public override string ToString() =>
        $"footer {DocumentType} diagram - futurum.openapi-to-plantuml";
}