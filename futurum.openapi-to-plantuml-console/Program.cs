using System.CommandLine;

using Futurum.OpenApiToPlantuml;
using Futurum.OpenApiToPlantumlConsole;

using Microsoft.OpenApi.Models;
using Microsoft.OpenApi.Readers;

try
{
    var fileOption = new Option<FileInfo?>(name: "--file", description: "The OpenApi file to use.");

    var directoryOption = new Option<DirectoryInfo?>(name: "--directory", description: "The directory containing the OpenApi files to use.");

    var rootCommand = new RootCommand("Futurum.OpenApiToPlantuml");
    rootCommand.AddOption(fileOption);
    rootCommand.AddOption(directoryOption);

    rootCommand.SetHandler(async fileInfo =>
                           {
                               if (fileInfo == null)
                               {
                                   return;
                               }

                               await ProcessFileAsync(fileInfo);
                           },
                           fileOption);

    rootCommand.SetHandler(async directoryInfo =>
                           {
                               if (directoryInfo == null)
                               {
                                   return;
                               }

                               await ProcessDirectoryAsync(directoryInfo);
                           },
                           directoryOption);

    return await rootCommand.InvokeAsync(args);

    async Task ProcessFileAsync(FileInfo fileInfo)
    {
        Console.WriteLine($"Processing file : '{fileInfo.FullName}'");

        var fileNameWithoutExtension = Path.GetFileNameWithoutExtension(fileInfo.FullName);
        var filePathWithoutFileExtension = Path.Combine(fileInfo.DirectoryName, fileNameWithoutExtension);

        var openApiFilePath = fileInfo.FullName;

        var stream = File.OpenRead(openApiFilePath);
        var openApiReaderSettings = new OpenApiReaderSettings
        {
            ReferenceResolution = ReferenceResolutionSetting.DoNotResolveReferences
        };
        var openApiDocument = new OpenApiStreamReader(openApiReaderSettings).Read(stream, out var diagnostic);

        await CreateOpenApiDiagram(openApiDocument, filePathWithoutFileExtension);

        await CreateOpenApiTypeDiagram(openApiDocument, filePathWithoutFileExtension);

        Console.WriteLine($"Processed file : '{fileInfo.FullName}'");
    }

    async Task ProcessDirectoryAsync(DirectoryInfo directoryInfo)
    {
        Console.WriteLine($"Processing directory : '{directoryInfo.FullName}'");

        var fileInfos = directoryInfo.GetFiles()
                                     .Where(fileInfo => FileConstants.FileExtensions.Contains(fileInfo.Extension));

        foreach (var fileInfo in fileInfos)
        {
            await ProcessFileAsync(fileInfo);
        }

        Console.WriteLine($"Processed directory : '{directoryInfo.FullName}'");
    }

    async Task CreateOpenApiDiagram(OpenApiDocument openApiDocument, string filePathWithoutFileExtension)
    {
        var diagram = OpenApiDiagram.Create(openApiDocument);

        var plantUmlFilePath = $"{filePathWithoutFileExtension}-openapi.puml";

        if (File.Exists(plantUmlFilePath))
        {
            File.Delete(plantUmlFilePath);
        }

        await File.WriteAllTextAsync(plantUmlFilePath, diagram);
    }

    async Task CreateOpenApiTypeDiagram(OpenApiDocument openApiDocument, string filePathWithoutFileExtension)
    {
        var diagram = OpenApiTypeDiagram.Create(openApiDocument);

        var plantUmlFilePath = $"{filePathWithoutFileExtension}-openapi-type.puml";

        if (File.Exists(plantUmlFilePath))
        {
            File.Delete(plantUmlFilePath);
        }

        await File.WriteAllTextAsync(plantUmlFilePath, diagram);
    }
}
catch (Exception exception)
{
    Console.WriteLine(exception.Message);

    return 1;
}