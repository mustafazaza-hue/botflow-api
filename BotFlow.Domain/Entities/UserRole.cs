namespace BotFlow.Domain.Enums
{
    public enum UserRole
    {
        User = 1,
        Admin = 2,
        SuperAdmin = 3
    }
    
    // public enum BotStatus
    // {
    //     Active = 1,
    //     Inactive = 2,
    //     Draft = 3,
    //     Error = 4,
    //     Busy = 5,
    //     Training = 6
    // }
    
    // public enum ConversationStatus
    // {
    //     New = 1,
    //     InProgress = 2,
    //     Resolved = 3,
    //     Escalated = 4,
    //     Closed = 5,
    //     Spam = 6
    // }
    
    public enum PlatformType
    {
        Facebook = 1,
        Instagram = 2,
        WhatsApp = 3,
        Telegram = 4,
        DirectMessage = 5,
        Comment = 6,
        Twitter = 7,
        LinkedIn = 8,
        Email = 9
    }
    
    public enum SubscriptionPlan
    {
        Free = 1,
        Starter = 2,
        Pro = 3,
        Business = 4,
        Enterprise = 5
    }
    
    public enum ReportPeriod
    {
        Daily = 1,
        Weekly = 2,
        Monthly = 3,
        Quarterly = 4,
        Yearly = 5
    }
}