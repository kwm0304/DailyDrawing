using Server.DTOs;
using Server.Entities;

namespace Server.Mappers;

public class Mapper
{
  public DrawingDto ConvertDrawingToDto(Drawing drawing)
  {
    return new DrawingDto
    {
      Id = drawing.Id,
      Title = drawing.Title,
      Description = drawing.Description,
      CreatedAt = drawing.CreatedAt,
      UpvoteCount = drawing.Upvotes?.Count ?? 0,
      CommentCount = drawing.Comments?.Count ?? 0,
      Tags = drawing.Tags?.Select(tag => tag.TagName).ToList() ?? [],
      Badges = drawing.Badges.ToList() ?? []
    };
  }

  public Drawing ConvertDrawingUploadDtoToDrawing(DrawingUploadDto dto, AppUser user)
  {
    return new Drawing
    {
      Title = dto.Title,
      Tags = dto.Tags,
      Description = !string.IsNullOrWhiteSpace(dto.Description) ? dto.Description : string.Empty,
      UserId = user.Id,
      User = user
    };
  }

  public PagedResponse<Comment> ConvertCommentToPagedResponse(List<Comment> comments, int page, int pageSize, int totalCount, int totalPages)
  {
    return new PagedResponse<Comment>
    {
      Data = comments,
      CurrentPage = page,
      PageSize = pageSize,
      TotalCount = totalCount,
      TotalPages = totalPages,
      HasNextPage = page < totalPages,
      HasPreviousPage = page > 1
    };
  }
  
  public PagedResponse<DrawingDto> ConvertDrawingDtoToPagedResponse(List<DrawingDto> dtoList, int page, int pageSize, int totalCount, int totalPages)
  {
    return new PagedResponse<DrawingDto>
    {
      Data = dtoList,
      CurrentPage = page,
      PageSize = pageSize,
      TotalCount = totalCount,
      TotalPages = totalPages,
      HasNextPage = page < totalPages,
      HasPreviousPage = page > 1
    };
  }

  public Comment ConvertDtoToComment(CreateCommentDto dto, AppUser commentor, Drawing drawing)
  {
    //assign in services:CommentId, 
    return new Comment
    {
      Content = dto.Content,
      User = commentor,
      UserId = commentor.Id,
      ParentComment = dto.ParentComment ?? null,
      ParentCommentId = dto.ParentCommentId ?? null,
      Drawing = drawing,
      DrawingId = drawing.Id
    };
  }
}
