using System.Text;

namespace Futurum.OpenApiToPlantuml;

public record ClassRelationship(string LeftId, ClassRelationshipType Type, ClassRelationshipCardinality Cardinality, string RightId, string? Label = null) : IPlantUmlWriter
{
    public override string ToString()
    {
        var stringBuilder = new StringBuilder();

        var labelString = !string.IsNullOrEmpty(Label)
            ? $" : \"{Label}\""
            : string.Empty;
        
        stringBuilder.Append($"\"{LeftId}\"" +
                             $" {TransformTypeToString(Type)} {TransformCardinalityToString(Cardinality)}" +
                             $" \"{RightId}\"" +
                             $"{labelString}");

        return stringBuilder.ToString();
    }

    private static string TransformTypeToString(ClassRelationshipType type) =>
        type switch
        {
            ClassRelationshipType.Reference => "..>",
            ClassRelationshipType.AllOf     => "-->",
            ClassRelationshipType.AnyOf     => "--|>",
            ClassRelationshipType.OneOf     => "--|>",
            _                               => string.Empty
        };

    private static string TransformCardinalityToString(ClassRelationshipCardinality cardinality) =>
        cardinality switch
        {
            ClassRelationshipCardinality.None => string.Empty,
            ClassRelationshipCardinality.One  => "\"1\"",
            ClassRelationshipCardinality.Many => "\"*\"",
            _                                 => string.Empty
        };
}