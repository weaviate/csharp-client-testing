namespace Example;

public record Category
{
    public string? Name { get; set; }
    public string? Description { get; set; }
    public bool IsActive { get; set; }
    public float[]? Vector { get; set; }

    public override string ToString()
    {
        return $"Category {{ Name: {Name}, Active: {IsActive} }}";
    }
}
