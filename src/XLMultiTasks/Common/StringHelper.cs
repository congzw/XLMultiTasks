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
    }
}
