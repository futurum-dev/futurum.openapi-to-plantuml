using System.Text;

using Microsoft.OpenApi.Models;

namespace Futurum.OpenApiToPlantuml;

public class OpenApiTypeDiagram
{
    private const string BoilerplateStart = @"@startuml OpenApi Type diagram
'!theme blueprint

hide <<Path>> circle
hide <<Response>> circle
hide <<Parameter>> circle
hide empty methods
hide empty fields
set namespaceSeparator none

";

    private const string BoilerplateEnd = @"

@enduml";
        
    public static string Create(OpenApiDocument openApiDocument)
    {
        var stringBuilder = new StringBuilder();
        stringBuilder.Append(BoilerplateStart);

        stringBuilder.Write(new DiagramTitle(openApiDocument));
        stringBuilder.Write(new DiagramFooter("OpenApi Type"));

        foreach (var (openApiPathItemKey, openApiPathItem) in openApiDocument.Paths)
        {
            foreach (var (operationType, openApiOperation) in openApiPathItem.Operations)
            {
                var classPath = new ClassPath($"{operationType.ToString().ToUpper()} {openApiPathItemKey}");

                foreach (var openApiParameter in openApiOperation.Parameters)
                {
                    if (openApiParameter.Schema.Items != null && openApiParameter.Schema.Items.Reference != null && openApiParameter.Schema.Type == OpenApiConstants.ArrayType)
                    {
                        classPath.AddField(new ClassField(openApiParameter.Name, $"{openApiParameter.Schema.Type}<{openApiParameter.Schema.Items.Reference.Id}>", openApiParameter.Required));
                    }
                    else if (openApiParameter.Schema.Items != null && openApiParameter.Schema.Type == OpenApiConstants.ArrayType)
                    {
                        classPath.AddField(new ClassField(openApiParameter.Name, $"{openApiParameter.Schema.Type}<{openApiParameter.Schema.Items.Type}>", openApiParameter.Required));
                    }
                    else
                    {
                        classPath.AddField(new ClassField(openApiParameter.Name, openApiParameter.Schema.Type, openApiParameter.Required));
                    }
                }

                if (openApiOperation.RequestBody != null && openApiOperation.RequestBody.Content.Any())
                {
                    var (_, openApiMediaType) = openApiOperation.RequestBody.Content.First();

                    foreach (var (propertyOpenApiSchemaKey, propertyOpenApiSchema) in openApiMediaType.Schema.Properties)
                    {
                        if (propertyOpenApiSchema.Items != null && propertyOpenApiSchema.Items.Reference != null && propertyOpenApiSchema.Type == OpenApiConstants.ArrayType)
                        {
                            classPath.AddField(new ClassField(propertyOpenApiSchemaKey, $"{propertyOpenApiSchema.Type}<{propertyOpenApiSchema.Items.Reference.Id}>"));
                        }
                        else if (propertyOpenApiSchema.Items != null && propertyOpenApiSchema.Type == OpenApiConstants.ArrayType)
                        {
                            classPath.AddField(new ClassField(propertyOpenApiSchemaKey, $"{propertyOpenApiSchema.Type}<{propertyOpenApiSchema.Items.Type}>"));
                        }
                        else
                        {
                            classPath.AddField(new ClassField(propertyOpenApiSchemaKey, propertyOpenApiSchema.Type));
                        }
                    }
                }

                stringBuilder.Write(classPath);

                foreach (var openApiResponse in openApiOperation.Responses)
                {
                    stringBuilder.Write(new ClassResponse($"{operationType.ToString().ToUpper()} {openApiPathItemKey} {openApiResponse.Key}"));
                }
            }
        }

        foreach (var (openApiSchemaKey, openApiSchema) in openApiDocument.Components.Schemas)
        {
            var classType = new ClassType(SanitiseOpenApiSchemaKey(openApiSchemaKey));

            if (openApiSchema.Properties.Count > 0)
            {
                foreach (var (propertyOpenApiSchemaKey, propertyOpenApiSchema) in openApiSchema.Properties)
                {
                    if (!OpenApiConstants.BuiltInTypes.Contains(openApiSchema.Type) && propertyOpenApiSchema.Reference != null)
                    {
                        classType.AddField(new ClassField(propertyOpenApiSchemaKey, propertyOpenApiSchema.Reference.Id));
                    }
                    else
                    {
                        if (propertyOpenApiSchema.Items != null && propertyOpenApiSchema.Items.Reference != null && propertyOpenApiSchema.Type == OpenApiConstants.ArrayType)
                        {
                            classType.AddField(new ClassField(propertyOpenApiSchemaKey, $"{propertyOpenApiSchema.Type}<{propertyOpenApiSchema.Items.Reference.Id}>"));
                        }
                        else if (propertyOpenApiSchema.Items != null && propertyOpenApiSchema.Type == OpenApiConstants.ArrayType)
                        {
                            classType.AddField(new ClassField(propertyOpenApiSchemaKey, $"{propertyOpenApiSchema.Type}<{propertyOpenApiSchema.Items.Type}>"));
                        }
                        else
                        {
                            classType.AddField(new ClassField(propertyOpenApiSchemaKey, propertyOpenApiSchema.Type));
                        }
                    }
                }
            }
            else
            {
                classType.AddField(new ClassField("value", openApiSchema.Type));
            }

            stringBuilder.Write(classType);

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

                    foreach (var (openApiMediaTypeKey, openApiMediaType) in openApiResponse.Content)
                    {
                        if (openApiMediaType.Schema != null)
                        {
                            var label = openApiResponse.Content.Count > 1
                                ? openApiMediaTypeKey
                                : string.Empty;

                            if (openApiMediaType.Schema.Type == OpenApiConstants.ArrayType)
                            {
                                stringBuilder.Write(new ClassRelationship($"{operationType.ToString().ToUpper()} {openApiPathItemKey} {openApiResponseKey}",
                                                                          ClassRelationshipType.Reference, ClassRelationshipCardinality.Many,
                                                                          openApiMediaType.Schema.Items.Reference.Id, label));
                            }
                            else if (openApiMediaType.Schema.Reference != null)
                            {
                                stringBuilder.Write(new ClassRelationship($"{operationType.ToString().ToUpper()} {openApiPathItemKey} {openApiResponseKey}",
                                                                          ClassRelationshipType.Reference, ClassRelationshipCardinality.One,
                                                                          openApiMediaType.Schema.Reference.Id, label));
                            }

                            foreach (var anyOfOpenApiSchema in openApiMediaType.Schema.AnyOf)
                            {
                                stringBuilder.Write(new ClassRelationship($"{operationType.ToString().ToUpper()} {openApiPathItemKey} {openApiResponseKey}",
                                                                          ClassRelationshipType.AnyOf, ClassRelationshipCardinality.One, anyOfOpenApiSchema.Reference.Id, label));
                            }

                            foreach (var allOfOpenApiSchema in openApiMediaType.Schema.AllOf)
                            {
                                stringBuilder.Write(new ClassRelationship($"{operationType.ToString().ToUpper()} {openApiPathItemKey} {openApiResponseKey}",
                                                                          ClassRelationshipType.AllOf, ClassRelationshipCardinality.One,
                                                                          allOfOpenApiSchema.Reference.Id, label));
                            }

                            foreach (var oneOfOpenApiSchema in openApiMediaType.Schema.OneOf)
                            {
                                stringBuilder.Write(new ClassRelationship($"{operationType.ToString().ToUpper()} {openApiPathItemKey} {openApiResponseKey}",
                                                                          ClassRelationshipType.OneOf, ClassRelationshipCardinality.One,
                                                                          oneOfOpenApiSchema.Reference.Id, label));
                            }
                        }
                    }
                }
            }
        }

        foreach (var (openApiSchemaKey, openApiSchema) in openApiDocument.Components.Schemas)
        {
            if (openApiSchema.Type == OpenApiConstants.ArrayType)
            {
                stringBuilder.Write(new ClassRelationship(SanitiseOpenApiSchemaKey(openApiSchemaKey), ClassRelationshipType.Reference, ClassRelationshipCardinality.Many,
                                                          openApiSchema.Items.Reference.Id));
            }
            else
            {
                if (!OpenApiConstants.BuiltInTypes.Contains(openApiSchema.Type))
                {
                    foreach (var (propertyOpenApiSchemaKey, propertyOpenApiSchema) in openApiSchema.Properties)
                    {
                        if (propertyOpenApiSchema.Type == OpenApiConstants.ArrayType)
                        {
                            if (propertyOpenApiSchema.Items.Reference != null)
                            {
                                stringBuilder.Write(new ClassRelationship(SanitiseOpenApiSchemaKey(openApiSchemaKey), ClassRelationshipType.Reference, ClassRelationshipCardinality.One,
                                                                          propertyOpenApiSchema.Items.Reference.Id, propertyOpenApiSchemaKey));
                            }
                        }
                        else
                        {
                            if (propertyOpenApiSchema.Type != null && !OpenApiConstants.BuiltInTypes.Contains(propertyOpenApiSchema.Type))
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
                stringBuilder.Write(new ClassRelationship(SanitiseOpenApiSchemaKey(openApiSchemaKey), ClassRelationshipType.OneOf, ClassRelationshipCardinality.None, oneOf.Reference.Id));
            }
        }

        foreach (var (openApiPathItemKey, openApiPathItem) in openApiDocument.Paths)
        {
            foreach (var (operationType, openApiOperation) in openApiPathItem.Operations)
            {
                if (openApiOperation.RequestBody != null)
                {
                    foreach (var (openApiMediaTypeKey, openApiMediaType) in openApiOperation.RequestBody.Content)
                    {
                        var label = openApiOperation.RequestBody.Content.Count > 1
                            ? openApiMediaTypeKey
                            : string.Empty;

                        if (openApiMediaType.Schema.Type == OpenApiConstants.ArrayType)
                        {
                            stringBuilder.Write(new ClassRelationship($"{operationType.ToString().ToUpper()} {openApiPathItemKey}", ClassRelationshipType.Reference, ClassRelationshipCardinality.One,
                                                                      openApiMediaType.Schema.Items.Reference.Id, label));
                        }
                        else
                        {
                            if (openApiMediaType.Schema.Reference != null)
                            {
                                stringBuilder.Write(new ClassRelationship($"{operationType.ToString().ToUpper()} {openApiPathItemKey}", ClassRelationshipType.Reference,
                                                                          ClassRelationshipCardinality.One,
                                                                          openApiMediaType.Schema.Reference.Id, label));
                            }
                            else if(openApiMediaType.Schema.Type != null)
                            {
                                stringBuilder.Write(new ClassRelationship($"{operationType.ToString().ToUpper()} {openApiPathItemKey}", ClassRelationshipType.Reference,
                                                                          ClassRelationshipCardinality.One,
                                                                          openApiMediaType.Schema.Type, label));
                            }
                        }
                    }
                }
            }
        }

        stringBuilder.Append(BoilerplateEnd);

        return stringBuilder.ToString();
    }

    private static string SanitiseOpenApiOperationDescription(string description) =>
        description.Replace(Environment.NewLine, " ");

    private static string SanitiseOpenApiSchemaKey(string openApiSchemaKey) =>
        openApiSchemaKey.Replace("/", string.Empty)
                        .Replace("{", string.Empty)
                        .Replace("}", string.Empty);
}