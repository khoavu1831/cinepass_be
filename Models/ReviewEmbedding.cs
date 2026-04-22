namespace CinePass_be.Models;

public class ReviewEmbedding
{
    public int Id { get; set; }
    public int ReviewId { get; set; }
    public int MovieId { get; set; }

    public float[] MovieDescriptionVector { get; set; } = [];
    public float[] ReviewContentVector { get; set; } = [];
    public float[] CombinedVector { get; set; } = [];

    public string EmbeddingModel { get; set; } = "text-embedding-3-small";
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }

    public Review Review { get; set; } = null!;
    public Movie Movie { get; set; } = null!;
}
