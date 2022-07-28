namespace Futurum.OpenApiToPlantuml;

public record DiagramFooter(string DocumentType) : IPlantUmlWriter
{
    public override string ToString() =>
        $"footer {DocumentType} diagram - futurum.openapi-to-plantuml";
}