namespace API.Options
{
    public class RateLimitOptions
    {
        public int PermitLimit { get; set; }
        public int WindowInMinutes { get; set; }
        public int QueueLimit { get; set; }
        public int TokensPerPeriod { get; set; }
        public int ReplenishmentPeriodInSeconds { get; set; }
    }
} 