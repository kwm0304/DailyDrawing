
using Server.DTOs;
using Server.Entities;

namespace Server.Services.Interfaces;

public interface ICommentService
{
  Task<bool> DeleteCommentAsync(int id, string? userId);
  Task<Comment?> GetCommentById(int id);
  Task<Comment?> GetProtectedCommentById(int commentId, string userId);
  Task<PagedResponse<Comment>> GetCommentsForDrawing(int drawingId, int page, int pageSize);
  Task<Comment> CreateComment(AppUser commentor, CreateCommentDto dto, Drawing drawing);
  Task<Comment?> UpdateCommentById(UpdateCommentDto dto);

}
