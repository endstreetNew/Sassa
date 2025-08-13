namespace Sassa.Services
{
    public class ScheduleOptions
    {
        public string ConnectionString { get; set; } = string.Empty;
        public bool Enabled { get; set; } = true; // Default to enabled
        public int RunAtHour { get; set; } = 4; // Default to 60 minutes
    }
}