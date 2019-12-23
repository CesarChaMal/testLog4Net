using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace testLog4Net
{
    class Program
    {
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger("System Object");

        static void Main(string[] args)
        {
            //AppLog.Init();
            log.Info(String.Format("hi! started in mode:{0}", "DEV"));
            AppLog.Write("Starting main thread", AppLog.LogMessageType.Debug);
        }
    }
}
