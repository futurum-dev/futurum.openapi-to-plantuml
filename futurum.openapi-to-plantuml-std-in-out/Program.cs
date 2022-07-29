using System.CommandLine;

using Futurum.OpenApiToPlantuml;

using Microsoft.OpenApi.Readers;

var standardOutWriter = Console.Out;

try
{
    var rootCommand = new RootCommand("Futurum.OpenApiToPlantuml Standard In Standard Out");

    var themeOption = new Option<string>(name: "--theme", description: "PlantUml theme");
    var showNotesOption = new Option<bool>(name: "--shownotes", description: "Show notes in PlantUml");

    var openApiDiagramCommand = new Command("openapi", "Create OpenApi diagram");
    openApiDiagramCommand.AddOption(themeOption);
    openApiDiagramCommand.AddOption(showNotesOption);
    openApiDiagramCommand.SetHandler((theme, showNotes) =>
    {
        var configuration = DiagramConfiguration.Default;
        configuration.Theme = theme;
        configuration.ShowNotes = showNotes;

        var standardInputStream = Console.OpenStandardInput();

        var openApiReaderSettings = new OpenApiReaderSettings
        {
            ReferenceResolution = ReferenceResolutionSetting.DoNotResolveReferences
        };
        var openApiDocument = new OpenApiStreamReader(openApiReaderSettings).Read(standardInputStream, out var diagnostic);

        var diagram = OpenApiDiagram.Create(openApiDocument, configuration);

        standardOutWriter.Write(diagram);
    }, themeOption, showNotesOption);
    rootCommand.AddCommand(openApiDiagramCommand);

    var openApiTypeDiagramCommand = new Command("openapi-type", "Create OpenApi Type diagram");
    openApiTypeDiagramCommand.AddOption(themeOption);
    openApiTypeDiagramCommand.AddOption(showNotesOption);
    openApiTypeDiagramCommand.SetHandler((theme, showNotes) =>
    {
        var configuration = DiagramConfiguration.Default;
        configuration.Theme = theme;
        configuration.ShowNotes = showNotes;

        var standardInputStream = Console.OpenStandardInput();

        var openApiReaderSettings = new OpenApiReaderSettings
        {
            ReferenceResolution = ReferenceResolutionSetting.DoNotResolveReferences
        };
        var openApiDocument = new OpenApiStreamReader(openApiReaderSettings).Read(standardInputStream, out var diagnostic);

        var diagram = OpenApiTypeDiagram.Create(openApiDocument, configuration);

        standardOutWriter.Write(diagram);
    }, themeOption, showNotesOption);
    rootCommand.AddCommand(openApiTypeDiagramCommand);

    return await rootCommand.InvokeAsync(args);
}
catch (Exception exception)
{
    standardOutWriter.Write(exception.Message);

    return 1;
}