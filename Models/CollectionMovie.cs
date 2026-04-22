namespace CinePass_be.Models;

public class CollectionMovie
{
    public int CollectionId { get; set; }
    public Collection Collection { get; set; } = null!;

    public int MovieId { get; set; }
    public Movie Movie { get; set; } = null!;

    public int OrderIndex { get; set; } = 0;
}
