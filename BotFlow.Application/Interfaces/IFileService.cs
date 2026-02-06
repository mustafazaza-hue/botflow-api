using Microsoft.AspNetCore.Http;

namespace BotFlow.Application.Interfaces
{
    public interface IFileService
    {
        Task<string> UploadFileAsync(IFormFile file, string folder);
        Task<bool> DeleteFileAsync(string fileUrl);
        Task<byte[]> GetFileAsync(string fileUrl);
        Task<string> UploadFileAsync(byte[] fileBytes, string fileName, string folder);
        Task<bool> FileExistsAsync(string fileUrl);
        Task<long> GetFileSizeAsync(string fileUrl);
        Task<string> GetFileExtensionAsync(string fileUrl);
    }
}