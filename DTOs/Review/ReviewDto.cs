namespace CinePass_be.DTOs;

public class CreateReviewDto
{
    public string Title { get; set; } = string.Empty;
    public string Content { get; set; } = string.Empty;
    public decimal Rating { get; set; }
    public bool HasSpoiler { get; set; } = false;
}

public class UpdateReviewDto
{
    public string? Title { get; set; }
    public string? Content { get; set; }
    public decimal? Rating { get; set; }
    public bool? HasSpoiler { get; set; }
}

public class ReviewResponseDto
{
    public int Id { get; set; }
    public int UserId { get; set; }
    public string Username { get; set; } = string.Empty;
    public string? UserAvatarUrl { get; set; }
    public int MovieId { get; set; }
    public string MovieTitle { get; set; } = string.Empty;

    public string Title { get; set; } = string.Empty;
    public string Content { get; set; } = string.Empty;
    public decimal Rating { get; set; }

    public bool HasSpoiler { get; set; }
    public bool IsEdited { get; set; }
    public DateTime? EditedAt { get; set; }

    public int LikeCount { get; set; }
    public int CommentCount { get; set; }
    public bool IsLikedByCurrentUser { get; set; }

    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
}

public class CreateCommentDto
{
    public string Content { get; set; } = string.Empty;
}

public class CommentResponseDto
{
    public int Id { get; set; }
    public int UserId { get; set; }
    public string Username { get; set; } = string.Empty;
    public string? UserAvatarUrl { get; set; }
    public int ReviewId { get; set; }
    public string Content { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
}
