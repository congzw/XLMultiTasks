using System;
using System.IO;
using System.Threading;
using XLMultiTasks.Common;
using XLMultiTasks.XLDownloads;

namespace XLMultiTasks.Pluralsights
{
    public class PSTaskHelper
    {
        public PSTaskHelper()
        {
            ProcessTaskMaxSeconds = 60;
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
            Thread.Sleep(5000);

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
                    processSuccess = false;
                    var message = string.Format("! Fail: {0}\n", startTask.Url);
                    var filePath = string.Format("{0}\\{1}\\{2}",
                        AppDomain.CurrentDomain.BaseDirectory.TrimEnd('\\'),
                        startTask.SaveTo, 
                        startTask.FileName);
                    if (File.Exists(filePath))
                    {
                        processSuccess = true;
                        message = "! File exist: completePercent => " + completePercent + " => " + filePath;
                    }
                    ConsoleHelper.NewLine();
                    Console.WriteLine(message);
                    var logFilePath = AppDomain.CurrentDomain.BaseDirectory.TrimEnd('\\') + "\\fail.txt";
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

        private MessageResult CreateDelayBadResult(string message)
        {
            Thread.Sleep(1000 * 10);
            var mr = new MessageResult();
            mr.Message = message;
            return mr;
        }
    }
}