using Server.DTOs;
using Server.Entities;

namespace Server.Services.Interfaces;

public interface IDrawingService
{
  Task<PagedResponse<DrawingDto>> GetAllPaginatedDrawingsForUser(string userId, int page, int pageSize);
  Task<DrawingDto> GetDrawingDtoById(int id);
  Task<Drawing> SubmitDrawing(DrawingUploadDto dto, AppUser user);
  Task<List<Drawing>> GetAllUserDrawings(string userId);
  Task<bool> DeleteDrawingById(string userId, int drawingId);
  Task<Drawing?> UpdateDrawingById(string userId, int drawingId, UpdateDrawingDto dto);
  Task<Drawing?> GetProtectedDrawingById(string userId, int drawingId);
  Task<Drawing?> GetDrawingById(int drawingId); 
}
