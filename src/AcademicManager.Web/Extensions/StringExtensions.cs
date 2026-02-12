namespace AcademicManager.Web.Extensions
{
    public static class StringExtensions
    {
        public static string Truncate(this string value, int maxLength, string suffix = "...")
        {
            if (string.IsNullOrEmpty(value) || value.Length <= maxLength)
                return value;

            return value.Substring(0, maxLength - suffix.Length) + suffix;
        }

        public static string ToTitleCase(this string value)
        {
            if (string.IsNullOrEmpty(value))
                return value;

            var words = value.ToLower().Split(' ', StringSplitOptions.RemoveEmptyEntries);
            for (int i = 0; i < words.Length; i++)
            {
                if (words[i].Length > 0)
                {
                    words[i] = char.ToUpper(words[i][0]) + words[i].Substring(1);
                }
            }

            return string.Join(" ", words);
        }

        public static string ToPlural(this string value, int count)
        {
            if (count == 1)
                return value;

            if (value.EndsWith("y"))
                return value.Substring(0, value.Length - 1) + "ies";
            if (value.EndsWith("s") || value.EndsWith("sh") || value.EndsWith("ch") || value.EndsWith("x") || value.EndsWith("z"))
                return value + "es";
            if (value.EndsWith("f"))
                return value.Substring(0, value.Length - 1) + "ves";
            if (value.EndsWith("fe"))
                return value.Substring(0, value.Length - 2) + "ves";

            return value + "s";
        }

        public static bool IsNullOrEmpty(this string value)
        {
            return string.IsNullOrEmpty(value);
        }

        public static bool IsNullOrWhiteSpace(this string value)
        {
            return string.IsNullOrWhiteSpace(value);
        }

        public static string DefaultIfEmpty(this string value, string defaultValue)
        {
            return string.IsNullOrEmpty(value?.Trim()) ? defaultValue : value;
        }
    }
}