namespace API.Models
{
    public class RefreshToken
    {
        public string Token { get; set; } = string.Empty;
        public DateTime ExpiryDate { get; set; }
        public string UserId { get; set; } = string.Empty;
        public bool IsActive { get; set; } = true;
        
        private bool? _isExpired;
        public bool IsExpired
        {
            get
            {
                _isExpired ??= DateTime.UtcNow >= ExpiryDate;
                return _isExpired.Value;
            }
        }
        
        public bool IsValid => !IsExpired && IsActive;

        public virtual AppUser User { get; set; } = null!;
    }
} 