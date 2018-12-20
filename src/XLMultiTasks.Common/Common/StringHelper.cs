using System;
using System.Text.RegularExpressions;

namespace XLMultiTasks.Common
{
    public static class StringHelper
    {
        public static string FixEmpty(this string input)
        {
            if (input == null)
            {
                return null;
            }
            return Regex.Replace(input, @"\p{Z}", " ");
        }

        public static bool NbEquals(this string input, string value)
        {
            if (input == null)
            {
                return string.IsNullOrWhiteSpace(value);
            }
            return input.Equals(value, StringComparison.OrdinalIgnoreCase);
        }
    }
}
