namespace ActosAdultosAPI.Helpers
{
    public static class DateHelper
    {
        public static DateTime ArgentinaNow()
        {
            //solo funciona para windows
            var timeZone = TimeZoneInfo.FindSystemTimeZoneById("Argentina Standard Time");

            return TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, timeZone);
        }
    }
}
