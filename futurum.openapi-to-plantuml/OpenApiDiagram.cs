using System.Text;

using Microsoft.OpenApi.Models;

namespace Futurum.OpenApiToPlantuml;

public class OpenApiDiagram
{
    private const string BoilerplateStart = @"@startuml OpenApi diagram

!include https://raw.githubusercontent.com/plantuml-stdlib/C4-PlantUML/master/C4_Component.puml

";

    private const string BoilerplateEnd = @"

@enduml";
    
    public static string Create(OpenApiDocument openApiDocument, DiagramConfiguration configuration)
    {
        var stringBuilder = new StringBuilder();
        stringBuilder.Append(BoilerplateStart);

        if (!string.IsNullOrEmpty(configuration.Theme))
        {
            stringBuilder.Write(new DiagramTheme(configuration.Theme));
        }

        stringBuilder.Write(new DiagramTitle(openApiDocument));
        stringBuilder.Write(new DiagramFooter("OpenApi"));

        foreach (var (openApiPathItemKey, openApiPathItem) in openApiDocument.Paths)
        {
            var containerBoundaryId = SanitiseContainerBoundaryId(openApiPathItemKey);

            var containerBoundary = new ContainerBoundary(containerBoundaryId, openApiPathItemKey);

            foreach (var (operationType, openApiOperation) in openApiPathItem.Operations)
            {
                var componentId = $"{containerBoundaryId}_{operationType.ToString().ToLower()}";

                var title = !string.IsNullOrEmpty(openApiOperation.Summary)
                    ? !string.IsNullOrEmpty(openApiOperation.OperationId) ? $"{openApiOperation.Summary}\\r[{openApiOperation.OperationId}]" : openApiOperation.OperationId
                    : openApiOperation.OperationId;

                var protocol = $"REST ApiEndpoint - {operationType.ToString().ToUpper()}";

                var component = new Component(componentId, title, protocol);

                if (configuration.ShowNotes && !string.IsNullOrEmpty(openApiOperation.Description))
                {
                    component.AddNote(new Note(componentId, SanitiseNote(openApiOperation)));
                }

                containerBoundary.AddComponent(component);
            }

            stringBuilder.Write(containerBoundary);
        }

        stringBuilder.Append(BoilerplateEnd);

        return stringBuilder.ToString();
    }

    private static string SanitiseContainerBoundaryId(string openApiPathKey)
    {
        var result = openApiPathKey.Replace("/", "_")
                                   .Replace("{", string.Empty)
                                   .Replace("}", string.Empty);

        if (result.StartsWith("_"))
        {
            result = result.Substring(1, result.Length - 1);
        }

        return result;
    }

    private static string SanitiseNote(OpenApiOperation openApiOperation) =>
        // TODO : We should transform HTML to markdown and not do this hack stuff
        openApiOperation.Description
                        .Replace("<br />", Environment.NewLine)
                        .Replace("<br/>", Environment.NewLine)
                        .Replace("<strong>", "**")
                        .Replace("</strong>", "**")
                        .Replace("<ul>", "")
                        .Replace("</ul>", "")
                        .Replace("</ ul>", "")
                        .Replace("<li>", "\r*")
                        .Replace("</li>", "")
                        .Replace("</ li>", "");
}