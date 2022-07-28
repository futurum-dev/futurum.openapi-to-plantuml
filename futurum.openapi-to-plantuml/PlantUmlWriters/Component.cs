using System.Text;

namespace Futurum.OpenApiToPlantuml;

public record Component(string Id, string Title, string Protocol, string? Description = null) : IPlantUmlWriter
{
    private Note? _note = null;

    public Component AddNote(Note note)
    {
        _note = note;

        return this;
    }

    public override string ToString()
    {
        var stringBuilder = new StringBuilder();
        
        stringBuilder.AppendLine($"Component({Id}, \"{Title}\", \"{Protocol}\", \"{Description ?? string.Empty}\")");

        if (_note != null)
        {
            stringBuilder.Write(_note);
        }

        return stringBuilder.ToString();
    }
}