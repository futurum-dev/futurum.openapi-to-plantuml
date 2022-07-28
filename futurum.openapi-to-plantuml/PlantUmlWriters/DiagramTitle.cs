using Microsoft.OpenApi.Models;

namespace Futurum.OpenApiToPlantuml;

public record DiagramTitle(OpenApiDocument OpenApiDocument) : IPlantUmlWriter
{
    public override string ToString() =>
        $"title {OpenApiDocument.Info.Title} v{OpenApiDocument.Info.Version}";
}