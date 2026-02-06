using BotFlow.Application.Common.DTOs.AIDataSources;
using Microsoft.AspNetCore.Http;

namespace BotFlow.Application.Interfaces
{
    public interface IAIDataSourceService
    {
        Task<DataSourceListDto> GetAllDataSourcesAsync(DataSourceFilter filter);
        Task<AIDataSourceDto> GetDataSourceByIdAsync(Guid id);
        Task<AIDataSourceDto> CreateDataSourceAsync(CreateDataSourceRequest request);
        Task<AIDataSourceDto> UploadDocumentAsync(IFormFile file, UploadDocumentRequest request);
        Task<bool> ConnectUrlAsync(ConnectUrlRequest request);
        Task<bool> ConnectApiAsync(CreateDataSourceRequest request);
        Task<bool> ConnectDatabaseAsync(CreateDataSourceRequest request);
        Task<AIDataSourceDto> UpdateDataSourceAsync(Guid id, UpdateDataSourceRequest request);
        Task<bool> DeleteDataSourceAsync(Guid id);
        Task<bool> ProcessDataSourceAsync(Guid id);
        Task<bool> RetryFailedDataSourceAsync(Guid id);
        Task<bool> StopProcessingDataSourceAsync(Guid id);
        Task<DataSourceStatsDto> GetDataSourceStatsAsync();
        Task<List<AIDataSourceDto>> GetDataSourcesByStatusAsync(string status);
        Task<List<AIDataSourceDto>> GetDataSourcesByTypeAsync(string type);
        Task<bool> BulkDeleteDataSourcesAsync(List<Guid> ids);
        Task<bool> BulkUpdateStatusAsync(List<Guid> ids, string status);
    }
}