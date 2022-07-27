using System.Text;

using Microsoft.OpenApi.Models;

namespace Futurum.OpenApiToPlantuml;

public class OpenApiTypeDiagram
{
    public static string Create(OpenApiDocument openApiDocument)
    {
        string SanitiseOpenApiOperationDescription(string description) =>
            description
                .Replace(Environment.NewLine, " ");

        string SanitiseOpenApiSchemaKey(string openApiSchemaKey) =>
            openApiSchemaKey
                .Replace("/", "")
                .Replace("{", "")
                .Replace("}", "");

        string FieldRequired(bool required) =>
            required ? "" : " {O}";

        const string boilerplateStart = @"@startuml OpenApi Type diagram
!theme blueprint

hide <<Path>> circle
hide <<Response>> circle
hide <<Parameter>> circle
hide empty methods
hide empty fields
set namespaceSeparator none

";

        const string boilerplateEnd = @"

@enduml";

        var stringBuilder = new StringBuilder();
        stringBuilder.Append(boilerplateStart);

        stringBuilder.AppendLine($"title {openApiDocument.Info.Title} v{openApiDocument.Info.Version}");
        stringBuilder.AppendLine("footer OpenApi Type diagram - futurum.openapi-to-plantuml");

        foreach (var (openApiPathItemKey, openApiPathItem) in openApiDocument.Paths)
        {
            foreach (var (operationType, openApiOperation) in openApiPathItem.Operations)
            {
                stringBuilder.AppendLine($"class \"{operationType.ToString().ToUpper()} {openApiPathItemKey}\" <<Path>> {{");

                foreach (var openApiParameter in openApiOperation.Parameters)
                {
                    if (openApiParameter.Schema.Items != null && openApiParameter.Schema.Items.Reference != null && openApiParameter.Schema.Type == "array")
                    {
                        stringBuilder.AppendLine($"{{field}} {openApiParameter.Name} : {openApiParameter.Schema.Type}<{openApiParameter.Schema.Items.Reference.Id}>{FieldRequired(openApiParameter.Required)}");
                    }
                    else if (openApiParameter.Schema.Items != null && openApiParameter.Schema.Type == "array")
                    {
                        stringBuilder.AppendLine($"{{field}} {openApiParameter.Name} : {openApiParameter.Schema.Type}<{openApiParameter.Schema.Items.Type}>{FieldRequired(openApiParameter.Required)}");
                    }
                    else
                    {
                        stringBuilder.AppendLine($"{{field}} {openApiParameter.Name} : {openApiParameter.Schema.Type}{FieldRequired(openApiParameter.Required)}");
                    }
                }

                stringBuilder.AppendLine($"}}");

                foreach (var openApiResponse in openApiOperation.Responses)
                {
                    stringBuilder.AppendLine($"class \"{operationType.ToString().ToUpper()} {openApiPathItemKey} {openApiResponse.Key}\" <<Response>> {{ }}");
                }
            }
        }

        foreach (var (openApiSchemaKey, openApiSchema) in openApiDocument.Components.Schemas)
        {
            stringBuilder.AppendLine($"class {SanitiseOpenApiSchemaKey(openApiSchemaKey)} {{");

            if (openApiSchema.Properties.Count > 0)
            {
                foreach (var (propertyOpenApiSchemaKey, propertyOpenApiSchema) in openApiSchema.Properties)
                {
                    if (!Constants.OpenApiBuiltInTypes.Contains(openApiSchema.Type) && propertyOpenApiSchema.Reference != null)
                    {
                        stringBuilder.AppendLine($"{{field}} {propertyOpenApiSchemaKey} : {propertyOpenApiSchema.Reference.Id}");
                    }
                    else
                    {
                        if (propertyOpenApiSchema.Items != null && propertyOpenApiSchema.Items.Reference != null && propertyOpenApiSchema.Type == "array")
                        {
                            stringBuilder.AppendLine($"{{field}} {propertyOpenApiSchemaKey} : {propertyOpenApiSchema.Type}<{propertyOpenApiSchema.Items.Reference.Id}>");
                        }
                        else if (propertyOpenApiSchema.Items != null && propertyOpenApiSchema.Type == "array")
                        {
                            stringBuilder.AppendLine($"{{field}} {propertyOpenApiSchemaKey} : {propertyOpenApiSchema.Type}<{propertyOpenApiSchema.Items.Type}>");
                        }
                        else
                        {
                            stringBuilder.AppendLine($"{{field}} {propertyOpenApiSchemaKey} : {propertyOpenApiSchema.Type}");
                        }
                    }
                }
            }
            else
            {
                stringBuilder.AppendLine($"{{field}} value : {openApiSchema.Type}");
            }

            stringBuilder.AppendLine($"}}");

            if (!string.IsNullOrEmpty(openApiSchema.Description))
            {
                stringBuilder.AppendLine($"note bottom of {SanitiseOpenApiSchemaKey(openApiSchemaKey)}");
                stringBuilder.AppendLine(SanitiseOpenApiOperationDescription(openApiSchema.Description));
                stringBuilder.AppendLine("end note");
            }
        }

        foreach (var (openApiPathItemKey, openApiPathItem) in openApiDocument.Paths)
        {
            foreach (var (operationType, openApiOperation) in openApiPathItem.Operations)
            {
                foreach (var (openApiResponseKey, openApiResponse) in openApiOperation.Responses)
                {
                    stringBuilder.AppendLine($"\"{operationType.ToString().ToUpper()} {openApiPathItemKey}\"" +
                                             $" ..> \"1\"" +
                                             $" \"{operationType.ToString().ToUpper()} {openApiPathItemKey} {openApiResponseKey}\"" +
                                             $" : {openApiResponseKey}");

                    foreach (var (_, openApiMediaType) in openApiResponse.Content)
                    {
                        if (openApiMediaType.Schema != null)
                        {
                            if (openApiMediaType.Schema.Type == "array")
                            {
                                stringBuilder.AppendLine($"\"{operationType.ToString().ToUpper()} {openApiPathItemKey} {openApiResponseKey}\"" +
                                                         $" ..> \"*\"" +
                                                         $" \"{openApiMediaType.Schema.Items.Reference.Id}\"");
                            }
                            else if (openApiMediaType.Schema.Reference != null)
                            {
                                stringBuilder.AppendLine($"\"{operationType.ToString().ToUpper()} {openApiPathItemKey} {openApiResponseKey}\"" +
                                                         $" ..> \"1\"" +
                                                         $" \"{openApiMediaType.Schema.Reference.Id}\"" +
                                                         $" : \"{openApiResponseKey}\"");
                            }

                            foreach (var anyOfOpenApiSchema in openApiMediaType.Schema.AnyOf)
                            {
                                stringBuilder.AppendLine($"\"{operationType.ToString().ToUpper()} {openApiPathItemKey} {openApiResponseKey}\"" +
                                                         $" ..> \"1\"" +
                                                         $" \"{anyOfOpenApiSchema.Reference.Id}\"" +
                                                         $" : \"[AnyOf]\"");
                            }

                            foreach (var allOfOpenApiSchema in openApiMediaType.Schema.AllOf)
                            {
                                stringBuilder.AppendLine($"\"{operationType.ToString().ToUpper()} {openApiPathItemKey} {openApiResponseKey}\"" +
                                                         $" --> \"1\"" +
                                                         $" \"{allOfOpenApiSchema.Reference.Id}\"" +
                                                         $" : \"[AllOf]\"");
                            }
                        }
                    }
                }
            }
        }

        foreach (var (openApiSchemaKey, openApiSchema) in openApiDocument.Components.Schemas)
        {
            if (openApiSchema.Type == "array")
            {
                stringBuilder.AppendLine($"\"{SanitiseOpenApiSchemaKey(openApiSchemaKey)}\"" +
                                         $" ..> \"*\"" +
                                         $" \"{openApiSchema.Items.Reference.Id}\"");
            }
            else
            {
                if (!Constants.OpenApiBuiltInTypes.Contains(openApiSchema.Type))
                {
                    foreach (var (propertyOpenApiSchemaKey, propertyOpenApiSchema) in openApiSchema.Properties)
                    {
                        if (propertyOpenApiSchema.Type == "array")
                        {
                            if (propertyOpenApiSchema.Items.Reference != null)
                            {
                                stringBuilder.AppendLine($"\"{SanitiseOpenApiSchemaKey(openApiSchemaKey)}\"" +
                                                         $" ..> \"1\"" +
                                                         $" \"{propertyOpenApiSchema.Items.Reference.Id}\"" +
                                                         $" : \"{propertyOpenApiSchemaKey}\"");
                            }
                        }
                        else
                        {
                            if (propertyOpenApiSchema.Type != null && !Constants.OpenApiBuiltInTypes.Contains(propertyOpenApiSchema.Type))
                            {
                                stringBuilder.AppendLine($"\"{SanitiseOpenApiSchemaKey(openApiSchemaKey)}\"" +
                                                         $" ..> \"1\"" +
                                                         $" \"{propertyOpenApiSchema.Reference.Id}\"" +
                                                         $" : \"{propertyOpenApiSchemaKey}\"");
                            }
                            else if(propertyOpenApiSchema.Reference != null)
                            {
                                stringBuilder.AppendLine($"\"{SanitiseOpenApiSchemaKey(openApiSchemaKey)}\"" +
                                                         $" ..> \"1\"" +
                                                         $" \"{propertyOpenApiSchema.Reference.Id}\"" +
                                                         $" : \"{propertyOpenApiSchemaKey}\"");
                            }
                        }
                    }
                }
            }
        }

        foreach (var (openApiSchemaKey, openApiSchema) in openApiDocument.Components.Schemas)
        {
            foreach (var allOf in openApiSchema.AllOf)
            {
                stringBuilder.AppendLine($"\"{SanitiseOpenApiSchemaKey(openApiSchemaKey)}\"" +
                                         $" --> \"1\"" +
                                         $" \"{allOf.Reference.Id}\"");
            }
            foreach (var anyOf in openApiSchema.AnyOf)
            {
                stringBuilder.AppendLine($"\"{SanitiseOpenApiSchemaKey(openApiSchemaKey)}\"" +
                                         $" --|>" +
                                         $" \"{anyOf.Reference.Id}\"");
            }
            foreach (var oneOf in openApiSchema.OneOf)
            {
                stringBuilder.AppendLine($"\"{SanitiseOpenApiSchemaKey(openApiSchemaKey)}\"" +
                                         $" --|>" +
                                         $" \"{oneOf.Reference.Id}\"");
            }
        }

        stringBuilder.Append(boilerplateEnd);

        return stringBuilder.ToString();
    }
}