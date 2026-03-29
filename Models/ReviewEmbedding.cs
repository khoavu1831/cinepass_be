namespace CinePass_be.Models;

/// <summary>
/// ReviewEmbedding model - Store vector embeddings for AI semantic search
/// Used for finding movies by description/sentiment
/// Contains embeddings for movie descriptions and review content
/// </summary>
public class ReviewEmbedding
{
    // Identity
    public int Id { get; set; }
    public int ReviewId { get; set; }
    public int MovieId { get; set; }

    // Vector Embeddings (1536 dimensions for OpenAI text-embedding-3-small)
    // These are stored as arrays of floats
    public float[] MovieDescriptionVector { get; set; } = [];  // Movie description embedding
    public float[] ReviewContentVector { get; set; } = [];     // Review content embedding
    public float[] CombinedVector { get; set; } = [];          // Optional: weighted average

    // Metadata
    public string EmbeddingModel { get; set; } = "text-embedding-3-small";  // Track which model generated this
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }

    // Navigation Properties
    public Review Review { get; set; } = null!;
    public Movie Movie { get; set; } = null!;
}
