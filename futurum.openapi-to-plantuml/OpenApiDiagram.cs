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

        stringBuilder.Write(new Title(openApiDocument));
        stringBuilder.Write(new Footer("OpenApi"));

        foreach (var (openApiPathItemKey, openApiPathItem) in openApiDocument.Paths)
        {
            stringBuilder.AppendLine($"Container_Boundary({SanitiseOpenApiPathKey(openApiPathItemKey)}, \"{openApiPathItemKey}\") {{");
            foreach (var (operationType, openApiOperation) in openApiPathItem.Operations)
            {
                var component = new Component($"{SanitiseOpenApiPathKey(openApiPathItemKey)}_{operationType.ToString().ToLower()}",
                                              openApiOperation.OperationId,
                                              $"REST ApiEndpoint - {operationType.ToString().ToUpper()}",
                                              string.Empty);

                stringBuilder.Write(component);

                if (!string.IsNullOrEmpty(openApiOperation.Description))
                {
                    stringBuilder.Write(new Note($"{SanitiseOpenApiPathKey(openApiPathItemKey)}_{operationType.ToString().ToLower()}", SanitiseOpenApiOperationDescription(openApiOperation)));
                }
            }

            stringBuilder.AppendLine($"}}");
        }

        stringBuilder.Append(boilerplateEnd);

        return stringBuilder.ToString();
    }
}