namespace Kebler.QBittorrent.Extensions
{
    internal static class BoolExtensions
    {
        private const string TrueString = "true";
        private const string FalseString = "false";

        public static string ToLowerString(this bool value) => value ? TrueString : FalseString;
    }
}
