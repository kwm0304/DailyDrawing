using Microsoft.AspNetCore.Mvc;
using Server.Entities;

namespace Server.Services.Interfaces;

public interface IS3Service
{
  Task<string> UploadFileAsync(IFormFile file, string title, string description);
  Task DeleteUserDrawingFilesAsync(List<Drawing> drawings);
  Task<bool> DeleteFileAsync(string filePath);
  Task<string> GetPresignedUrlAsync(string filePath, TimeSpan? expiration);

}
