namespace Futurum.OpenApiToPlantuml;

public class DiagramConfiguration
{
    public string Theme { get; set; } = string.Empty;
    public bool ShowNotes { get; set; } = false;

    public static readonly DiagramConfiguration Default = new();
}