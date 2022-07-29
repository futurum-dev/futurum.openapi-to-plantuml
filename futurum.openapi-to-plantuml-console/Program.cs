using System.CommandLine;

using Futurum.OpenApiToPlantuml;
using Futurum.OpenApiToPlantumlConsole;

using Microsoft.OpenApi.Models;
using Microsoft.OpenApi.Readers;

try
{
    var rootCommand = new RootCommand("Futurum.OpenApiToPlantuml console");

    var themeOption = new Option<string>(name: "--theme", description: "PlantUml theme");
    var showNotesOption = new Option<bool>(name: "--shownotes", description: "Show notes in PlantUml");

    var fileCommand = new Command("file", "Process a OpenApi file");
    var fileOption = new Option<FileInfo?>(name: "--path", description: "File path");
    fileCommand.AddOption(fileOption);
    fileCommand.AddOption(themeOption);
    fileCommand.AddOption(showNotesOption);
    fileCommand.SetHandler(async (fileInfo, theme, showNotes) =>
    {
        if (fileInfo == null)
        {
            return;
        }

        var configuration = DiagramConfiguration.Default;
        configuration.Theme = theme;
        configuration.ShowNotes = showNotes;

        await ProcessFileAsync(fileInfo, configuration);
    }, fileOption, themeOption, showNotesOption);
    rootCommand.AddCommand(fileCommand);

    var directoryCommand = new Command("directory", "Process a directory containing OpenApi files");
    var directoryOption = new Option<DirectoryInfo?>(name: "--path", description: "Directory path");
    directoryCommand.AddOption(directoryOption);
    directoryCommand.AddOption(themeOption);
    directoryCommand.AddOption(showNotesOption);
    directoryCommand.SetHandler(async (directoryInfo, theme, showNotes) =>
                                {
                                    if (directoryInfo == null)
                                    {
                                        return;
                                    }

                                    var configuration = DiagramConfiguration.Default;
                                    configuration.Theme = theme;
                                    configuration.ShowNotes = showNotes;

                                    await ProcessDirectoryAsync(directoryInfo, configuration);
                                },
                                directoryOption, themeOption, showNotesOption);
    rootCommand.AddCommand(directoryCommand);

    return await rootCommand.InvokeAsync(args);

    async Task ProcessFileAsync(FileInfo fileInfo, DiagramConfiguration configuration)
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

        await CreateOpenApiDiagram(openApiDocument, filePathWithoutFileExtension, configuration);

        await CreateOpenApiTypeDiagram(openApiDocument, filePathWithoutFileExtension, configuration);

        Console.WriteLine($"Processed file : '{fileInfo.FullName}'");
    }

    async Task ProcessDirectoryAsync(DirectoryInfo directoryInfo, DiagramConfiguration configuration)
    {
        Console.WriteLine($"Processing directory : '{directoryInfo.FullName}'");

        var fileInfos = directoryInfo.GetFiles()
                                     .Where(fileInfo => FileConstants.FileExtensions.Contains(fileInfo.Extension));

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
}
catch (Exception exception)
{
    Console.WriteLine(exception.Message);

    return 1;
}