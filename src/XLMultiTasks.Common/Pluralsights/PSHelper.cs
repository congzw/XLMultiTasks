using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using XLMultiTasks.Common;
using XLMultiTasks.XLDownloads;

namespace XLMultiTasks.Pluralsights
{
    public partial class PSHelper
    {
        public PSHelper()
        {
            NextTaskWaitSeconds = 60;
        }

        public int NextTaskWaitSeconds { get; set; }

        public void Process()
        {
            if (!File.Exists("log.txt"))
            {
                Console.WriteLine("log.txt file not exist!");
                Console.Read();
                return;
            }
            var links = ParseFileLinks("log.txt");
            var xlTaskItems = CreateXlTaskQueues(links);
            //Download(xlTaskItems);
            var xlHelper = new XLHelper();
            xlHelper.Init();
            ProcessQueues(xlHelper, xlTaskItems);

            Console.WriteLine("work complete!");
            MyCommonHelper.TryChangeFileName("log.txt", string.Format("log_{0}.txt", DateTime.Now.ToString("yyyyMMdd-HHmmss")));
        }

        public Queue<XLTaskItem> CreateXlTaskQueues(IList<PluralsightFileLink> fileLinks)
        {
            var xlTaskItems = new Queue<XLTaskItem>();
            foreach (var fileLink in fileLinks)
            {
                var xlTaskItem = ConvertToXLTaskItem(fileLink);
                xlTaskItems.Enqueue(xlTaskItem);
            }
            return xlTaskItems;
        }
        
        public void ProcessQueues(XLHelper xlHelper, Queue<XLTaskItem> taskItems)
        {
            int totalCount = taskItems.Count;
            int completeCount = 0;
            string lastProcessUrl = string.Empty;
            while (true)
            {
                if (taskItems.Count == 0)
                {
                    break;
                }
                var xlTaskItem = taskItems.Dequeue();
                if (lastProcessUrl == xlTaskItem.Url)
                {
                    completeCount++;
                    Console.WriteLine("Escape processed same task: {0} => {1} / {2}", xlTaskItem.FileName, completeCount, totalCount);
                    continue;
                }
                var filePath = string.Format("{0}\\{1}", xlTaskItem.SaveTo, xlTaskItem.FileName);
                if (File.Exists(filePath))
                {
                    completeCount++;
                    Console.WriteLine("Escape completed task: {0} => {1} / {2}", xlTaskItem.FileName, completeCount, totalCount);
                    continue;
                }

                lastProcessUrl = xlTaskItem.Url;
                var startTask = xlHelper.StartTask(xlTaskItem);
                
                ConsoleHelper.NewLine();
                ConsoleHelper.UpdateLine(string.Format("Processing: {0} => ", startTask.FileName));

                int taskFailCount = 0;
                for (int i = 0; i < NextTaskWaitSeconds; i++)
                {
                    var queryTask = xlHelper.QueryTask(startTask);
                    if (queryTask.Result.Success)
                    {
                        completeCount++;
                        //Console.WriteLine("Complete => {0} / {1}", completeCount, totalCount);
                        ConsoleHelper.UpdateLine(string.Format("Processing: {0} => Complete {1} / {2}", startTask.FileName, completeCount, totalCount));
                        break;
                    }

                    var completePercent = (float)queryTask.Result.Data;
                    if (Math.Abs(completePercent) < 0.01)
                    {
                        taskFailCount++;
                    }
                    if (taskFailCount > 10)
                    {
                        Console.WriteLine("fail: {0} {1} {2}", startTask.SaveTo, startTask.FileName, startTask.Url);
                        taskFailCount = 0;
                        continue;
                    }

                    //Console.Write("{0}%. ", (int)((float)(queryTask.Result.Data) * 100));
                    ConsoleHelper.UpdateLine(string.Format("Processing: {0} => {1}%", startTask.FileName, (int)(completePercent * 100)));
                    Thread.Sleep(1000 * 2);
                }
            }
        }
        
        public IList<PluralsightFileLink> ParseFileLinks(string logFileName)
        {
            var fileLinks = new List<PluralsightFileLink>();
            var rawAllLines = File.ReadAllLines(logFileName);
            var readAllLines = FixLines(rawAllLines);
            if (readAllLines.Count == 0)
            {
                Console.WriteLine("no record find!");
                Console.Read();
            }

            for (int i = 0; i < readAllLines.Count; i++)
            {
                var readAllLine = readAllLines[i];
                if (string.IsNullOrWhiteSpace(readAllLine))
                {
                    break;
                }
                if (readAllLine == PluralsightFileLink.EmptyLine)
                {
                    var fileLink = new PluralsightFileLink();
                    //Pluralsight/Java Fundamentals The Java Language/02 - Introduction and Setting up Your Environment/01 - Introduction.mp4
                    var nameLine = readAllLines[i + 1].Trim().FixEmpty();
                    fileLink.FixSaveFilePath = nameLine.Replace("/", "\\").FixEmpty();
                    fileLink.Link = readAllLines[i + 2].Trim().FixEmpty();
                    fileLinks.Add(fileLink);
                }
            }
            return fileLinks;
        }
        
        public static XLTaskItem ConvertToXLTaskItem(PluralsightFileLink fileLink)
        {
            var xlTaskItem = new XLTaskItem();
            var fileName = Path.GetFileName(fileLink.FixSaveFilePath);
            var folderName = Path.GetDirectoryName(fileLink.FixSaveFilePath);
            xlTaskItem.FileName = fileName;
            xlTaskItem.SaveTo = AppDomain.CurrentDomain.BaseDirectory.TrimEnd('\\') + "\\" + folderName;
            xlTaskItem.Url = fileLink.Link;
            return xlTaskItem;
        }

        private string TrimLine(string line)
        {
            var replace = PluralsightFileLink.ReplaceSplit;
            var strings = line.Split(new[] {replace}, StringSplitOptions.RemoveEmptyEntries);
            return strings.LastOrDefault();
        }
        private IList<string> FixLines(IEnumerable<string> rawAllLines)
        {
            var readAllLines = new List<string>();
            foreach (var rawLine in rawAllLines)
            {
                if (!rawLine.Contains(PluralsightFileLink.LogLineContainMark))
                {
                    continue;
                }
                readAllLines.Add(TrimLine(rawLine));
            }
            return readAllLines;
        }
    }

    //for ajax call
    public partial class PSHelper
    {
        private string lastAjaxTaskProcessUrl = string.Empty;
        public MessageResult ProcessAjaxTask(XLHelper xlHelper, AjaxTaskDto taskDto)
        {
            var start = DateTime.Now;
            var mr = new MessageResult();
            if (taskDto == null)
            {
                var message = "参数不能为空";
                Console.WriteLine(message);
                mr.Message = message;
                return mr;
            }

            try
            {
                var xlTaskItem = AjaxTaskDto.ConvertToXLTaskItem(taskDto);
                if (lastAjaxTaskProcessUrl == xlTaskItem.Url)
                {
                    var message = string.Format("\nEscape processed same task: {0}\n", xlTaskItem.FileName);
                    Console.Write(message);
                    mr.Message = message;
                    return mr;
                }
                var filePath = string.Format("{0}\\{1}", xlTaskItem.SaveTo, xlTaskItem.FileName);
                if (File.Exists(filePath))
                {
                    var message = string.Format("\nEscape completed task: {0}\n", xlTaskItem.FileName);
                    Console.Write(message);
                    mr.Message = message;
                    return mr;
                }

                lastAjaxTaskProcessUrl = xlTaskItem.Url;
                ConsoleHelper.NewLine();
                var startTask = xlHelper.StartTask(xlTaskItem);

                ConsoleHelper.UpdateLine(string.Format("Processing: {0} => ", startTask.FileName));

                int taskFailCount = 0;
                for (int i = 0; i < NextTaskWaitSeconds; i++)
                {
                    var queryTask = xlHelper.QueryTask(startTask);
                    if (queryTask.Result.Success)
                    {
                        ConsoleHelper.UpdateLine(string.Format("Processing: {0} => 100%", startTask.FileName));
                        break;
                    }

                    var completePercent = (float)queryTask.Result.Data;
                    if (Math.Abs(completePercent) < 0.0001)
                    {
                        taskFailCount++;
                    }
                    if (taskFailCount > 20)
                    {
                        var logFilePath = AppDomain.CurrentDomain.BaseDirectory.TrimEnd('\\') + "\\fail.txt";
                        ConsoleHelper.NewLine();
                        var message = string.Format("[!Fail] {0}\n", filePath);
                        //try fix ok file path
                        var allFails = File.ReadAllLines(logFilePath);
                        var fixMessage = message;
                        foreach (var line in allFails)
                        {
                            var filePathInLine = line.Replace("[!Fail] ", "").Replace("\n", "");
                            if (File.Exists(filePathInLine))
                            {
                                fixMessage = "[Fixed]" + message;
                                var fixContent = File.ReadAllText(logFilePath).Replace(message, fixMessage);
                                File.WriteAllText(filePath, fixContent);
                                break;
                            }
                        }
                        Console.WriteLine(fixMessage);
                        File.AppendAllText(logFilePath, message);
                        break;
                    }
                    ConsoleHelper.UpdateLine(string.Format("Processing: {0} => {1}%", startTask.FileName, (int)(completePercent * 100)));
                    Thread.Sleep(1000 * 2);
                }

                mr.Success = true;
                mr.Message = string.Format("Processing Complete: {0} => ", startTask.FileName);

                var end = DateTime.Now;
                var downloadSeconds = (int)(end - start).TotalSeconds;
                var mockWatchSeconds = downloadSeconds * 1;
                Console.WriteLine("\n{0} Mock Watch Seconds: {1}", xlTaskItem.FileName, mockWatchSeconds);
                mr.Data = mockWatchSeconds;
            }
            catch (Exception ex)
            {
                var message = ex.Message;
                Console.WriteLine(message);
                mr.Message = message;
            }
            return mr;
        }
    }
    
    public class AjaxTaskDto
    {
        public string SaveFilePath { get; set; }
        public string Link { get; set; }

        public static XLTaskItem ConvertToXLTaskItem(AjaxTaskDto taskDto)
        {
            //fix encode problems
            taskDto.SaveFilePath = taskDto.SaveFilePath.Trim().FixEmpty();

            var xlTaskItem = new XLTaskItem();
            var fileName = Path.GetFileName(taskDto.SaveFilePath);
            var folderName = Path.GetDirectoryName(taskDto.SaveFilePath);
            xlTaskItem.FileName = fileName;
            xlTaskItem.SaveTo = AppDomain.CurrentDomain.BaseDirectory.TrimEnd('\\') + "\\" + folderName;
            xlTaskItem.Url = taskDto.Link;
            return xlTaskItem;
        }
    }
}