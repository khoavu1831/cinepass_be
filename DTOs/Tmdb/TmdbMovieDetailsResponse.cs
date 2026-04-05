using System.Text.Json.Serialization;

namespace CinePass_be.DTOs.Tmdb;

public class TmdbMovieDetailsResponse
{
    [JsonPropertyName("id")]
    public int Id { get; set; }

    [JsonPropertyName("title")]
    public string Title { get; set; } = string.Empty;

    [JsonPropertyName("original_title")]
    public string OriginalTitle { get; set; } = string.Empty;

    [JsonPropertyName("overview")]
    public string Overview { get; set; } = string.Empty;

    [JsonPropertyName("poster_path")]
    public string? PosterPath { get; set; }

    [JsonPropertyName("backdrop_path")]
    public string? BackdropPath { get; set; }

    [JsonPropertyName("release_date")]
    public string? ReleaseDate { get; set; }

    [JsonPropertyName("runtime")]
    public int Runtime { get; set; }

    [JsonPropertyName("genres")]
    public List<TmdbGenre> Genres { get; set; } = [];

    [JsonPropertyName("credits")]
    public TmdbCredits? Credits { get; set; }

    [JsonPropertyName("videos")]
    public TmdbVideos? Videos { get; set; }

    [JsonPropertyName("vote_average")]
    public decimal VoteAverage { get; set; }

    [JsonPropertyName("vote_count")]
    public int VoteCount { get; set; }

    [JsonPropertyName("original_language")]
    public string? OriginalLanguage { get; set; }
}

public class TmdbGenre
{
    [JsonPropertyName("id")]
    public int Id { get; set; }

    [JsonPropertyName("name")]
    public string Name { get; set; } = string.Empty;
}

public class TmdbCredits
{
    [JsonPropertyName("cast")]
    public List<TmdbCastMember> Cast { get; set; } = [];

    [JsonPropertyName("crew")]
    public List<TmdbCrewMember> Crew { get; set; } = [];
}

public class TmdbCastMember
{
    [JsonPropertyName("id")]
    public int Id { get; set; }

    [JsonPropertyName("name")]
    public string Name { get; set; } = string.Empty;

    [JsonPropertyName("character")]
    public string Character { get; set; } = string.Empty;

    [JsonPropertyName("order")]
    public int Order { get; set; }
}

public class TmdbCrewMember
{
    [JsonPropertyName("id")]
    public int Id { get; set; }

    [JsonPropertyName("name")]
    public string Name { get; set; } = string.Empty;

    [JsonPropertyName("job")]
    public string Job { get; set; } = string.Empty;

    [JsonPropertyName("department")]
    public string Department { get; set; } = string.Empty;
}

public class TmdbVideos
{
    [JsonPropertyName("results")]
    public List<TmdbVideo> Results { get; set; } = [];
}

public class TmdbVideo
{
    [JsonPropertyName("id")]
    public string Id { get; set; } = string.Empty;

    [JsonPropertyName("key")]
    public string Key { get; set; } = string.Empty;

    [JsonPropertyName("name")]
    public string Name { get; set; } = string.Empty;

    [JsonPropertyName("site")]
    public string Site { get; set; } = string.Empty;

    [JsonPropertyName("type")]
    public string Type { get; set; } = string.Empty;

    [JsonPropertyName("official")]
    public bool Official { get; set; }
}
