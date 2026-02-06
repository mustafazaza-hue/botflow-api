namespace BotFlow.Application.Common.DTOs.AIDataSources
{
    public class ConnectUrlRequest
    {
        public string Name { get; set; } = string.Empty;
        public string Url { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public Guid? UserId { get; set; }
    }
}