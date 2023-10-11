using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OcrToPdfOnConsole2.TableModels
{
    public class DataModel
    {
        /// <summary>
        /// PDF 文件名
        /// </summary>
        public string? PdfFileName { get; set; }
        /// <summary>
        /// 登記號碼
        public string? RegistrationNumber { get; set; }
    }



    public enum state
    {
        waiting = 0, reading = 1, over = 2
    }


    public class Result
    {
        public bool flag { get; set; }
        public string text { get; set; }
    }

    public class Result2
    {
        public bool flag { get; set; }
        public int count { get; set; }
    }



}
