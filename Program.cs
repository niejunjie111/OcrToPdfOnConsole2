
using log4net;
using OcrToPdfOnConsole2.SqlSugar;
using System.Configuration;
using System.Diagnostics;
using System.Threading;
using System.Timers;
using System.Xml.Linq;

namespace OcrToPdfOnConsole2
{
    internal static class Program
    {
        /// <summary>
        /// 聂俊杰
        /// 2023/7/20
        /// </summary>
        [STAThread]
        static void Main()
        {
            //InitDatabase.RefreshEntity(ConfigurationSettings.AppSettings["directoryPath"]);
            Service.start();
        }


    }
}