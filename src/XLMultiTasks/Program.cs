using System;
using System.IO;
using XLMultiTasks.Pluralsights;

namespace XLMultiTasks
{
    class Program
    {
        static void Main(string[] args)
        {
            var psCourseHelper = new PsCourseHelper();
            var dirPath = AppDomain.CurrentDomain.BaseDirectory;
            var directoryInfo = new DirectoryInfo(dirPath);
            if (directoryInfo.Parent == null)
            {
                Console.WriteLine("copy _CollectCourses to the folder.");
                Console.Read();
                return;
            }
            var collectCourses = psCourseHelper.CollectCourses(directoryInfo.Parent.FullName);
            Console.WriteLine("current total files: {0}", collectCourses.Count);
            Console.WriteLine("-------------");

            var logPath = dirPath.TrimEnd('\\') + "\\course.json";
            var updateCourses = psCourseHelper.UpdateCourses(logPath, collectCourses);
            Console.WriteLine("---- updated total: {0} ----",updateCourses.Count);
            int index = 0;
            foreach (var updateCourse in updateCourses)
            {
                Console.WriteLine("{0} => {1}", (++index).ToString("000"), updateCourse.CourseTitle);
            }
            Console.Read();
        }
    }
}
