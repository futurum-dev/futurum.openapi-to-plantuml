namespace Futurum.OpenApiToPlantuml;

public record ClassResponse(string Title) : IPlantUmlWriter
{
    public override string ToString() =>
        $"class \"{Title}\" <<Response>> {{ }}";
}