namespace CinePass_be.Models;

public class Collection
{
    public int Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string Type { get; set; } = "standard_horizontal";
    public string? ThumbnailUrl { get; set; }
    public int DisplayOrder { get; set; } = 0;
    
    public ICollection<CollectionMovie> CollectionMovies { get; set; } = [];
}
