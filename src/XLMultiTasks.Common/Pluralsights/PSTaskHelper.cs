using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading;
using XLMultiTasks.Common;
using XLMultiTasks.XLDownloads;

namespace XLMultiTasks.Pluralsights
{
    public class PSTaskHelper
    {
        public PSTaskHelper()
        {
            ProcessTaskMaxSeconds = 90;
        }
        
        public int ProcessTaskMaxSeconds { get; set; }

        private string lastAjaxTaskUrl = string.Empty;

        public MessageResult ProcessAjaxTask(XLHelper xlHelper, AjaxTaskDto taskDto)
        {
            if (taskDto == null)
            {
                return CreateDelayBadResult("task should not null");
            }
            var xlTaskItem = AjaxTaskDto.ConvertToXLTaskItem(taskDto);
            if (lastAjaxTaskUrl == xlTaskItem.Url)
            {
                var message = string.Format("Escape processed same task: {0}", xlTaskItem.FileName);
                Console.WriteLine();
                Console.WriteLine(message);
                return CreateDelayBadResult(message);
            }

            var filePath = string.Format("{0}\\{1}", xlTaskItem.SaveTo, xlTaskItem.FileName);
            if (File.Exists(filePath))
            {
                var message = string.Format("Escape completed task: {0}", xlTaskItem.FileName);
                Console.WriteLine();
                Console.WriteLine(message);
                return CreateDelayBadResult(message);
            }

            lastAjaxTaskUrl = xlTaskItem.Url;

            return ProcessTask(xlHelper, xlTaskItem);
        }
        
        private MessageResult ProcessTask(XLHelper xlHelper, XLTaskItem xlTaskItem)
        {
            var start = DateTime.Now;
            ConsoleHelper.NewLine();
            var startTask = xlHelper.StartTask(xlTaskItem);

            //·ÀÖ¹Á¬½ÓÌ«¿ì£¬×´Ì¬·µ»ØµÄ´íÎó
            Thread.Sleep(3000);

            var logFilePath = AppDomain.CurrentDomain.BaseDirectory.TrimEnd('\\') + "\\fail.txt";
            TryFixFailFile(logFilePath);
            var processSuccess = false;
            ConsoleHelper.UpdateLine(string.Format("Processing: {0} => ", startTask.FileName));
            int tryConnectCount = 0;
            for (int i = 0; i < ProcessTaskMaxSeconds; i++)
            {
                var queryTask = xlHelper.QueryTask(startTask);
                if (queryTask.Result.Success)
                {
                    processSuccess = true;
                    ConsoleHelper.UpdateLine(string.Format("Processing: {0} => 100%", startTask.FileName));
                    break;
                }

                var completePercent = (float)queryTask.Result.Data;
                ConsoleHelper.UpdateLine(string.Format("Processing: {0} => {1}%", startTask.FileName, (int)(completePercent * 100)));

                if (Math.Abs(completePercent) < 0.0001)
                {
                    tryConnectCount++;
                    ConsoleHelper.UpdateLine(string.Format("Processing: {0} => {1}%  => connect time: {2}", startTask.FileName, (int)(completePercent * 100), tryConnectCount));
                }

                if (tryConnectCount > 30)
                {
                    var filePath = string.Format("{0}\\{1}",
                        startTask.SaveTo,
                        startTask.FileName);
                    var message = string.Format("{0}\n", filePath);
                    if (File.Exists(filePath))
                    {
                        processSuccess = true;
                    }
                    ConsoleHelper.NewLine();
                    Console.WriteLine(message);
                    File.AppendAllText(logFilePath, message);
                    break;
                }

                Thread.Sleep(1000);
            }
            
            var end = DateTime.Now;
            var processSeconds = (int)(end - start).TotalSeconds;
            Console.WriteLine("\n{0} Process Task Use Seconds: {1}", xlTaskItem.FileName, processSeconds);

            var mr = new MessageResult();
            mr.Success = processSuccess;
            mr.Message = string.Format("Processing Complete: {0} => {1}({2})", startTask.FileName, processSuccess, processSeconds);
            mr.Data = processSeconds;
            return mr;
        }

        private void TryFixFailFile(string logFilePath)
        {
            try
            {
                if (!File.Exists(logFilePath))
                {
                    return;
                }
                var failFilePaths = File.ReadAllLines(logFilePath);
                var sb = new StringBuilder();
                foreach (var failFilePath in failFilePaths)
                {
                    var item = failFilePath.Trim();
                    if (!File.Exists(item))
                    {
                        sb.AppendLine(item);
                    }
                    else
                    {
                        Console.WriteLine("! Fix: {0}", item);
                    }
                }
                File.WriteAllText(logFilePath, sb.ToString());
            }
            catch (Exception ex)
            {
                Console.WriteLine("! Fix Ex: {0}", ex.Message);
            }
        }

        private MessageResult CreateDelayBadResult(string message)
        {
            Thread.Sleep(1000 * 10);
            var mr = new MessageResult();
            mr.Message = message;
            return mr;
        }
    }
}