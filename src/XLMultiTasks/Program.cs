using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using XLMultiTasks.Common;
using XLMultiTasks.Courses;
using XLMultiTasks.Pluralsights;

namespace XLMultiTasks
{
    class Program
    {
        static void Main(string[] args)
        {
            GetCourses();
            return;
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

        static void GetCourses()
        {
            var myCourseFinder = new MyCourseFinder();
            var testFolder = @"E:\2019_learn";
            var myCourses = myCourseFinder.FindAll(testFolder);

            foreach (var myCourse in myCourses)
            {
                Console.WriteLine("{0}\t{1}", myCourse.Title, myCourse.Size.ToFriendlySize());
            }

            var nodeCourses = ConvertToNodes(myCourses);
            var asciiTreeHelper = new AsciiTreeHelper();
            //asciiTreeHelper.MaxPrintDeep = 0;
            asciiTreeHelper.PrintAllDeep();
            var sb = new StringBuilder();
            foreach (var nodeCourse in nodeCourses)
            {
                asciiTreeHelper.ProcessNode(nodeCourse, "", sb, 0);
            }
            
            var json = myCourses.ToJson(true);
            File.WriteAllText("courses.json", json);
            File.WriteAllText("courses_ascii.text", sb.ToString());
            Console.Read();
        }

        private static List<Node> ConvertToNodes(IList<MyCourse> myCourses)
        {
            var nodeCourses = new List<Node>();
            foreach (var myCourse in myCourses)
            {
                var nodeCourse = new Node();
                nodeCourse.Name = myCourse.Title + " (" + myCourse.Size.ToFriendlySize() + ")";
                foreach (var section in myCourse.Sections)
                {
                    var nodeSection = new Node();
                    nodeSection.Name = section.Title;
                    foreach (var item in section.Items)
                    {
                        var nodeItem = new Node();
                        nodeItem.Name = item;
                        nodeSection.Children.Add(nodeItem);
                    }
                    nodeCourse.Children.Add(nodeSection);
                }
                nodeCourses.Add(nodeCourse);
            }
            return nodeCourses;
        }
    }
}
