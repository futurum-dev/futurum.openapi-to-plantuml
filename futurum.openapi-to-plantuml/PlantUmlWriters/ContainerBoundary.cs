using System.Text;

namespace Futurum.OpenApiToPlantuml;

public record ContainerBoundary(string Id, string Title) : IPlantUmlWriter
{
    private readonly List<Component> _components = new();

    public ContainerBoundary AddComponent(Component component)
    {
        _components.Add(component);

        return this;
    }
    
    public override string ToString()
    {
        var stringBuilder = new StringBuilder();

        stringBuilder.AppendLine($"Container_Boundary({Id}, \"{Title}\") {{");

        foreach (var component in _components)
        {
            stringBuilder.Write(component);
        }
        
        stringBuilder.AppendLine($"}}");
        
        return stringBuilder.ToString();
    }
}