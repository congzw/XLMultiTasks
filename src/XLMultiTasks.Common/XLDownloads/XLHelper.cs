using System;
using XLMultiTasks.Common;

namespace XLMultiTasks.XLDownloads
{
    public class XLHelper
    {
        public bool Init()
        {
            return XL.XL_Init();
        }

        public XLTaskItem StartTask(XLTaskItem item)
        {
            var param = new XL.DownTaskParam
            {
                IsResume = 0,
                szTaskUrl = item.Url,
                szFilename = item.FileName, // "http://whaterver/abc.xzy" => "foo.bar",
                szSavePath = item.SaveTo
            };
            var ptrDownloadTask = XL.XL_CreateTask(param);
            var status = XL.XL_StartTask(ptrDownloadTask);
            item.TaskIntPtr = ptrDownloadTask;


            var messageResult = new MessageResult();
            messageResult.Message = status ? "开始成功":"开始失败";
            messageResult.Data = 0;
            item.Result = messageResult;

            return item;
        }

        public XLTaskItem QueryTask(XLTaskItem item)
        {
            var taskInfo = new XL.DownTaskInfo();
            var success = XL.XL_QueryTaskInfoEx(item.TaskIntPtr, taskInfo);
            if (!success)
            {
                item.Result.Message = "查询状态失败";
                return item;
            }
            var messageResult = new MessageResult();
            messageResult.Message = string.Format("下载进度：{0}%", (int)(taskInfo.fPercent * 100));
            messageResult.Data =  taskInfo.fPercent;
            if (taskInfo.stat == XL.DOWN_TASK_STATUS.TSC_COMPLETE)
            {
                messageResult.Success = true;
            }
            item.Result = messageResult;
            return item;
        }
    }

    public class XLTaskItem
    {
        public string FileName { get; set; }
        public string Url { get; set; }
        public string SaveTo { get; set; }
        public IntPtr TaskIntPtr { get; set; }
        public MessageResult Result { get; set; }
    }
}
