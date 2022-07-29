namespace Futurum.OpenApiToPlantuml;

public record DiagramTheme(string Theme) : IPlantUmlWriter
{
    public override string ToString() =>
        $"!theme {Theme}";
}