using System;

namespace Example;

public record Product
{
    public string? Name { get; set; }
    public string? Description { get; set; }
    public string? Brand { get; set; }
    public string? Image { get; set; }
    public float Price { get; set; }
    public int StockQuantity { get; set; }
    public bool IsAvailable { get; set; }
    public DateTime ReleaseDate { get; set; }
    public string[]? Tags { get; set; }
    public float[]? Ratings { get; set; }
    public string? Category { get; set; }

    public override string ToString()
    {
        return $"Product {{ Name: {Name}, Brand: {Brand}, Price: ${Price:F2}, Stock: {StockQuantity}, Available: {IsAvailable} }}";
    }
}

