using Futurum.OpenApiToPlantuml;

using Microsoft.OpenApi.Readers;

var standardOutWriter = Console.Out;

const string openapiCommand = "openapi";
const string openApiTypeCommand = "openapi-type";

try
{
    if (args.Length > 0)
    {
        var standardInputStream = Console.OpenStandardInput();

        var openApiReaderSettings = new OpenApiReaderSettings
        {
            ReferenceResolution = ReferenceResolutionSetting.DoNotResolveReferences
        };
        var openApiDocument = new OpenApiStreamReader(openApiReaderSettings).Read(standardInputStream, out var diagnostic);

        switch (args[0])
        {
            case openapiCommand:
            {
                var diagram = OpenApiDiagram.Create(openApiDocument);

                standardOutWriter.Write(diagram);

                break;
            }
            case openApiTypeCommand:
            {
                var diagram = OpenApiTypeDiagram.Create(openApiDocument);

                standardOutWriter.Write(diagram);

                break;
            }
        }
    }
    else
    {
        standardOutWriter.Write($"Please specify type of diagram - '{openapiCommand}', '{openApiTypeCommand}'");
    }
}
catch (Exception exception)
{
    standardOutWriter.Write(exception.Message);
}