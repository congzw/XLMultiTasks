using System;
using System.Net.Http;
using Microsoft.Owin.Hosting;
using XLMultiTasksApi.Apis;

namespace XLMultiTasksApi
{
    class Program
    {
        static void Main(string[] args)
        {
            var baseAddress = "http://localhost:12345/";
            using (WebApp.Start<Startup>(url: baseAddress))
            {
                var client = new HttpClient();

                var checkUrl = baseAddress + "Api/PS/GetSystemDate";
                var response = client.GetAsync(checkUrl).Result;
                Console.WriteLine("{0} => {1}", checkUrl, response.Content.ReadAsStringAsync().Result);
                Console.WriteLine("-= enter exit to quit! =-");
                //Console.ReadLine();
                
                var exit = CheckIExit();
                while (!exit)
                {
                    exit = CheckIExit(); 
                }
            } 
        }

        static bool CheckIExit()
        {
            var line = Console.ReadLine();
            if ("exit" == line)
            {
                Environment.Exit(0);
            }
            Console.WriteLine(line);
            return false;
        }
    }
}
