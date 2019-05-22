using Newtonsoft.Json;

namespace XLMultiTasks.Common
{
    public static class JsonExtensions
    {
        public static string ToJson(this object value, bool indented)
        {
            return JsonConvert.SerializeObject(value, indented == true ? Formatting.Indented : Formatting.None);
        }
    }
}
