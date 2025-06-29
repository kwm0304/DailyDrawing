using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.EntityFrameworkCore;
using Server.Data;
using Server.DTOs;
using Server.Entities;
using Server.Mappers;
using Server.Services.Interfaces;

namespace Server.Services;

public class DrawingService(AppDbContext context, IS3Service s3Service, Mapper mapper, ILogger<DrawingService> logger) : IDrawingService
{
  private readonly AppDbContext _context = context;
  private readonly IS3Service _s3Service = s3Service;
  private readonly Mapper _mapper = mapper;
  private readonly ILogger<DrawingService> _logger = logger;
  public async Task<PagedResponse<DrawingDto>> GetAllPaginatedDrawingsForUser(string userId, int page, int pageSize)
  {
    page = Math.Max(1, page);
    pageSize = Math.Clamp(pageSize, 1, 50);
    var totalCount = await _context.Drawings
      .Where(d => d.UserId == userId)
      .CountAsync();

    var userDrawings = await _context.Drawings
      .Where(d => d.UserId == userId)
      .Include(d => d.Upvotes)
      .Include(d => d.Badges)
      .Skip((page - 1) * pageSize)
      .Take(pageSize)
      .ToListAsync();

    var dtoList = new List<DrawingDto>();
    foreach (var drawing in userDrawings)
    {
      var fullUrl = await _s3Service.GetPresignedUrlAsync(drawing.FileName, null);
      DrawingDto dto = _mapper.ConvertDrawingToDto(drawing);
      dto.FullImageUrl = fullUrl;
      dtoList.Add(dto);
    }
    var totalPages = (int)Math.Ceiling((double)totalCount / pageSize);

    return _mapper.ConvertDrawingDtoToPagedResponse(dtoList, page, pageSize, totalCount, totalPages);
  }

  public async Task<DrawingDto> GetDrawingDtoById(int id)
  {
    var drawing = await _context.Drawings
    .Include(d => d.Upvotes)
    .Include(d => d.Comments)
    .Include(d => d.Badges)
    .SingleOrDefaultAsync(d => d.Id == id);

    string fullUrl = await _s3Service.GetPresignedUrlAsync(drawing!.FileName, null);
    DrawingDto dto = _mapper.ConvertDrawingToDto(drawing!);
    dto.FullImageUrl = fullUrl;
    return dto;
  }


  public async Task<Drawing> SubmitDrawing(DrawingUploadDto dto, AppUser user)
  {
    Drawing created = _mapper.ConvertDrawingUploadDtoToDrawing(dto, user);
    string userId = user.Id;
    created = AddFileMetadata(created, dto, userId);
    await _context.AddAsync(created);
    return created;
  }
  private static Drawing AddFileMetadata(Drawing created, DrawingUploadDto dto, string userId)
  {
    string fileName = dto.File.FileName;
    string contentType = Path.GetExtension(fileName);
    string s3Key = GenerateS3Key(contentType, userId);
    long fileSize = dto.File.Length;
    created.FileName = fileName;
    created.ContentType = contentType;
    created.S3Key = s3Key;
    created.FileSize = fileSize;
    return created;
  }

  private static string GenerateS3Key(string contentType, string userId)
  {
    var uniqueId = Guid.NewGuid();
    return $"drawings/{userId}/{uniqueId}/{contentType}";
  }

  public async Task<List<Drawing>> GetAllUserDrawings(string userId)
  {
    return await _context.Drawings.Where(d => d.UserId == userId).ToListAsync();
  }

  public async Task<Drawing?> GetProtectedDrawingById(string userId, int drawingId)
  {
    return await _context.Drawings
      .FirstOrDefaultAsync(d => d.UserId == userId && d.Id == drawingId);
  }

  public async Task<Drawing?> GetDrawingById(int drawingId)
  {
    return await _context.Drawings.FirstOrDefaultAsync(d => d.Id == drawingId);
  }

  public async Task<bool> DeleteDrawingById(string userId, int drawingId)
  {
    var drawing = await GetProtectedDrawingById(userId, drawingId);
    if (drawing == null) return false;
    try
    {
      await _s3Service.DeleteFileAsync(drawing.S3Key);
      _context.Drawings.Remove(drawing);
      await _context.SaveChangesAsync();
      return true;
    }
    catch (Exception ex)
    {
      _logger.LogError(ex, "Failed to delete drawing {DrawingId} for user {UserId}", drawingId, userId);
      throw;
    }
  }

  public async Task<Drawing?> UpdateDrawingById(string userId, int drawingId, UpdateDrawingDto dto)
  {
    var drawing = await GetProtectedDrawingById(userId, drawingId);
    if (drawing == null) return null;
    try
    {
      if (!string.IsNullOrWhiteSpace(dto.NewDescription))
      {
        drawing.Description = dto.NewDescription.Trim();
      }
      if (!string.IsNullOrWhiteSpace(dto.NewTitle))
      {
        drawing.Title = dto.NewTitle.Trim();
      }
      drawing.Tags = dto.NewTags ?? [];
      await _context.SaveChangesAsync();

      _logger.LogInformation("Updated drawing {DrawingId} for user {UserId}", drawingId, userId);
      return drawing;
    }
    catch (Exception ex)
    {
      _logger.LogError(ex, "Failed to update drawing {DrawingId} for user {UserId}", drawingId, userId);
      throw;
    }
  }
}