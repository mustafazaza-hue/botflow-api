namespace BotFlow.Application.Common.DTOs.SuperAdmin
{
    public class SuperAdminUpdateUserRequest
    {
        public string? FirstName { get; set; }
        public string? LastName { get; set; }
        public string? Role { get; set; }
        public string? SubscriptionPlan { get; set; }
        public bool? IsActive { get; set; }
        public DateTime? RenewalDate { get; set; }
        public string? Status { get; set; }
        public string? Email { get; set; }
        public string? CompanyName { get; set; }
    }
}