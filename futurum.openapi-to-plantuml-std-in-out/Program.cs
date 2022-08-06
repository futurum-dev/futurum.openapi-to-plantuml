using System.CommandLine;

using Futurum.OpenApiToPlantuml;

using Microsoft.OpenApi.Models;
using Microsoft.OpenApi.Readers;

var standardOutWriter = Console.Out;

try
{
    var rootCommand = new RootCommand("Futurum.OpenApiToPlantuml Standard In Standard Out");

    var themeOption = new Option<string>(name: "--theme", description: "PlantUml theme");
    var showNotesOption = new Option<bool>(name: "--shownotes", description: "Show notes in PlantUml");

    rootCommand.AddCommand(OpenApiDiagramCommand(themeOption, showNotesOption));

    rootCommand.AddCommand(OpenApiTypeDiagramCommand(themeOption, showNotesOption));

    return await rootCommand.InvokeAsync(args);
}
catch (Exception exception)
{
    standardOutWriter.Write(exception.Message);

    return 1;
}

Command OpenApiDiagramCommand(Option<string> themeOption, Option<bool> showNotesOption)
{
    var command = new Command("openapi", "Create OpenApi diagram");
    command.AddOption(themeOption);
    command.AddOption(showNotesOption);
    command.SetHandler((theme, showNotes) =>
    {
        var configuration = DiagramConfiguration.Default;
        configuration.Theme = theme;
        configuration.ShowNotes = showNotes;

        var openApiDocument = CreateOpenApiDocument();

        var diagram = OpenApiDiagram.Create(openApiDocument, configuration);

        standardOutWriter.Write(diagram);
    }, themeOption, showNotesOption);
    return command;
}

Command OpenApiTypeDiagramCommand(Option<string> themeOption, Option<bool> showNotesOption)
{
    var command = new Command("openapi-type", "Create OpenApi Type diagram");
    command.AddOption(themeOption);
    command.AddOption(showNotesOption);
    command.SetHandler((theme, showNotes) =>
    {
        var configuration = DiagramConfiguration.Default;
        configuration.Theme = theme;
        configuration.ShowNotes = showNotes;

        var openApiDocument = CreateOpenApiDocument();

        var diagram = OpenApiTypeDiagram.Create(openApiDocument, configuration);

        standardOutWriter.Write(diagram);
    }, themeOption, showNotesOption);
    return command;
}

OpenApiDocument CreateOpenApiDocument()
{
    var standardInputStream = Console.OpenStandardInput();

    var openApiReaderSettings = new OpenApiReaderSettings
    {
        ReferenceResolution = ReferenceResolutionSetting.ResolveLocalReferences
    };

    return new OpenApiStreamReader(openApiReaderSettings).Read(standardInputStream, out var diagnostic);
}