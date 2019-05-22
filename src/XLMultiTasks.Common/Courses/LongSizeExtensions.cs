using System;

namespace XLMultiTasks.Courses
{
    public static class LongSizeExtensions
    {
        private static readonly string[] SizeSuffixes = {"B", "KB", "MB", "GB", "TB", "PB", "EB", "ZB", "YB" };
        public static string ToFriendlySize(this long size)
        {
            const string formatTemplate = "{0}{1:0.#} {2}";

            if (size == 0)
            {
                return string.Format(formatTemplate, null, 0, SizeSuffixes[0]);
            }

            var absSize = Math.Abs((double)size);
            var fpPower = Math.Log(absSize, 1000);
            var intPower = (int)fpPower;
            var iUnit = intPower >= SizeSuffixes.Length
                ? SizeSuffixes.Length - 1
                : intPower;
            var normSize = absSize / Math.Pow(1000, iUnit);

            return string.Format(
                formatTemplate,
                size < 0 ? "-" : null, normSize, SizeSuffixes[iUnit]);
        }

        public static string ToFriendlySize(this long? size)
        {
            if (size == null)
            {
                return ToFriendlySize(0);
            }
            return ToFriendlySize(size.Value);
        }
    }
}
