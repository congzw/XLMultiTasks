using XLMultiTasks.Pluralsights;

namespace XLMultiTasks
{
    class Program
    {
        static void Main(string[] args)
        {
            var psHelper = new PSHelper();
            psHelper.Process();
            //Console.Read();
        }
    }
}
