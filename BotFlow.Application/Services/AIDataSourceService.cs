using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.Http;
using BotFlow.Application.Interfaces;
using BotFlow.Infrastructure.Data;
using BotFlow.Domain.Entities;
using BotFlow.Application.Common.DTOs.AIDataSources;

namespace BotFlow.Application.Services
{
    public class AIDataSourceService : IAIDataSourceService
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<AIDataSourceService> _logger;
        private readonly IFileService _fileService;

        public AIDataSourceService(
            ApplicationDbContext context,
            ILogger<AIDataSourceService> logger,
            IFileService fileService)
        {
            _context = context;
            _logger = logger;
            _fileService = fileService;
        }

        public async Task<DataSourceListDto> GetAllDataSourcesAsync(DataSourceFilter filter)
        {
            try
            {
                var query = _context.AIDataSources
                    .Include(d => d.User)
                    .AsQueryable();

                // Apply filters
                if (!string.IsNullOrEmpty(filter.Search))
                {
                    query = query.Where(d => 
                        d.Name.Contains(filter.Search) || 
                        d.Description.Contains(filter.Search));
                }

                if (!string.IsNullOrEmpty(filter.Type))
                {
                    query = query.Where(d => d.Type == filter.Type);
                }

                if (!string.IsNullOrEmpty(filter.Status))
                {
                    query = query.Where(d => d.Status == filter.Status);
                }

                if (filter.StartDate.HasValue)
                {
                    query = query.Where(d => d.CreatedAt >= filter.StartDate.Value);
                }

                if (filter.EndDate.HasValue)
                {
                    query = query.Where(d => d.CreatedAt <= filter.EndDate.Value);
                }

                // Apply sorting
                query = filter.SortBy?.ToLower() switch
                {
                    "name" => filter.SortDescending 
                        ? query.OrderByDescending(d => d.Name)
                        : query.OrderBy(d => d.Name),
                    "createdat" => filter.SortDescending
                        ? query.OrderByDescending(d => d.CreatedAt)
                        : query.OrderBy(d => d.CreatedAt),
                    "status" => filter.SortDescending
                        ? query.OrderByDescending(d => d.Status)
                        : query.OrderBy(d => d.Status),
                    _ => query.OrderByDescending(d => d.CreatedAt)
                };

                // Get total count
                var totalCount = await query.CountAsync();

                // Apply pagination
                var dataSources = await query
                    .Skip((filter.Page - 1) * filter.PageSize)
                    .Take(filter.PageSize)
                    .Select(d => MapToDto(d))
                    .ToListAsync();

                return new DataSourceListDto
                {
                    DataSources = dataSources,
                    TotalCount = totalCount,
                    Page = filter.Page,
                    PageSize = filter.PageSize,
                    TotalPages = (int)Math.Ceiling(totalCount / (double)filter.PageSize)
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting all data sources");
                throw;
            }
        }

        public async Task<AIDataSourceDto> GetDataSourceByIdAsync(Guid id)
        {
            try
            {
                var dataSource = await _context.AIDataSources
                    .Include(d => d.User)
                    .FirstOrDefaultAsync(d => d.Id == id);

                if (dataSource == null)
                    throw new KeyNotFoundException($"Data source with ID {id} not found");

                return MapToDto(dataSource);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting data source by ID: {Id}", id);
                throw;
            }
        }

        public async Task<AIDataSourceDto> CreateDataSourceAsync(CreateDataSourceRequest request)
        {
            try
            {
                var userId = request.UserId ?? GetCurrentUserId();

                var dataSource = new AIDataSource
                {
                    Id = Guid.NewGuid(),
                    Name = request.Name,
                    Type = request.Type,
                    Description = request.Description,
                    UserId = userId,
                    Status = "Processing",
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                };

                // Set type-specific properties
                switch (request.Type.ToLower())
                {
                    case "url":
                        dataSource.Url = request.Url ?? string.Empty;
                        break;
                    case "api":
                        dataSource.ApiEndpoint = request.ApiEndpoint ?? string.Empty;
                        break;
                    case "database":
                        dataSource.DatabaseType = request.DatabaseType ?? string.Empty;
                        break;
                }

                await _context.AIDataSources.AddAsync(dataSource);
                await _context.SaveChangesAsync();

                // Start processing in background
                _ = Task.Run(() => ProcessDataSourceInBackground(dataSource.Id));

                return MapToDto(dataSource);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating data source");
                throw;
            }
        }

        public async Task<AIDataSourceDto> UploadDocumentAsync(IFormFile file, UploadDocumentRequest request)
        {
            try
            {
                if (file == null || file.Length == 0)
                    throw new ArgumentException("No file provided");

                // Validate file type
                var allowedExtensions = new List<string> { ".pdf", ".txt", ".docx", ".json", ".csv" };
                var extension = Path.GetExtension(file.FileName).ToLower();
                
                if (!allowedExtensions.Contains(extension))
                    throw new ArgumentException($"File type {extension} not supported. Allowed types: {string.Join(", ", allowedExtensions)}");

                // Validate file size (max 50MB)
                if (file.Length > 50 * 1024 * 1024)
                    throw new ArgumentException("File size exceeds 50MB limit");

                var userId = request.UserId ?? GetCurrentUserId();

                // Upload file to storage
                var fileUrl = await _fileService.UploadFileAsync(file, "ai-documents");

                // Create data source record
                var dataSource = new AIDataSource
                {
                    Id = Guid.NewGuid(),
                    Name = request.Name,
                    Type = "Document",
                    Description = request.Description,
                    FileUrl = fileUrl,
                    FileType = extension.TrimStart('.'),
                    FileSize = file.Length,
                    UserId = userId,
                    Status = "Processing",
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                };

                await _context.AIDataSources.AddAsync(dataSource);
                await _context.SaveChangesAsync();

                // Start processing in background
                _ = Task.Run(() => ProcessDocumentInBackground(dataSource.Id, fileUrl));

                return MapToDto(dataSource);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error uploading document");
                throw;
            }
        }

        public async Task<bool> ConnectUrlAsync(ConnectUrlRequest request)
        {
            try
            {
                var userId = request.UserId ?? GetCurrentUserId();

                // Validate URL
                if (!Uri.TryCreate(request.Url, UriKind.Absolute, out var uri))
                    throw new ArgumentException("Invalid URL format");

                // Create data source
                var dataSource = new AIDataSource
                {
                    Id = Guid.NewGuid(),
                    Name = request.Name,
                    Type = "URL",
                    Description = request.Description,
                    Url = request.Url,
                    UserId = userId,
                    Status = "Processing",
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                };

                await _context.AIDataSources.AddAsync(dataSource);
                await _context.SaveChangesAsync();

                // Start web crawling in background
                _ = Task.Run(() => CrawlUrlInBackground(dataSource.Id, request.Url));

                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error connecting URL");
                throw;
            }
        }

        public async Task<bool> ConnectApiAsync(CreateDataSourceRequest request)
        {
            try
            {
                var userId = request.UserId ?? GetCurrentUserId();

                // Validate API endpoint
                if (string.IsNullOrEmpty(request.ApiEndpoint))
                    throw new ArgumentException("API endpoint is required");

                // Create data source
                var dataSource = new AIDataSource
                {
                    Id = Guid.NewGuid(),
                    Name = request.Name,
                    Type = "API",
                    Description = request.Description,
                    ApiEndpoint = request.ApiEndpoint,
                    UserId = userId,
                    Status = "Processing",
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                };

                await _context.AIDataSources.AddAsync(dataSource);
                await _context.SaveChangesAsync();

                // Start API integration in background
                _ = Task.Run(() => IntegrateApiInBackground(dataSource.Id, request.ApiEndpoint));

                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error connecting API");
                throw;
            }
        }

        public async Task<bool> ConnectDatabaseAsync(CreateDataSourceRequest request)
        {
            try
            {
                var userId = request.UserId ?? GetCurrentUserId();

                if (string.IsNullOrEmpty(request.DatabaseType))
                    throw new ArgumentException("Database type is required");

                // Create data source
                var dataSource = new AIDataSource
                {
                    Id = Guid.NewGuid(),
                    Name = request.Name,
                    Type = "Database",
                    Description = request.Description,
                    DatabaseType = request.DatabaseType,
                    UserId = userId,
                    Status = "Processing",
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                };

                await _context.AIDataSources.AddAsync(dataSource);
                await _context.SaveChangesAsync();

                // Start database integration in background
                _ = Task.Run(() => IntegrateDatabaseInBackground(dataSource.Id, request.DatabaseType));

                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error connecting database");
                throw;
            }
        }

        public async Task<AIDataSourceDto> UpdateDataSourceAsync(Guid id, UpdateDataSourceRequest request)
        {
            try
            {
                var dataSource = await _context.AIDataSources.FindAsync(id);
                if (dataSource == null)
                    throw new KeyNotFoundException($"Data source with ID {id} not found");

                // Update properties if provided
                if (!string.IsNullOrEmpty(request.Name))
                    dataSource.Name = request.Name;

                if (!string.IsNullOrEmpty(request.Description))
                    dataSource.Description = request.Description;

                if (!string.IsNullOrEmpty(request.Status))
                    dataSource.Status = request.Status;

                dataSource.UpdatedAt = DateTime.UtcNow;
                await _context.SaveChangesAsync();

                return MapToDto(dataSource);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating data source: {Id}", id);
                throw;
            }
        }

        public async Task<bool> DeleteDataSourceAsync(Guid id)
        {
            try
            {
                var dataSource = await _context.AIDataSources.FindAsync(id);
                if (dataSource == null)
                    throw new KeyNotFoundException($"Data source with ID {id} not found");

                // Delete associated file if exists
                if (!string.IsNullOrEmpty(dataSource.FileUrl))
                {
                    await _fileService.DeleteFileAsync(dataSource.FileUrl);
                }

                _context.AIDataSources.Remove(dataSource);
                await _context.SaveChangesAsync();

                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting data source: {Id}", id);
                throw;
            }
        }

        public async Task<bool> ProcessDataSourceAsync(Guid id)
        {
            try
            {
                var dataSource = await _context.AIDataSources.FindAsync(id);
                if (dataSource == null)
                    throw new KeyNotFoundException($"Data source with ID {id} not found");

                dataSource.Status = "Processing";
                dataSource.ProgressPercentage = 0;
                dataSource.UpdatedAt = DateTime.UtcNow;

                await _context.SaveChangesAsync();

                // Start processing based on type
                _ = Task.Run(async () =>
                {
                    try
                    {
                        // Simulate processing
                        for (int i = 0; i <= 100; i += 10)
                        {
                            await Task.Delay(1000); // Simulate work
                            
                            dataSource.ProgressPercentage = i;
                            dataSource.UpdatedAt = DateTime.UtcNow;
                            await _context.SaveChangesAsync();
                        }

                        dataSource.Status = "Active";
                        dataSource.LastProcessedAt = DateTime.UtcNow;
                        await _context.SaveChangesAsync();
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Error processing data source in background: {Id}", id);
                        dataSource.Status = "Failed";
                        dataSource.ErrorMessage = ex.Message;
                        await _context.SaveChangesAsync();
                    }
                });

                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing data source: {Id}", id);
                throw;
            }
        }

        public async Task<bool> RetryFailedDataSourceAsync(Guid id)
        {
            try
            {
                var dataSource = await _context.AIDataSources.FindAsync(id);
                if (dataSource == null)
                    throw new KeyNotFoundException($"Data source with ID {id} not found");

                dataSource.Status = "Processing";
                dataSource.ErrorMessage = string.Empty;
                dataSource.ProgressPercentage = 0;
                dataSource.UpdatedAt = DateTime.UtcNow;

                await _context.SaveChangesAsync();

                // Retry processing
                _ = Task.Run(() => ProcessDataSourceInBackground(id));

                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrying failed data source: {Id}", id);
                throw;
            }
        }

        public async Task<bool> StopProcessingDataSourceAsync(Guid id)
        {
            try
            {
                var dataSource = await _context.AIDataSources.FindAsync(id);
                if (dataSource == null)
                    throw new KeyNotFoundException($"Data source with ID {id} not found");

                if (dataSource.Status == "Processing")
                {
                    dataSource.Status = "Disabled";
                    dataSource.UpdatedAt = DateTime.UtcNow;
                    await _context.SaveChangesAsync();
                }

                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error stopping data source processing: {Id}", id);
                throw;
            }
        }

        public async Task<DataSourceStatsDto> GetDataSourceStatsAsync()
        {
            try
            {
                var dataSources = await _context.AIDataSources.ToListAsync();

                return new DataSourceStatsDto
                {
                    TotalDocuments = dataSources.Count(d => d.Type == "Document"),
                    ActiveSources = dataSources.Count(d => d.Status == "Active"),
                    ProcessingSources = dataSources.Count(d => d.Status == "Processing"),
                    FailedSources = dataSources.Count(d => d.Status == "Failed"),
                    TotalSources = dataSources.Count
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting data source stats");
                throw;
            }
        }

        public async Task<List<AIDataSourceDto>> GetDataSourcesByStatusAsync(string status)
        {
            try
            {
                var dataSources = await _context.AIDataSources
                    .Include(d => d.User)
                    .Where(d => d.Status == status)
                    .OrderByDescending(d => d.CreatedAt)
                    .Select(d => MapToDto(d))
                    .ToListAsync();

                return dataSources;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting data sources by status: {Status}", status);
                throw;
            }
        }

        public async Task<List<AIDataSourceDto>> GetDataSourcesByTypeAsync(string type)
        {
            try
            {
                var dataSources = await _context.AIDataSources
                    .Include(d => d.User)
                    .Where(d => d.Type == type)
                    .OrderByDescending(d => d.CreatedAt)
                    .Select(d => MapToDto(d))
                    .ToListAsync();

                return dataSources;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting data sources by type: {Type}", type);
                throw;
            }
        }

        public async Task<bool> BulkDeleteDataSourcesAsync(List<Guid> ids)
        {
            try
            {
                var dataSources = await _context.AIDataSources
                    .Where(d => ids.Contains(d.Id))
                    .ToListAsync();

                // Delete associated files
                foreach (var dataSource in dataSources)
                {
                    if (!string.IsNullOrEmpty(dataSource.FileUrl))
                    {
                        await _fileService.DeleteFileAsync(dataSource.FileUrl);
                    }
                }

                _context.AIDataSources.RemoveRange(dataSources);
                await _context.SaveChangesAsync();

                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error bulk deleting data sources");
                throw;
            }
        }

        public async Task<bool> BulkUpdateStatusAsync(List<Guid> ids, string status)
        {
            try
            {
                var dataSources = await _context.AIDataSources
                    .Where(d => ids.Contains(d.Id))
                    .ToListAsync();

                foreach (var dataSource in dataSources)
                {
                    dataSource.Status = status;
                    dataSource.UpdatedAt = DateTime.UtcNow;
                }

                await _context.SaveChangesAsync();
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error bulk updating data source status");
                throw;
            }
        }

        // Private helper methods
        private async Task ProcessDataSourceInBackground(Guid id)
        {
            try
            {
                await Task.Delay(3000); // Simulate processing
                
                var dataSource = await _context.AIDataSources.FindAsync(id);
                if (dataSource != null)
                {
                    dataSource.Status = "Active";
                    dataSource.ProgressPercentage = 100;
                    dataSource.LastProcessedAt = DateTime.UtcNow;
                    await _context.SaveChangesAsync();
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in background processing for data source: {Id}", id);
            }
        }

        private async Task ProcessDocumentInBackground(Guid id, string fileUrl)
        {
            try
            {
                await Task.Delay(5000); // Simulate document processing
                
                var dataSource = await _context.AIDataSources.FindAsync(id);
                if (dataSource != null)
                {
                    dataSource.Status = "Active";
                    dataSource.DocumentCount = 1; // Assume 1 document
                    dataSource.LastProcessedAt = DateTime.UtcNow;
                    await _context.SaveChangesAsync();
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in document processing: {Id}", id);
            }
        }

        private async Task CrawlUrlInBackground(Guid id, string url)
        {
            try
            {
                await Task.Delay(4000); // Simulate web crawling
                
                var dataSource = await _context.AIDataSources.FindAsync(id);
                if (dataSource != null)
                {
                    dataSource.Status = "Active";
                    dataSource.DocumentCount = 10; // Assume 10 pages crawled
                    dataSource.LastProcessedAt = DateTime.UtcNow;
                    await _context.SaveChangesAsync();
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in URL crawling: {Id}", id);
            }
        }

        private async Task IntegrateApiInBackground(Guid id, string apiEndpoint)
        {
            try
            {
                await Task.Delay(3000); // Simulate API integration
                
                var dataSource = await _context.AIDataSources.FindAsync(id);
                if (dataSource != null)
                {
                    dataSource.Status = "Active";
                    dataSource.LastProcessedAt = DateTime.UtcNow;
                    await _context.SaveChangesAsync();
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in API integration: {Id}", id);
            }
        }

        private async Task IntegrateDatabaseInBackground(Guid id, string databaseType)
        {
            try
            {
                await Task.Delay(6000); // Simulate database integration
                
                var dataSource = await _context.AIDataSources.FindAsync(id);
                if (dataSource != null)
                {
                    dataSource.Status = "Active";
                    dataSource.LastProcessedAt = DateTime.UtcNow;
                    await _context.SaveChangesAsync();
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in database integration: {Id}", id);
            }
        }

        private AIDataSourceDto MapToDto(AIDataSource dataSource)
        {
            return new AIDataSourceDto
            {
                Id = dataSource.Id,
                Name = dataSource.Name,
                Type = dataSource.Type,
                Status = dataSource.Status,
                Description = dataSource.Description,
                FileType = dataSource.FileType,
                FileSize = dataSource.FileSize,
                FormattedFileSize = FormatFileSize(dataSource.FileSize),
                QueryCount = dataSource.QueryCount,
                DocumentCount = dataSource.DocumentCount,
                Url = dataSource.Url,
                ApiEndpoint = dataSource.ApiEndpoint,
                DatabaseType = dataSource.DatabaseType,
                ErrorMessage = dataSource.ErrorMessage,
                ProgressPercentage = (int)dataSource.ProgressPercentage,
                CreatedAt = dataSource.CreatedAt,
                UpdatedAt = dataSource.UpdatedAt,
                LastProcessedAt = dataSource.LastProcessedAt,
                UserName = dataSource.User?.FirstName + " " + dataSource.User?.LastName ?? "System",
                UserEmail = dataSource.User?.Email ?? "system@botflow.com",
                Icon = GetIconForType(dataSource.Type),
                Color = GetColorForType(dataSource.Type),
                StatusColor = GetStatusColor(dataSource.Status),
                StatusIcon = GetStatusIcon(dataSource.Status)
            };
        }

        private string FormatFileSize(long bytes)
        {
            if (bytes == 0) return "0 B";

            string[] sizes = { "B", "KB", "MB", "GB", "TB" };
            int order = 0;
            double len = bytes;

            while (len >= 1024 && order < sizes.Length - 1)
            {
                order++;
                len /= 1024;
            }
            
            return $"{len:0.##} {sizes[order]}";
        }

        private string GetIconForType(string type)
        {
            return type?.ToLower() switch
            {
                "document" => "FileText",
                "url" => "Link",
                "api" => "Code",
                "database" => "Database",
                _ => "File"
            };
        }

        private string GetColorForType(string type)
        {
            return type?.ToLower() switch
            {
                "document" => "blue",
                "url" => "green",
                "api" => "purple",
                "database" => "orange",
                _ => "gray"
            };
        }

        private string GetStatusColor(string status)
        {
            return status?.ToLower() switch
            {
                "active" => "success",
                "processing" => "warning",
                "failed" => "danger",
                "disabled" => "secondary",
                _ => "secondary"
            };
        }

        private string GetStatusIcon(string status)
        {
            return status?.ToLower() switch
            {
                "active" => "CheckCircle",
                "processing" => "Sync",
                "failed" => "XCircle",
                "disabled" => "Ban",
                _ => "HelpCircle"
            };
        }

        private Guid GetCurrentUserId()
        {
            // في التطبيق الفعلي، يتم الحصول على ID المستخدم من HttpContext أو JWT
            return Guid.Parse("00000000-0000-0000-0000-000000000001");
        }
    }
}