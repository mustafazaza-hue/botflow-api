namespace BotFlow.Application.Common.DTOs.AIDataSources
{
    public class AIDataSourceDto
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Type { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string? FileType { get; set; }
        public long? FileSize { get; set; }
        public string? FormattedFileSize { get; set; }
        public int QueryCount { get; set; }
        public int DocumentCount { get; set; }
        public string? Url { get; set; }
        public string? ApiEndpoint { get; set; }
        public string? DatabaseType { get; set; }
        public string? ErrorMessage { get; set; }
        public int ProgressPercentage { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
        public DateTime? LastProcessedAt { get; set; }
        public string UserName { get; set; } = string.Empty;
        public string UserEmail { get; set; } = string.Empty;
        
        // UI Properties
        public string Icon { get; set; } = string.Empty;
        public string Color { get; set; } = string.Empty;
        public string StatusColor { get; set; } = string.Empty;
        public string StatusIcon { get; set; } = string.Empty;
    }
}