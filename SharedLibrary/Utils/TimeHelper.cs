namespace SharedLibrary.Utils
{
    public static class TimeHelper
    {
        private static readonly TimeZoneInfo MalaysiaTimeZone =
            TimeZoneInfo.FindSystemTimeZoneById("Singapore");

        public static DateTime Now()
        {
            return TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, MalaysiaTimeZone);
        }
        public static DateTime ConvertToMYT(DateTime utc)
        {
            return TimeZoneInfo.ConvertTimeFromUtc(utc, MalaysiaTimeZone);
        }
    }
}
