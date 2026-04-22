namespace CinePass_be.DTOs;

public class CollectionDto
{
    public int Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string Type { get; set; } = "standard_horizontal";
    public string? ThumbnailUrl { get; set; }
    public int DisplayOrder { get; set; }
    public List<MovieResponseDto> Movies { get; set; } = [];
}

public class CreateCollectionDto
{
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string Type { get; set; } = "standard_horizontal";
    public string? ThumbnailUrl { get; set; }
    public int DisplayOrder { get; set; }
}

public class UpdateCollectionDto : CreateCollectionDto
{
}

public class AddMovieToCollectionDto
{
    public int MovieId { get; set; }
}

public class ReorderCollectionMoviesDto
{
    // A list of MovieIds in their new order
    public List<int> MovieIds { get; set; } = [];
}
