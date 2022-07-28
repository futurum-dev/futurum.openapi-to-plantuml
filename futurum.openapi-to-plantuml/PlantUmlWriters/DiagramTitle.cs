using Microsoft.OpenApi.Models;

namespace Futurum.OpenApiToPlantuml;

public record DiagramTitle(OpenApiDocument OpenApiDocument) : IPlantUmlWriter
{
    public override string ToString()
    {
        var version = OpenApiDocument.Info.Version.ToLowerInvariant().StartsWith("v") 
            ? OpenApiDocument.Info.Version 
            : $"v{OpenApiDocument.Info.Version}";
        
        return $"title {OpenApiDocument.Info.Title} {version}";
    }
}