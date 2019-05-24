using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
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
            //Console.WriteLine(PSTaskHelper.GuessCourseName(@"d:\TheCourseName\b\c.mp4"));
            //Console.WriteLine(PSTaskHelper.GuessCourseName(@"d:\a\TheCourseName\c\d.mp4"));
            //Console.WriteLine(PSTaskHelper.GuessCourseName(@"d:\a\x\TheCourseName\c\d.mp4"));
            //Console.Read();
            //return;
            var courseFolder = AppDomain.CurrentDomain.BaseDirectory;
            var courseNames = new List<string>();
            var dbFilePath = "completed_courses.json";
            if (File.Exists(dbFilePath))
            {
                courseNames = JsonHelper.Read(dbFilePath, courseNames);
            }
            
            var currentCourses = GetCourseNames(courseFolder);
            foreach (var course in currentCourses)
            {
                if (!courseNames.Contains(course, StringComparer.OrdinalIgnoreCase))
                {
                    courseNames.Add(course);
                    Console.WriteLine("add new course: " + course);
                }
            }

            Console.WriteLine("****************");
            Console.WriteLine("**** total courses: {0} ****", courseNames.Count);
            //foreach (var course in courseNames)
            //{
            //    Console.WriteLine(course);
            //}
            Console.WriteLine("****************");

            JsonHelper.Save("completed_courses.json", courseNames, true, false);

            //UpdateCourses();
            Console.Read();
        }

        private static void UpdateCourses()
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
            Console.WriteLine("---- updated total: {0} ----", updateCourses.Count);
            int index = 0;
            foreach (var updateCourse in updateCourses)
            {
                Console.WriteLine("{0} => {1}", (++index).ToString("000"), updateCourse.CourseTitle);
            }
        }

        static IList<string> GetCourseNames(string courseFolder)
        {
            var myCourseFinder = new MyCourseFinder();
            var myCourses = myCourseFinder.FindAll(courseFolder).Select(x => x.Title).ToList();
            return myCourses;
        }

        static void GetCourses(string courseFolder)
        {
            var myCourseFinder = new MyCourseFinder();
            var myCourses = myCourseFinder.FindAll(courseFolder);

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
