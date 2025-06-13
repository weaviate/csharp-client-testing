using System;
using System.Collections.Generic;

namespace WeaviateProject;

public class Article
{
    public string Title { get; set; } = string.Empty;
    public string Content { get; set; } = string.Empty;
    public string Author { get; set; } = string.Empty;
    public DateTime PublishedDate { get; set; }
    public List<string> Tags { get; set; } = new();
    public string? Url { get; set; }
}