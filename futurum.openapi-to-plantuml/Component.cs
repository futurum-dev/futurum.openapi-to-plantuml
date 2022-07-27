using System.Text;

namespace Futurum.OpenApiToPlantuml;

public record Component(string Id, string Title, string Protocol, string Description) : IPlantUmlWriter
{
    public override string ToString()
    {
        var stringBuilder = new StringBuilder();
        
        stringBuilder.Append($"Component({Id}, " +
                                 $"\"{Title}\", " +
                                 $"\"{Protocol}\", " +
                                 $"\"{Description}\")");
        
        return stringBuilder.ToString();
    }
}