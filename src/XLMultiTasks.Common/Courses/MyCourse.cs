using System;
using System.Collections.Generic;
using System.IO;

namespace XLMultiTasks.Courses
{
    public class MyCourse
    {
        public MyCourse()
        {
            Sections = new List<MySection>();
        }
        public string Title { get; set; }
        public IList<MySection> Sections { get; set; }
        public long? Size { get; set; }
        public DateTime CreateAt { get; set; }
        public string Location { get; set; }
    }

    public class MySection
    {
        public MySection()
        {
            Items = new List<string>();
        }
        public string Title { get; set; }
        public IList<string> Items { get; set; }
    }

    public class MyFileInfo
    {
        public FileInfo ItemInfo { get; set; }
        public DirectoryInfo SectionInfo { get; set; }
        public DirectoryInfo CourseInfo { get; set; }
    }
}
