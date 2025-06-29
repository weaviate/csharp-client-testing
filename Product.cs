namespace Example;

public record Product
{
    public string? Name { get; set; }
    public string? Description { get; set; }
    public string? Brand { get; set; }
    public string? Image { get; set; }
    public float Price { get; set; }

    public override string ToString()
    {
        return $"Product {{ Name: {Name}, Brand: {Brand}, Price: ${Price:F2}, Description: {Description} }}";
    }
}
