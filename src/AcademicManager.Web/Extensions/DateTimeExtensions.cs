namespace AcademicManager.Web.Extensions
{
    public static class DateTimeExtensions
    {
        public static string ToRelativeTime(this DateTime dateTime)
        {
            var now = DateTime.Now;
            var difference = now - dateTime;

            if (difference.TotalSeconds < 60)
                return "hace un momento";

            if (difference.TotalMinutes < 60)
            {
                int minutes = (int)difference.TotalMinutes;
                return $"hace {minutes} minuto{(minutes != 1 ? "s" : "")}";
            }

            if (difference.TotalHours < 24)
            {
                int hours = (int)difference.TotalHours;
                return $"hace {hours} hora{(hours != 1 ? "s" : "")}";
            }

            if (difference.TotalDays < 7)
            {
                int days = (int)difference.TotalDays;
                return $"hace {days} día{(days != 1 ? "s" : "")}";
            }

            if (difference.TotalDays < 30)
            {
                int weeks = (int)(difference.TotalDays / 7);
                return $"hace {weeks} semana{(weeks != 1 ? "s" : "")}";
            }

            if (difference.TotalDays < 365)
            {
                int months = (int)(difference.TotalDays / 30);
                return $"hace {months} mes{(months != 1 ? "es" : "")}";
            }

            int years = (int)(difference.TotalDays / 365);
            return $"hace {years} año{(years != 1 ? "s" : "")}";
        }

        public static string ToShortDateString(this DateTime? dateTime, string format = "dd/MM/yyyy")
        {
            return dateTime?.ToString(format) ?? "--";
        }

        public static string ToShortTimeString(this DateTime? dateTime, string format = "HH:mm")
        {
            return dateTime?.ToString(format) ?? "--";
        }

        public static string ToShortDateTimeString(this DateTime? dateTime, string format = "dd/MM/yyyy HH:mm")
        {
            return dateTime?.ToString(format) ?? "--";
        }

        public static bool IsBetween(this DateTime date, DateTime startDate, DateTime endDate)
        {
            return date >= startDate && date <= endDate;
        }

        public static DateTime StartOfWeek(this DateTime date, DayOfWeek startDayOfWeek = DayOfWeek.Monday)
        {
            int diff = (7 + (date.DayOfWeek - startDayOfWeek)) % 7;
            return date.AddDays(-1 * diff).Date;
        }

        public static DateTime EndOfWeek(this DateTime date, DayOfWeek startDayOfWeek = DayOfWeek.Monday)
        {
            return date.StartOfWeek(startDayOfWeek).AddDays(6);
        }

        public static DateTime StartOfMonth(this DateTime date)
        {
            return new DateTime(date.Year, date.Month, 1);
        }

        public static DateTime EndOfMonth(this DateTime date)
        {
            return date.StartOfMonth().AddMonths(1).AddDays(-1);
        }
    }
}