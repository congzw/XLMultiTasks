using System.Collections.Generic;
using System.IO;
using System.Linq;
using Newtonsoft.Json;
using XLMultiTasks.Common;

namespace XLMultiTasks.Pluralsights
{
    public class PsCourseHelper
    {
        public IList<PsCourse> CollectCourses(string dirPath)
        {
            var psCourses = new List<PsCourse>();

            var mp4Files = GetMp4Files(dirPath);
            DirectoryInfo lastProcessDirectory = null;
            foreach (var mp4File in mp4Files)
            {
                var fileInfo = new FileInfo(mp4File);
                var directoryInfo = fileInfo.Directory;
                if (directoryInfo == null)
                {
                    continue;
                }

                var parent = directoryInfo.Parent;
                if (parent != null)
                {
                    if (parent == lastProcessDirectory)
                    {
                        continue;
                    }
                    lastProcessDirectory = parent;
                    psCourses.Add(new PsCourse() { CourseTitle = parent.Name });
                }
            } 
            return psCourses;
        }

        public IList<PsCourse> UpdateCourses(string logPath, IList<PsCourse> courses)
        {
            var psCourses = new List<PsCourse>();
            if (File.Exists(logPath))
            {
                var content = File.ReadAllText(logPath);
                psCourses = JsonConvert.DeserializeObject<List<PsCourse>>(content);
            }

            foreach (var psCourse in courses)
            {
                if (!psCourses.Any(x => x.CourseTitle.NbEquals(psCourse.CourseTitle)))
                {
                    psCourses.Add(psCourse);
                }
            }

            psCourses = psCourses.OrderBy(x => x.CourseTitle).ToList();
            var saveContent = JsonConvert.SerializeObject(psCourses);
            File.WriteAllText(logPath, saveContent);
            return psCourses;
        }

        private static List<string> GetMp4Files(string folderPath)
        {
            var list = new List<string>();
            string[] temp = Directory.GetFiles(folderPath, "*.mp4", SearchOption.AllDirectories);
            if (temp.Length > 0)
            {
                list.AddRange(temp);
            }
            return list;
        }


    }

    public class PsCourse
    {
        public string CourseTitle { get; set; }
    }

    public class JsonHelper
    {
        public static void Save(string path, object data)
        {
            var serializeObject = JsonConvert.SerializeObject(data);
            if (!File.Exists(path))
            {
                File.WriteAllText(path, serializeObject);
            }
            else
            {
                File.AppendAllText(path, serializeObject);
            }
        }
    }
}
