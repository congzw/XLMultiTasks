using System;
using System.Collections.Generic;
using System.IO;
using System.Security.AccessControl;
using System.Text;

namespace XLMultiTasks.Common
{
    public class MyCommonHelper
    {
        /// <summary>  
        /// 复制文件夹  
        /// </summary>  
        /// <param name="sourceFolder">待复制的文件夹</param>  
        /// <param name="destFolder">复制到的文件夹</param>  
        public static void CopyFolder(string sourceFolder, string destFolder)
        {
            if (!Directory.Exists(destFolder))
            {
                Directory.CreateDirectory(destFolder);
            }
            string[] files = Directory.GetFiles(sourceFolder);
            foreach (string file in files)
            {
                string name = Path.GetFileName(file);
                string dest = Path.Combine(destFolder, name);
                File.Copy(file, dest, true);
            }
            string[] folders = Directory.GetDirectories(sourceFolder);
            foreach (string folder in folders)
            {
                string name = Path.GetFileName(folder);
                string dest = Path.Combine(destFolder, name);
                CopyFolder(folder, dest);
            }
        }

        public static void CopyFile(string sourceFile, string destFile)
        {
            File.Copy(sourceFile, destFile, true);
        }

        public static void TryCreateFolder(string folderPath)
        {
            if (!Directory.Exists(folderPath))
            {
                Directory.CreateDirectory(folderPath);
            }
        }

        public static string MakeSubFolderPath(string folderPath, string subFolderName)
        {
            string tempFolder = folderPath.TrimEnd('\\');
            string newFolder = string.Format("{0}\\{1}", tempFolder, subFolderName);
            return newFolder;
        }

        public static bool TryChangeFileName(string sourceFileName, string destFileName)
        {
            if (!File.Exists(sourceFileName))
            {
                return false;
            }
            try
            {
                File.Move(sourceFileName, destFileName);
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                return false;
            }
        }

        /// <summary>     
        /// C# 删除文件夹        
        /// </summary>     
        /// <param name="dir">删除的文件夹，全路径格式</param>     
        public static void DeleteFolder(string dir)
        {
            // 循环文件夹里面的内容     
            foreach (string f in Directory.GetFileSystemEntries(dir))
            {
                // 如果是文件存在     
                if (File.Exists(f))
                {
                    FileInfo fi = new FileInfo(f);
                    if (fi.Attributes.ToString().IndexOf("Readonly") != 1)
                    {
                        fi.Attributes = FileAttributes.Normal;
                    }
                    // 直接删除其中的文件     
                    File.Delete(f);
                }
                else
                {
                    // 如果是文件夹存在     
                    // 递归删除子文件夹     
                    DeleteFolder(f);
                }
            }
            // 删除已空文件夹     
            Directory.Delete(dir);
        }

        /// <summary>
        /// 删除文件
        /// </summary>
        /// <param name="file"></param>
        public static void DeleteFile(string file)
        {
            // 如果是文件存在     
            if (File.Exists(file))
            {
                FileInfo fi = new FileInfo(file);
                if (fi.Attributes.ToString().IndexOf("Readonly") != 1)
                {
                    fi.Attributes = FileAttributes.Normal;
                }
                // 直接删除其中的文件     
                File.Delete(file);
            }
        }

        /// <summary>
        /// 查找所有文件
        /// </summary>
        /// <param name="folderPath"></param>
        /// <param name="searchPattern"></param>
        /// <param name="searchOption"></param>
        /// <returns></returns>
        public static List<string> GetFiles(string folderPath, string searchPattern, SearchOption searchOption)
        {
            List<string> list = new List<string>();
            string[] temp = Directory.GetFiles(folderPath, searchPattern, searchOption);
            if (temp.Length > 0)
            {
                list.AddRange(temp);
            }
            return list;
        }

        /// <summary>
        /// 查找所有文件夹
        /// </summary>
        /// <param name="folderPath"></param>
        /// <param name="searchPattern"></param>
        /// <param name="searchOption"></param>
        /// <returns></returns>
        public static List<string> GetDirectories(string folderPath, string searchPattern, SearchOption searchOption)
        {
            List<string> list = new List<string>();
            string[] temp = Directory.GetDirectories(folderPath, searchPattern, searchOption);
            if (temp != null && temp.Length > 0)
            {
                list.AddRange(temp);
            }
            return list;
        }

        private static object _logLock = new object();
        private static object _messageLock = new object();

        //记录异常日志
        public static void LogException(Exception ex)
        {
            if (ex != null)
            {
                try
                {
                    //if (ex.InnerException != null)
                    //{
                    //    LogMessage("exceptionlog", "exception", FormatExceptionMessage(ex.InnerException));
                    //}
                    LogMessage("exceptionlog", "exception", FormatExceptionMessage(ex));
                }
                catch
                {
                    //LogMessage("exceptionlog", "exception", ex.Message);
                }
            }
        }

        //记录日志
        public static void LogMessage(string dirName, string fileNamePre, string message)
        {
            //log to txt file
            lock (_logLock)
            {
                try
                {
                    if (string.IsNullOrEmpty(message))
                    {
                        return;
                    }
                    if (!Directory.Exists(dirName))
                    {
                        Directory.CreateDirectory(dirName);
                    }
                    using (StreamWriter sw = File.AppendText(string.Format("{0}\\{1}_{2}.txt", dirName, fileNamePre, DateTime.Now.ToString("yyyyMMdd_HH"))))
                    {
                        //sw.WriteLine(String.Format("----------------ZONEKEY {0}----------------", fileNamePre.ToUpper()));
                        //sw.WriteLine(DateTime.Now.ToString());
                        sw.WriteLine(message);
                        sw.Close();
                    }
                }
                catch (Exception ex)
                {
                    throw ex;
                }

            }
        }

        //获取异常基本信息
        public static string FormatExceptionMessage(Exception ex)
        {
            lock (_messageLock)
            {
                return string.Format(
@"异常模块：{0}
异常类名：{1}
异常方法：{2}
异常描述：{3}
异常来源：{4}
诊断信息：{5}
"
, ex.TargetSite.Module.Name
, ex.TargetSite.ReflectedType.Name
, ex.TargetSite.Name
, ex.Message
, ex.Source
, ex.StackTrace);
            }
        }


        public static string ReadAllText(string path)
        {
            return File.ReadAllText(path);
        }

        //没有目录，尝试自动创建
        //并且递归修改现有写权限
        public static bool TrySaveFileWithAddAccessRule(string filePath, string content, Encoding encoding, out string message)
        {
            message = "保存失败";
            bool result = false;
            try
            {
                string dirPath = Path.GetDirectoryName(filePath);
                //string dirPath = Path.GetFileNameWithoutExtension(filePath);
                if (!Directory.Exists(dirPath))
                {
                    Directory.CreateDirectory(dirPath);
                }
                //修改访问权限
                DirectoryInfo folder = new DirectoryInfo(dirPath);
                DirectorySecurity dirsecurity = folder.GetAccessControl(AccessControlSections.All);
                dirsecurity.AddAccessRule(new FileSystemAccessRule("Everyone", FileSystemRights.FullControl, AccessControlType.Allow));
                folder.SetAccessControl(dirsecurity);

                if (encoding == null)
                {
                    File.WriteAllText(filePath,content);
                }
                else
                {
                    File.WriteAllText(filePath, content, encoding);
                }

                message = "保存成功";
                result = true;
            }
            catch (Exception ex)
            {
                message = "保存失败。\n" + ex.Message;
            }
            return result;
        }
        
        //没有目录，尝试自动创建
        public static bool TrySaveFile(string filePath, string content, Encoding encoding, out string message)
        {
            message = "保存失败";
            bool result = false;
            try
            {
                string dirPath = Path.GetDirectoryName(filePath);
                //string dirPath = Path.GetFileNameWithoutExtension(filePath);
                if (!Directory.Exists(dirPath))
                {
                    Directory.CreateDirectory(dirPath);
                }

                File.WriteAllText(filePath, content, encoding);
                message = "保存成功";
                result = true;
            }
            catch (Exception ex)
            {
                message = "保存失败。\n" + ex.Message;
            }
            return result;
        }

    }
}
