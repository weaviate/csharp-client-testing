using System;
using System.Text.Json.Serialization;

namespace WeaviateProject;

public record Product
{
    [JsonPropertyName("name")]
    public string? Name { get; set; }

    [JsonPropertyName("description")]
    public string? Description { get; set; }

    [JsonPropertyName("price")]
    public float Price { get; set; }

    [JsonPropertyName("isAvailable")]
    public bool IsAvailable { get; set; }

    public override string ToString()
    {
        return $"Product {{ Name: {Name}, Price: ${Price:F2}, Available: {IsAvailable} }}";
    }
}
