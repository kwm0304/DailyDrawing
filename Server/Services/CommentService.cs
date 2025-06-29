using Microsoft.EntityFrameworkCore;
using Server.Data;
using Server.DTOs;
using Server.Entities;
using Server.Mappers;
using Server.Services.Interfaces;

namespace Server.Services;

public class CommentService(AppDbContext context, Mapper mapper, ILogger<CommentService> logger) : ICommentService
{
  private readonly AppDbContext _context = context;
  private readonly Mapper _mapper = mapper;
  private readonly ILogger<CommentService> _logger = logger;

  public async Task<Comment> CreateComment(AppUser commentor, CreateCommentDto dto, Drawing drawing)
  {
    var created = _mapper.ConvertDtoToComment(dto, commentor, drawing);
    _context.Comments.Add(created);
    await _context.SaveChangesAsync();
    return created;
  }

  public async Task<bool> DeleteCommentAsync(int id, string? userId)
  {
    var comment = await GetProtectedCommentById(id, userId ?? string.Empty);
    if (comment == null) return false;
    _context.Comments.Remove(comment);
    await _context.SaveChangesAsync();
    return true;
  }

  public async Task<Comment?> GetCommentById(int id)
  {
    return await _context.Comments.FirstOrDefaultAsync(c => c.Id == id);
  }

  public async Task<Comment?> GetProtectedCommentById(int commentId, string userId)
  {
    return await _context.Comments
      .FirstOrDefaultAsync(c => c.UserId == userId && c.Id == commentId);
  }

  public async Task<PagedResponse<Comment>> GetCommentsForDrawing(int drawingId, int page, int pageSize)
  {
    page = Math.Max(1, page);
    pageSize = Math.Clamp(pageSize, 1, 50);
    var totalCount = await _context.Comments
      .Where(c => c.DrawingId == drawingId)
      .CountAsync();
    var comments = await _context.Comments
      .Where(c => c.DrawingId == drawingId)
      .Include(c => c.User)
      .Include(c => c.Replies)
        .ThenInclude(r => r.User)
      .OrderByDescending(c => c.CommentedAt)
      .Skip((page - 1) * pageSize)
      .Take(pageSize)
      .ToListAsync();
    var totalPages = (int)Math.Ceiling((double)totalCount / pageSize);
    return _mapper.ConvertCommentToPagedResponse(comments, page, pageSize, totalCount, totalPages);
  }

  public async Task<Comment?> UpdateCommentById(UpdateCommentDto dto)
  {
    var existing = await GetCommentById(dto.CommentId);
    if (existing == null) return null;
    try
    {
      string formatted = dto.NewContent.Trim();
      if (string.Equals(existing.Content, formatted))
      {
        return null;
      }
      existing.Content = formatted;
      existing.UpdatedAt = DateTime.UtcNow;
      await _context.SaveChangesAsync();
      _logger.LogInformation("Updated comment {CommentId} for user {UserId}", existing.Id, existing.UserId);
      return existing;
    }
    catch (Exception ex)
    {
      _logger.LogError(ex, "Failed to update comment {CommentId} for user {UserId}", existing.Id, existing.UserId);
      throw;
    }
  }
}