using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace XLMultiTasks.Courses
{
    public class MyCourseFinder
    {
        public IList<MyCourse> FindAll(string folder)
        {
            if (!Directory.Exists(folder))
            {
                return new List<MyCourse>();
            }

            var directoryInfo = new DirectoryInfo(folder);
            var fileInfos = directoryInfo.GetFiles("*.mp4", SearchOption.AllDirectories).OrderBy(x => x.FullName).ToList();
            var myFileInfos = GetMyFileInfos(fileInfos);
            var myCourses = Process(myFileInfos);
            return myCourses;
        }

        private IList<MyFileInfo> GetMyFileInfos(IList<FileInfo> fileInfos)
        {
            var myFileInfos = new List<MyFileInfo>();
            foreach (var fileInfo in fileInfos)
            {
                var sectionInfo = fileInfo.Directory;
                if (sectionInfo == null)
                {
                    continue;
                }

                var courseInfo = sectionInfo.Parent;
                if (courseInfo == null)
                {
                    continue;
                }

                //A\B\C.mp4 => C.mp4, B, A
                var myFileInfo = new MyFileInfo();
                myFileInfo.ItemInfo = fileInfo;
                myFileInfo.SectionInfo = sectionInfo;
                myFileInfo.CourseInfo = courseInfo;
                myFileInfos.Add(myFileInfo);
            }
            return myFileInfos;
        }

        private IList<MyCourse> Process(IList<MyFileInfo> myFileInfos)
        {
            var courses = new Dictionary<string, MyCourse>(StringComparer.OrdinalIgnoreCase);
            var sections = new Dictionary<string, MySection>(StringComparer.OrdinalIgnoreCase);

            foreach (var myFileInfo in myFileInfos)
            {
                var courseKey = myFileInfo.CourseInfo.Name;
                if (!courses.ContainsKey(courseKey))
                {
                    var myCourse = new MyCourse();
                    myCourse.Title = myFileInfo.CourseInfo.Name;
                    myCourse.CreateAt = myFileInfo.CourseInfo.CreationTime;
                    myCourse.Location = myFileInfo.CourseInfo.FullName;
                    myCourse.Size = GetDirSize(myFileInfo.CourseInfo.FullName, true);
                    courses.Add(courseKey, myCourse);
                }

                var sectionKey = myFileInfo.CourseInfo.Name + "/" + myFileInfo.SectionInfo.Name;
                if (!sections.ContainsKey(sectionKey))
                {
                    var mySection = new MySection();
                    mySection.Title = myFileInfo.CourseInfo.Name;
                    sections.Add(sectionKey, mySection);
                    //add section to course
                    var course = courses[courseKey];
                    course.Sections.Add(mySection);
                }

                //add item to section
                var section = sections[sectionKey];
                section.Items.Add(myFileInfo.ItemInfo.Name);
            }

            return courses.Values.ToList();
        }

        private static long GetDirSize(string sourceDir, bool recurse)
        {
            long size = 0;
            string[] fileEntries = Directory.GetFiles(sourceDir);

            foreach (string fileName in fileEntries)
            {
                Interlocked.Add(ref size, (new FileInfo(fileName)).Length);
            }

            if (recurse)
            {
                var subEntries = Directory.GetDirectories(sourceDir);
                Parallel.For<long>(0, subEntries.Length, () => 0, (i, loop, subtotal) =>
                    {
                        if ((File.GetAttributes(subEntries[i]) & FileAttributes.ReparsePoint) != FileAttributes.ReparsePoint)
                        {
                            subtotal += GetDirSize(subEntries[i], true);
                            return subtotal;
                        }
                        return 0;
                    },
                    (x) => Interlocked.Add(ref size, x)
                );
            }
            return size;
        }
    }
}