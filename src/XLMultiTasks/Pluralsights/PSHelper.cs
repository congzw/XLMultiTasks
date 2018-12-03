using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using XLMultiTasks.XLDownloads;

namespace XLMultiTasks.Pluralsights
{
    public class PSHelper
    {
        public void Process()
        {
            var links = ParseFileLinks("log.txt");
            var xlTaskItems = CreateXlTaskQueues(links);
            //Download(xlTaskItems);
            var xlHelper = new XLHelper();
            xlHelper.Init();
            ProcessQueues(xlHelper, xlTaskItems);
        }

        public Queue<XLTaskItem> CreateXlTaskQueues(IList<PluralsightFileLink> fileLinks)
        {
            var xlTaskItems = new Queue<XLTaskItem>();
            foreach (var fileLink in fileLinks)
            {
                var xlTaskItem = new XLTaskItem();
                var fileName = Path.GetFileName(fileLink.FixSaveFilePath);
                var folderName = Path.GetDirectoryName(fileLink.FixSaveFilePath);
                xlTaskItem.FileName = fileName;
                xlTaskItem.SaveTo = AppDomain.CurrentDomain.BaseDirectory.TrimEnd('\\') + "\\" + folderName;
                xlTaskItem.Url = fileLink.Link;
                xlTaskItems.Enqueue(xlTaskItem);
            }
            return xlTaskItems;
        }
        
        public void ProcessQueues(XLHelper xlHelper, Queue<XLTaskItem> taskItems)
        {
            int totalCount = taskItems.Count;
            int completeCount = 0;
            while (true)
            {
                if (taskItems.Count == 0)
                {
                    break;
                }
                var xlTaskItem = taskItems.Dequeue();
                var startTask = xlHelper.StartTask(xlTaskItem);
                Console.WriteLine("Processing: {0} => ", startTask.FileName);

                var maxSeconds = 10*60;
                for (int i = 0; i < maxSeconds; i++)
                {
                    var queryTask = xlHelper.QueryTask(startTask);
                    if (queryTask.Result.Success)
                    {
                        completeCount++;
                        Console.WriteLine("Complete => {0} / {1}", completeCount, totalCount);
                        break;
                    }
                    Console.Write("{0}%. ", (int)((float)(queryTask.Result.Data) * 100));
                    Thread.Sleep(1000);
                }
            }
        }
        
        public IList<PluralsightFileLink> ParseFileLinks(string logFileName)
        {
            var fileLinks = new List<PluralsightFileLink>();
            var rawAllLines = File.ReadAllLines(logFileName);
            var readAllLines = new List<string>();
            foreach (var rawLine in rawAllLines)
            {
                readAllLines.Add(TrimLine(rawLine));
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
                    var nameLine = readAllLines[i + 1];
                    fileLink.FixSaveFilePath = nameLine.Replace("/", "\\");
                    fileLink.Link = readAllLines[i + 2];
                    fileLinks.Add(fileLink);
                }
            }
            return fileLinks;
        }

        private string TrimLine(string line)
        {
            var replace = PluralsightFileLink.ReplaceSplit;
            var strings = line.Split(new[] {replace}, StringSplitOptions.RemoveEmptyEntries);
            return strings.LastOrDefault();
        }
    }
}