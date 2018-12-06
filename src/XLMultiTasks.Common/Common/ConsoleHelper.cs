using System;

namespace XLMultiTasks.Common
{
    public static class ConsoleHelper
    {
        public static void UpdateLine(string message)
        {
            Console.Write("\r{0}", message);
        }

        public static void NewLine()
        {
            Console.Write(Environment.NewLine);
        }
    }
}
