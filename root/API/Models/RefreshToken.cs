namespace API.Models
{
    public class RefreshToken
    {
        public string Token { get; set; } = string.Empty;
        public DateTime ExpiryDate { get; set; }
        public bool IsExpired => DateTime.UtcNow >= ExpiryDate;
        public string UserId { get; set; } = string.Empty;
        public bool IsActive { get; set; } = true;
    }
} 