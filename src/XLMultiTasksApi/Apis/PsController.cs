using System;
using System.Web.Http;
using XLMultiTasks.Common;
using XLMultiTasks.Pluralsights;
using XLMultiTasks.XLDownloads;

namespace XLMultiTasksApi.Apis
{
    public class PsController : ApiController
    {
        private static readonly PSHelper _psHelper = new PSHelper();
        private static readonly XLHelper _xlHelper = new XLHelper();

        static PsController()
        {
            _xlHelper.Init();
        }

        [HttpGet]
        [Route("Api/PS/GetSystemDate")]
        public DateTime GetSystemDate()
        {
            var dateTime = DateTime.Now;
            Console.WriteLine("GetSystemDate Invoke At: " + dateTime.ToString("yyyy-MM-dd HH:mm:ss"));
            return dateTime;
        }

        [HttpPost]
        [Route("Api/PS/AddTask")]
        public MessageResult Post([FromBody]AjaxTaskDto dto)
        {
            var result = _psHelper.ProcessAjaxTask(_xlHelper, dto);
            return result;
        }
    }
}