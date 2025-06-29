using Amazon;
using Amazon.S3;
using Amazon.S3.Model;
using Microsoft.AspNetCore.Mvc;
using Server.Entities;
using Server.Services.Interfaces;

namespace Server.Services;

public class S3Service : IS3Service
{
  private readonly string _bucketName;
  private readonly AmazonS3Client _s3Client;
  private readonly ILogger<S3Service> _logger;
  public S3Service(IConfiguration config, ILogger<S3Service> logger)
  {
    var accessKey = config["AWS:AccessKey"];
    var secretKey = config["AWS:SecretKey"];
    _bucketName = config["AWS:BucketName"] ?? throw new Exception("Bucket not found");
    var region = RegionEndpoint.GetBySystemName(config["AWS:Region"]);
    _s3Client = new AmazonS3Client(accessKey, secretKey, region);
    _logger = logger;
  }

  public async Task<bool> DeleteFileAsync(string filePath)
  {
    try
    {
      var request = new DeleteObjectRequest
      {
        BucketName = _bucketName,
        Key = filePath
      };
      var response = await _s3Client.DeleteObjectAsync(request);
      return response.HttpStatusCode == System.Net.HttpStatusCode.NoContent;
    }
    catch (Exception ex)
    {
      _logger.LogError(ex, "Failed to delete file {FilePath} from S3", filePath);
      return false;
    }
  }


  public async Task DeleteUserDrawingFilesAsync(List<Drawing> drawings)
  {
    if (drawings.Count == 0) return;
    var deleteTasks = drawings.Select(async drawing =>
    {
      try
      {
        bool result = await DeleteFileAsync(drawing.S3Key);
        return (drawing.Id, Success: result, Error: "");
      }
      catch (Exception ex)
      {
        return (drawing.Id, Success: false, Error: ex.Message);
      }
    });
    await Task.WhenAll(deleteTasks);
  }

  public async Task<string> GetPresignedUrlAsync(string filePath, TimeSpan? expiration = null)
  {
    try
    {
      var expiresAt = expiration ?? TimeSpan.FromHours(1);
      var request = new GetPreSignedUrlRequest
      {
        BucketName = _bucketName,
        Key = filePath,
        Verb = HttpVerb.GET,
        Expires = DateTime.UtcNow.Add(expiresAt)
      };
      var presignedUrl = await _s3Client.GetPreSignedURLAsync(request);
      _logger.LogDebug("Generated presigned URL for {FilePath}, expires at {ExpiresAt}", filePath, request.Expires);
      return presignedUrl;
    }
    catch (Exception ex)
    {
      _logger.LogError(ex, "Failed to generate presigned URL for file {FilePath}", filePath);
      throw;
    }
  }

  public async Task<string> UploadFileAsync(IFormFile file, [FromForm] string title, [FromForm] string Description)
  {

    return "";
  }
}
