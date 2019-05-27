using System;
using System.Reflection;

namespace XLMultiTasks
{
    public class ExeNameHelper
    {
        public static bool Exist(string value)
        {
            //"foo_blah_value.exe", "_value" => true
            var assemblyName = Assembly.GetExecutingAssembly().CodeBase;
            return assemblyName.IndexOf(value, StringComparison.OrdinalIgnoreCase) != -1;
        }
    }
}