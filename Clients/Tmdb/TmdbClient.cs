using System.Net.Http.Json;
using CinePass_be.DTOs.Tmdb;
using Microsoft.Extensions.Configuration;

namespace CinePass_be.Clients.Tmdb;

public class TmdbClient : ITmdbClient
{
    private readonly HttpClient _httpClient;
    private readonly string _apiKey;
    private readonly string _baseUrl;

    public TmdbClient(HttpClient httpClient, IConfiguration configuration)
    {
        _httpClient = httpClient;
        _apiKey = configuration["Tmdb:Token"] ?? string.Empty;
        _baseUrl = configuration["Tmdb:BaseUrl"] ?? "https://api.themoviedb.org/3/";

        if (string.IsNullOrEmpty(_apiKey))
            throw new InvalidOperationException("TMDB API key is not configured. Set 'Tmdb:Token' in appsettings.json");
    }

    public async Task<TmdbMovieDetailsResponse?> GetMovieDetailsAsync(int tmdbId)
    {
        try
        {
            var url = $"{_baseUrl}movie/{tmdbId}?api_key={_apiKey}&append_to_response=credits,videos";
            var response = await _httpClient.GetAsync(url);

            if (!response.IsSuccessStatusCode)
            {
                Console.WriteLine($"TMDB API error: {response.StatusCode} - {await response.Content.ReadAsStringAsync()}");
                return null;
            }

            var movie = await response.Content.ReadFromJsonAsync<TmdbMovieDetailsResponse>();
            return movie;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error fetching TMDB movie details: {ex.Message}");
            return null;
        }
    }

    public async Task<TmdbSearchMovieResponse?> SearchMoviesAsync(string query, int page = 1)
    {
        try
        {
            var encodedQuery = Uri.EscapeDataString(query);
            var url = $"{_baseUrl}search/movie?api_key={_apiKey}&query={encodedQuery}&page={page}";
            var response = await _httpClient.GetAsync(url);

            if (!response.IsSuccessStatusCode)
            {
                Console.WriteLine($"TMDB API error: {response.StatusCode}");
                return null;
            }

            var result = await response.Content.ReadFromJsonAsync<TmdbSearchMovieResponse>();
            return result;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error searching TMDB movies: {ex.Message}");
            return null;
        }
    }

    public async Task<TmdbSearchMovieResponse?> GetPopularMoviesAsync(int page = 1, string region = "US")
    {
        try
        {
            var url = $"{_baseUrl}movie/popular?api_key={_apiKey}&page={page}&region={region}";
            var response = await _httpClient.GetAsync(url);

            if (!response.IsSuccessStatusCode)
                return null;

            var result = await response.Content.ReadFromJsonAsync<TmdbSearchMovieResponse>();
            return result;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error fetching popular movies: {ex.Message}");
            return null;
        }
    }

    public async Task<TmdbSearchMovieResponse?> GetTopRatedMoviesAsync(int page = 1)
    {
        try
        {
            var url = $"{_baseUrl}movie/top_rated?api_key={_apiKey}&page={page}";
            var response = await _httpClient.GetAsync(url);

            if (!response.IsSuccessStatusCode)
                return null;

            var result = await response.Content.ReadFromJsonAsync<TmdbSearchMovieResponse>();
            return result;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error fetching top rated movies: {ex.Message}");
            return null;
        }
    }

    public async Task<TmdbSearchMovieResponse?> GetUpcomingMoviesAsync(int page = 1, string region = "US")
    {
        try
        {
            var url = $"{_baseUrl}movie/upcoming?api_key={_apiKey}&page={page}&region={region}";
            var response = await _httpClient.GetAsync(url);

            if (!response.IsSuccessStatusCode)
                return null;

            var result = await response.Content.ReadFromJsonAsync<TmdbSearchMovieResponse>();
            return result;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error fetching upcoming movies: {ex.Message}");
            return null;
        }
    }

    public async Task<TmdbSearchMovieResponse?> GetMoviesByGenreAsync(int genreId, int page = 1, string sortBy = "popularity.desc")
    {
        try
        {
            var url = $"{_baseUrl}discover/movie?api_key={_apiKey}&with_genres={genreId}&page={page}&sort_by={sortBy}";
            var response = await _httpClient.GetAsync(url);

            if (!response.IsSuccessStatusCode)
                return null;

            var result = await response.Content.ReadFromJsonAsync<TmdbSearchMovieResponse>();
            return result;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error fetching movies by genre: {ex.Message}");
            return null;
        }
    }

    public async Task<TmdbSearchMovieResponse?> GetNowPlayingMoviesAsync(int page = 1, string region = "US")
    {
        try
        {
            var url = $"{_baseUrl}movie/now_playing?api_key={_apiKey}&page={page}&region={region}";
            var response = await _httpClient.GetAsync(url);

            if (!response.IsSuccessStatusCode)
                return null;

            var result = await response.Content.ReadFromJsonAsync<TmdbSearchMovieResponse>();
            return result;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error fetching now playing movies: {ex.Message}");
            return null;
        }
    }
}
