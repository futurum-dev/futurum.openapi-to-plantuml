using System.CommandLine;

using Futurum.OpenApiToPlantuml;
using Futurum.OpenApiToPlantumlDirectory;

using Microsoft.OpenApi.Models;
using Microsoft.OpenApi.Readers;

try
{
    var rootCommand = new RootCommand("Futurum.OpenApiToPlantuml Directory");

    var themeOption = new Option<string>(name: "--theme", description: "PlantUml theme");
    var showNotesOption = new Option<bool>(name: "--shownotes", description: "Show notes in PlantUml");

    rootCommand.AddOption(themeOption);
    rootCommand.AddOption(showNotesOption);
    rootCommand.SetHandler(async (theme, showNotes) =>
    {
        var configuration = DiagramConfiguration.Default;
        configuration.Theme = theme;
        configuration.ShowNotes = showNotes;

        await ProcessDirectoryAsync(new DirectoryInfo(FileConstants.Directory), configuration);
    }, themeOption, showNotesOption);

    return await rootCommand.InvokeAsync(args);
}
catch (Exception exception)
{
    Console.WriteLine(exception.Message);

    return 1;
}

async Task ProcessFileAsync(FileInfo fileInfo, DiagramConfiguration configuration)
{
    Console.WriteLine($"Processing file : '{fileInfo.FullName}'");

    var fileNameWithoutExtension = Path.GetFileNameWithoutExtension(fileInfo.FullName);
    var filePathWithoutFileExtension = Path.Combine(fileInfo.DirectoryName, fileNameWithoutExtension);

    var openApiFilePath = fileInfo.FullName;

    var stream = File.OpenRead(openApiFilePath);
    var openApiReaderSettings = new OpenApiReaderSettings
    {
        ReferenceResolution = ReferenceResolutionSetting.ResolveLocalReferences
    };
    var openApiDocument = new OpenApiStreamReader(openApiReaderSettings).Read(stream, out var diagnostic);

    await CreateOpenApiDiagram(openApiDocument, filePathWithoutFileExtension, configuration);

    await CreateOpenApiTypeDiagram(openApiDocument, filePathWithoutFileExtension, configuration);

    Console.WriteLine($"Processed file : '{fileInfo.FullName}'");
}

async Task ProcessDirectoryAsync(DirectoryInfo directoryInfo, DiagramConfiguration configuration)
{
    Console.WriteLine($"Processing directory : '{directoryInfo.FullName}'");

    var fileInfos = directoryInfo.GetFiles()
                                 .Where(fileInfo => FileConstants.FileExtensions.Contains(fileInfo.Extension.ToLower()));

    foreach (var fileInfo in fileInfos)
    {
        await ProcessFileAsync(fileInfo, configuration);
    }

    Console.WriteLine($"Processed directory : '{directoryInfo.FullName}'");
}

async Task CreateOpenApiDiagram(OpenApiDocument openApiDocument, string filePathWithoutFileExtension, DiagramConfiguration configuration)
{
    var diagram = OpenApiDiagram.Create(openApiDocument, configuration);

    var plantUmlFilePath = $"{filePathWithoutFileExtension}-openapi.puml";

    if (File.Exists(plantUmlFilePath))
    {
        File.Delete(plantUmlFilePath);
    }

    await File.WriteAllTextAsync(plantUmlFilePath, diagram);
}

async Task CreateOpenApiTypeDiagram(OpenApiDocument openApiDocument, string filePathWithoutFileExtension, DiagramConfiguration configuration)
{
    var diagram = OpenApiTypeDiagram.Create(openApiDocument, configuration);

    var plantUmlFilePath = $"{filePathWithoutFileExtension}-openapi-type.puml";

    if (File.Exists(plantUmlFilePath))
    {
        File.Delete(plantUmlFilePath);
    }

    await File.WriteAllTextAsync(plantUmlFilePath, diagram);
}