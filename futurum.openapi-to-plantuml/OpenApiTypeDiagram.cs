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

        stringBuilder.Write(new Title(openApiDocument));
        stringBuilder.Write(new Footer("OpenApi Type"));

        foreach (var (openApiPathItemKey, openApiPathItem) in openApiDocument.Paths)
        {
            foreach (var (operationType, openApiOperation) in openApiPathItem.Operations)
            {
                stringBuilder.AppendLine($"class \"{operationType.ToString().ToUpper()} {openApiPathItemKey}\" <<Path>> {{");

                foreach (var openApiParameter in openApiOperation.Parameters)
                {
                    if (openApiParameter.Schema.Items != null && openApiParameter.Schema.Items.Reference != null && openApiParameter.Schema.Type == "array")
                    {
                        stringBuilder.Write(new ClassField(openApiParameter.Name, $"{openApiParameter.Schema.Type}<{openApiParameter.Schema.Items.Reference.Id}>", openApiParameter.Required));
                    }
                    else if (openApiParameter.Schema.Items != null && openApiParameter.Schema.Type == "array")
                    {
                        stringBuilder.Write(new ClassField(openApiParameter.Name, $"{openApiParameter.Schema.Type}<{openApiParameter.Schema.Items.Type}>", openApiParameter.Required));
                    }
                    else
                    {
                        stringBuilder.Write(new ClassField(openApiParameter.Name, openApiParameter.Schema.Type, openApiParameter.Required));
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
                        stringBuilder.Write(new ClassField(propertyOpenApiSchemaKey, propertyOpenApiSchema.Reference.Id));
                    }
                    else
                    {
                        if (propertyOpenApiSchema.Items != null && propertyOpenApiSchema.Items.Reference != null && propertyOpenApiSchema.Type == "array")
                        {
                            stringBuilder.Write(new ClassField(propertyOpenApiSchemaKey, $"{propertyOpenApiSchema.Type}<{propertyOpenApiSchema.Items.Reference.Id}>"));
                        }
                        else if (propertyOpenApiSchema.Items != null && propertyOpenApiSchema.Type == "array")
                        {
                            stringBuilder.Write(new ClassField(propertyOpenApiSchemaKey, $"{propertyOpenApiSchema.Type}<{propertyOpenApiSchema.Items.Type}>"));
                        }
                        else
                        {
                            stringBuilder.Write(new ClassField(propertyOpenApiSchemaKey, propertyOpenApiSchema.Type));
                        }
                    }
                }
            }
            else
            {
                stringBuilder.Write(new ClassField("value", openApiSchema.Type));
            }

            stringBuilder.AppendLine($"}}");

            if (!string.IsNullOrEmpty(openApiSchema.Description))
            {
                stringBuilder.Write(new Note(SanitiseOpenApiSchemaKey(openApiSchemaKey), SanitiseOpenApiOperationDescription(openApiSchema.Description)));
            }
        }

        foreach (var (openApiPathItemKey, openApiPathItem) in openApiDocument.Paths)
        {
            foreach (var (operationType, openApiOperation) in openApiPathItem.Operations)
            {
                foreach (var (openApiResponseKey, openApiResponse) in openApiOperation.Responses)
                {
                    stringBuilder.Write(new ClassRelationship($"{operationType.ToString().ToUpper()} {openApiPathItemKey}",
                                                              ClassRelationshipType.Reference, ClassRelationshipCardinality.One,
                                                              $"{operationType.ToString().ToUpper()} {openApiPathItemKey} {openApiResponseKey}",
                                                              openApiResponseKey));

                    foreach (var (_, openApiMediaType) in openApiResponse.Content)
                    {
                        if (openApiMediaType.Schema != null)
                        {
                            if (openApiMediaType.Schema.Type == "array")
                            {
                                stringBuilder.Write(new ClassRelationship($"{operationType.ToString().ToUpper()} {openApiPathItemKey} {openApiResponseKey}",
                                                                          ClassRelationshipType.Reference, ClassRelationshipCardinality.Many,
                                                                          openApiMediaType.Schema.Items.Reference.Id));
                            }
                            else if (openApiMediaType.Schema.Reference != null)
                            {
                                stringBuilder.Write(new ClassRelationship($"{operationType.ToString().ToUpper()} {openApiPathItemKey} {openApiResponseKey}",
                                                                          ClassRelationshipType.Reference, ClassRelationshipCardinality.One,
                                                                          openApiMediaType.Schema.Reference.Id, openApiResponseKey));
                            }

                            foreach (var anyOfOpenApiSchema in openApiMediaType.Schema.AnyOf)
                            {
                                stringBuilder.Write(new ClassRelationship($"{operationType.ToString().ToUpper()} {openApiPathItemKey} {openApiResponseKey}",
                                                                          ClassRelationshipType.AllOf, ClassRelationshipCardinality.One, anyOfOpenApiSchema.Reference.Id, "[AnyOf]"));
                            }

                            foreach (var allOfOpenApiSchema in openApiMediaType.Schema.AllOf)
                            {
                                stringBuilder.Write(new ClassRelationship($"{operationType.ToString().ToUpper()} {openApiPathItemKey} {openApiResponseKey}",
                                                                          ClassRelationshipType.AllOf, ClassRelationshipCardinality.One,
                                                                          allOfOpenApiSchema.Reference.Id, "[AllOf]"));
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
                stringBuilder.Write(new ClassRelationship(SanitiseOpenApiSchemaKey(openApiSchemaKey), ClassRelationshipType.Reference, ClassRelationshipCardinality.Many,
                                                          openApiSchema.Items.Reference.Id));
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
                                stringBuilder.Write(new ClassRelationship(SanitiseOpenApiSchemaKey(openApiSchemaKey), ClassRelationshipType.Reference, ClassRelationshipCardinality.One,
                                                                          propertyOpenApiSchema.Items.Reference.Id, propertyOpenApiSchemaKey));
                            }
                        }
                        else
                        {
                            if (propertyOpenApiSchema.Type != null && !Constants.OpenApiBuiltInTypes.Contains(propertyOpenApiSchema.Type))
                            {
                                stringBuilder.Write(new ClassRelationship(SanitiseOpenApiSchemaKey(openApiSchemaKey), ClassRelationshipType.Reference, ClassRelationshipCardinality.One,
                                                                          propertyOpenApiSchema.Reference.Id, propertyOpenApiSchemaKey));
                            }
                            else if (propertyOpenApiSchema.Reference != null)
                            {
                                stringBuilder.Write(new ClassRelationship(SanitiseOpenApiSchemaKey(openApiSchemaKey), ClassRelationshipType.Reference, ClassRelationshipCardinality.One,
                                                                          propertyOpenApiSchema.Reference.Id, propertyOpenApiSchemaKey));
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
                stringBuilder.Write(new ClassRelationship(SanitiseOpenApiSchemaKey(openApiSchemaKey), ClassRelationshipType.AllOf, ClassRelationshipCardinality.One, allOf.Reference.Id));
            }

            foreach (var anyOf in openApiSchema.AnyOf)
            {
                stringBuilder.Write(new ClassRelationship(SanitiseOpenApiSchemaKey(openApiSchemaKey), ClassRelationshipType.AnyOf, ClassRelationshipCardinality.None, anyOf.Reference.Id));
            }

            foreach (var oneOf in openApiSchema.OneOf)
            {
                stringBuilder.Write(new ClassRelationship(SanitiseOpenApiSchemaKey(openApiSchemaKey), ClassRelationshipType.AnyOf, ClassRelationshipCardinality.None, oneOf.Reference.Id));
            }
        }

        stringBuilder.Append(boilerplateEnd);

        return stringBuilder.ToString();
    }
}