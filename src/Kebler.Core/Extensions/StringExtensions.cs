namespace Kebler.Core.Extensions
{
    public static class StringExtensions
    {
        public static bool IsNullOrEmpty(this string text)
        {
            return string.IsNullOrEmpty(text);
        }

        public static bool IsNotNullOrNotEmpty(this string text)
        {
            return !text.IsNullOrEmpty();
        }

    }
}
