using System.Text;

namespace Futurum.OpenApiToPlantuml;

public static class PlantUmlWriterExtensions
{
    public static StringBuilder Write(this StringBuilder stringBuilder, IPlantUmlWriter plantUmlWriter)
    {
        stringBuilder.AppendLine(plantUmlWriter.ToString());

        return stringBuilder;
    }
}