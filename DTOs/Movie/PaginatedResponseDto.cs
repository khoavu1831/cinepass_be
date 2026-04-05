namespace CinePass_be.DTOs.Movie;

public class PaginatedResponseDto<T>
{
  public List<T> Data { get; set; } = [];
  public int Total { get; set; }
  public int Page { get; set; }
  public int PageSize { get; set; }
}
