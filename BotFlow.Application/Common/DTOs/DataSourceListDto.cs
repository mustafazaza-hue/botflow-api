namespace BotFlow.Application.Common.DTOs.AIDataSources
{
    public class DataSourceListDto
    {
        public List<AIDataSourceDto> DataSources { get; set; } = new();
        public int TotalCount { get; set; }
        public int Page { get; set; }
        public int PageSize { get; set; }
        public int TotalPages { get; set; }
    }
}