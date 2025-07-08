using System;
using System.Text.Json.Serialization;

namespace Example;

public record Product
{
    [JsonPropertyName("name")]
    public string? Name { get; set; }

    [JsonPropertyName("description")]
    public string? Description { get; set; }

    [JsonPropertyName("brand")]
    public string? Brand { get; set; }

    [JsonPropertyName("image")]
    public string? Image { get; set; }

    [JsonPropertyName("price")]
    public float Price { get; set; }

    [JsonPropertyName("stockQuantity")]
    public int StockQuantity { get; set; }

    [JsonPropertyName("isAvailable")]
    public bool IsAvailable { get; set; }

    [JsonPropertyName("releaseDate")]
    public DateTime ReleaseDate { get; set; }

    [JsonPropertyName("tags")]
    public string[]? Tags { get; set; }

    [JsonPropertyName("ratings")]
    public float[]? Ratings { get; set; }

    [JsonPropertyName("category")]
    public string? Category { get; set; }

    public override string ToString()
    {
        return $"Product {{ Name: {Name}, Brand: {Brand}, Price: ${Price:F2}, Stock: {StockQuantity}, Available: {IsAvailable} }}";
    }
}
