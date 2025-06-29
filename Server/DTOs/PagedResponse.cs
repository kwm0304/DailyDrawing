namespace Server.DTOs;

public class PagedResponse<T>
{
  public List<T> Data { get; set; } = [];
  public int CurrentPage { get; set; }
  public int PageSize { get; set; }
  public int TotalPages { get; set; }
  public int TotalCount { get; set; }
  public bool HasNextPage { get; set; }
  public bool HasPreviousPage { get; set; }
}
