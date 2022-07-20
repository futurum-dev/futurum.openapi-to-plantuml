using System.Text;

using Microsoft.OpenApi.Models;

namespace Futurum.OpenApiToPlantuml;

public class OpenApiDiagram
{
    public static string Create(OpenApiDocument openApiDocument)
    {
        string SanitiseOpenApiPathKey(string openApiPathKey) =>
            openApiPathKey
                        .Replace("/", "")
                        .Replace("{", "")
                        .Replace("}", "");

        string SanitiseOpenApiOperationDescription(OpenApiOperation openApiOperation) =>
            openApiOperation.Description
                             .Replace(Environment.NewLine, " ");

        const string boilerplateStart = @"@startuml OpenApi diagram
!theme blueprint

!include https://raw.githubusercontent.com/plantuml-stdlib/C4-PlantUML/master/C4_Component.puml

";

        const string boilerplateEnd = @"

@enduml";

        var stringBuilder = new StringBuilder();
        stringBuilder.Append(boilerplateStart);

        stringBuilder.AppendLine($"title {openApiDocument.Info.Title} v{openApiDocument.Info.Version}");
        stringBuilder.AppendLine("footer OpenApi diagram - futurum.openapi-to-plantuml");

        foreach (var (openApiPathItemKey, openApiPathItem) in openApiDocument.Paths)
        {
            stringBuilder.AppendLine($"Container_Boundary({SanitiseOpenApiPathKey(openApiPathItemKey)}, \"{openApiPathItemKey}\") {{");
            foreach (var (operationType, openApiOperation) in openApiPathItem.Operations)
            {
                stringBuilder.AppendLine($"Component({SanitiseOpenApiPathKey(openApiPathItemKey)}_{operationType.ToString().ToLower()}, " +
                                         $"\"{openApiOperation.OperationId}\", " +
                                         $"\"REST ApiEndpoint - {operationType.ToString().ToUpper()}\", " +
                                         $"\"\")");

                if (!string.IsNullOrEmpty(openApiOperation.Description))
                {
                    stringBuilder.AppendLine($"note bottom of {SanitiseOpenApiPathKey(openApiPathItemKey)}_{operationType.ToString().ToLower()}");
                    stringBuilder.AppendLine(SanitiseOpenApiOperationDescription(openApiOperation));
                    stringBuilder.AppendLine("end note");
                }
            }

            stringBuilder.AppendLine($"}}");
        }

        stringBuilder.Append(boilerplateEnd);

        return stringBuilder.ToString();
    }
}