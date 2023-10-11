using System;
using System.Collections.Generic;
using System.Configuration;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Timers;
using Newtonsoft.Json.Linq;
using O2S;
using PaddleOCRSharp;
using SqlSugar;
using OcrToPdfOnConsole2.TableModels;
using System.Drawing.Imaging;
using System.Drawing.Drawing2D;
using System.IO.Compression;
using PdfiumViewer;
using iTextSharp.text.pdf;
using iTextSharp.text;
using System.Reflection.Metadata;
using iTextSharp.text.pdf.parser;
using System.Xml.Linq;
using System.Runtime.Intrinsics.X86;
using System.Security.Policy;
using O2S.Components.PDFRender4NET;
using log4net;

namespace OcrToPdfOnConsole2
{
    public class Service
    {
        private static readonly ILog log = LogManager.GetLogger(typeof(Program));
        public static SqlSugarClient db = new SqlSugarClient(new ConnectionConfig()
        {
            DbType = DbType.SqlServer,
            ConnectionString = ConfigurationSettings.AppSettings["SqlConnection"],
            IsAutoCloseConnection = true,
            InitKeyType = InitKeyType.Attribute
        },
                       db =>
                       {
                           //单例参数配置，所有上下文生效
                           db.Aop.OnLogExecuting = (sql, pars) =>
                           {
                           };
                       });

        //文件所在路径
        public static string FilePath { get; set; }
        //文件名
        public static string FileName { get; set; }
        //文件总路径
        public static string FillFullPath { get; set; }
        //图片路径
        public static string ImagesPath { get; set; }
        //第二张图片路径
        public static string ImagesPath2 { get; set; }

        public static string Source_Path = ConfigurationManager.AppSettings["Source_path"];
        public static string Processed_path = ConfigurationManager.AppSettings["Processed_path"];
        public static string Merged_path = ConfigurationManager.AppSettings["Merged_path"];
        public static string RemainingPage_Img = ConfigurationManager.AppSettings["RemainingPage_Img"];
        public static string Number_Img = ConfigurationManager.AppSettings["Number_Img"];
        public static string FirstPage_Img = ConfigurationManager.AppSettings["FirstPage_Img"];
        public static int page = Convert.ToInt32(ConfigurationManager.AppSettings["page"]);
        public static int dim_score = Convert.ToInt32(ConfigurationManager.AppSettings["dim_score"]);

        public static System.Timers.Timer t;
        /// <summary>
        /// 开始
        /// </summary>
        public static void start()
        {
            log.Debug("start");
            t = new System.Timers.Timer();
            t.Elapsed += new ElapsedEventHandler(Run);
            t.Interval = 1000;//第一次立即执行
            t.Enabled = true;
            Console.Read();
        }



        /// <summary>
        /// Run
        /// </summary>
        /// <param name="source"></param>
        /// <param name="e"></param>
        public static void Run(object source, ElapsedEventArgs e)
        {
            if (t.Interval == 1000)
            {
                int second = Convert.ToInt32(ConfigurationManager.AppSettings["second"]);
                t.Interval = 1000 * second;
            }
            try
            {
                t.Enabled = false;
                Console.WriteLine("Start getting PDF");
                if (!Directory.Exists(Source_Path))
                {
                    Console.WriteLine(Source_Path + " This folder does not exist. Please make sure that the source file is in this folde");
                }
                DirectoryInfo di = new DirectoryInfo(Source_Path);
                //找到该目录下的文件 
                FileInfo[] files = di.GetFiles();
                if (files.Length <= 0)
                {
                    Console.WriteLine("Folder is empty");
                }
                else
                {
                    int count = 0;
                    for (var i = 0; i < files.Length; i++)
                    {
                        if (files[i].Extension != ".pdf")
                        {
                            continue;
                        }
                        else
                        {
                            count++;
                            Console.WriteLine("datetime：" + DateTime.Now + " Start working on number " + count);
                            string filePath = Source_Path + @"\" + files[i].Name;
                            Spire.Pdf.PdfDocument pdf = new Spire.Pdf.PdfDocument();
                            pdf.LoadFromFile(filePath);
                            ImagesPath2 = FirstPage_Img + @"\";
                            if (!Directory.Exists(ImagesPath2))
                                Directory.CreateDirectory(ImagesPath2);
                            ImagesPath = Number_Img + @"\";
                            if (!Directory.Exists(ImagesPath))
                                Directory.CreateDirectory(ImagesPath);
                            if (!Directory.Exists(Merged_path))
                                Directory.CreateDirectory(Merged_path);
                            List<ModelList> OCR_List;
                            var str_img = string.Empty;
                            var text = string.Empty;
                            //判断pdf是否是多页
                            if (pdf.Pages.Count > 1)
                            {
                                int sum_page = pdf.Pages.Count % page;//取余
                                if (sum_page == 0)
                                {
                                    //循环页数
                                    PdfReader reader = new PdfReader(filePath);
                                    var str=string.Empty;
                                    bool id = false;//第一页，第五页，第九页...
                                    List<DataModel> DMS = new List<DataModel>();
                                    for (int j = 1; j <= pdf.Pages.Count; j++)
                                    {
                                        DataModel md = new DataModel();
                                        var filesNmae = files[i].Name.Split('.').Length==2? files[i].Name.Split('.')[0] + "-Split" + j: files[i].Name.Substring(0, files[i].Name.Length - 4) + "-Split" + j;
                                        iTextSharp.text.Document doc = new iTextSharp.text.Document(reader.GetPageSizeWithRotation(j));
                                        str = Source_Path + "\\" + filesNmae + ".pdf";
                                        md.PdfFileName= str;
                                        DMS.Add(md);
                                        PdfCopy copy = new PdfCopy(doc, new FileStream(str, FileMode.Create));
                                        doc.Open();
                                        copy.AddPage(copy.GetImportedPage(reader, j));
                                        doc.Close();
                                        //取每份第一页
                                        if (id==true || j==1)
                                        {
                                            //存图片
                                            string _path = filesNmae + @".jpg";
                                            string _png_path = ImagesPath2 + _path.Replace("/", "").Replace(@"\", "");
                                            File.Delete(_png_path);
                                            Function.PdfToImage2(str, ImagesPath2, filesNmae, @".jpg", ImageFormat.Jpeg, j, j, Function.Definition.Eight);
                                            //开始识别
                                            OCR_List = O2S.ConvertImg(str, ImagesPath, 2, true);
                                            str_img = Function.DocumentToBase64Str(OCR_List[0].imgPath);
                                            foreach (var item in OCR_List)
                                            {
                                                Result imgText = Function.CreateOCRParameter(item.imgPath);
                                                text = imgText.text;
                                                //写入数据库（先写数据再移动文件）
                                                var result = db.Insertable(new TableModels.OCRLogs()
                                                {
                                                    FileName = text.Replace("/", "").Replace(@"\", ""),
                                                    FilePath = Merged_path + "\\" + text.Replace("/", "").Replace(@"\", "") + @".pdf",
                                                    TrustBookNo = imgText.text.Replace("/", "").Replace(@"\", ""),
                                                    OCRTime = DateTime.Now,
                                                    Susscess = imgText.flag,
                                                    Message = "",
                                                    Page = j,
                                                    MergePath = Processed_path + "\\" + files[i].Name.Split("-Split")[0],
                                                    Image = str_img,
                                                    Remark = ""
                                                }).ExecuteCommand();
                                                if (result != 1)
                                                {
                                                    Console.WriteLine("Database write failure，program stop!");
                                                    return;
                                                }
                                            }
                                        }
                                        var _temp = int.TryParse(((float)j / page).ToString(), out int num);
                                        var _temp2 = int.TryParse(((float)(j-1) / page).ToString(), out int num2);
                                        //筛选“额外条款”，并模糊处理
                                        if (_temp2 == false)
                                        {
                                            if (j != 1)
                                            {
                                                var RemainingPagePath = RemainingPage_Img + @"\";
                                                Function.PdfToImage2(str, RemainingPagePath, filesNmae, @".jpg", ImageFormat.Jpeg, j, j, Function.Definition.Eight);
                                                //识别文字信息
                                                var result = Function.OCRtxt(RemainingPagePath + filesNmae + ".jpg");
                                                if (result.flag == true)
                                                {
                                                    System.Drawing.Image img = System.Drawing.Image.FromFile(RemainingPagePath + filesNmae + ".jpg");
                                                    Bitmap map = new Bitmap(img);
                                                    //马赛克处理后的图片
                                                    System.Drawing.Image img2 = Function.AdjustTobMosaic(map, dim_score, result.count);
                                                    img.Dispose();
                                                    File.Delete(RemainingPagePath + filesNmae + ".jpg");
                                                    img2.Save(RemainingPagePath + filesNmae + ".jpg", ImageFormat.Jpeg);
                                                    map.Dispose();
                                                    //再将该图片转pdf后存进Source，便于下面方法直接合并
                                                    Function.ConvertJPG2PDF(RemainingPagePath + filesNmae + ".jpg", Source_Path + "\\" + filesNmae + ".pdf", "A4", true);
                                                }
                                            }
                                        }
                                        //每四次拆分后，执行一次合并
                                        if (_temp == true)
                                        {
                                            if (j != 1)//第一次不执行
                                            {
                                                File.Delete(Merged_path + "\\" + text.Replace("/", "").Replace(@"\", "") + @".pdf");
                                                Function.MergePDFs(DMS, Merged_path + "\\" + text.Replace("/", "").Replace(@"\", "") + @".pdf");//合并图片
                                                foreach (var items in DMS)
                                                {
                                                    File.Delete(items.PdfFileName);//处理完即删除拆分文件
                                                }
                                            }
                                            DMS.Clear();
                                        }
                                        var xx = _temp == true ? id = true : id = false;
                                        Console.WriteLine("datetime：" + DateTime.Now + " Complete, Working on the " + j + " page");
                                    }
                                    if (reader != null)
                                    {
                                        reader.Close();
                                    }
                                    Result move_result = Function.MoveFile(files[i].FullName, Processed_path + @"\" + files[i].Name, Processed_path);
                                    if (move_result.flag == false)
                                    {
                                        Console.WriteLine("to:" + files[i].FullName + " File move failed, program stop!");
                                        return;
                                    }
                                    i = -1;
                                    files = di.GetFiles();
                                    continue;
                                }
                                else
                                {
                                    Console.WriteLine("The total number of pages cannot be divided");
                                }
                            }
                            else//单页
                            {
                                OCR_List = O2S.ConvertImg(Source_Path + @"\" + files[i].Name, ImagesPath, 2, true);
                                str_img = Function.DocumentToBase64Str(OCR_List[0].imgPath);
                                foreach (var item in OCR_List)
                                {
                                    Result imgText = Function.CreateOCRParameter(item.imgPath);
                                    //写入数据库（先写数据再移动文件）
                                    var result = db.Insertable(new TableModels.OCRLogs()
                                    {
                                        FileName = item.pdfName,
                                        FilePath = item.pdfPath,
                                        TrustBookNo = imgText.text,
                                        OCRTime = DateTime.Now,
                                        Susscess = imgText.flag,
                                        Message = "",
                                        Page = 0,
                                        MergePath = Processed_path + "\\"+ files[i].Name.Split("-Split")[0],
                                        Image = str_img,
                                        Remark = ""
                                    }).ExecuteCommand();
                                    if (result != 1)
                                    {
                                        Console.WriteLine("Database write failure，program stop!");
                                        return;
                                    }
                                    //移动文件
                                    Result move_result2 = Function.MoveFile(files[i].FullName, Processed_path + @"\" + files[i].Name, Processed_path);
                                    if (move_result2.flag == false)
                                    {
                                        Console.WriteLine("to:" + files[i].FullName + " File move failed, program stop!");
                                        return;
                                    }
                                    Console.WriteLine("datetime：" + DateTime.Now + " Complete, this time processing a total of " + count + " PDF file");
                                }
                            }
                        }
                       
                    }
                }
                t.Enabled = true;
            }
            catch (Exception ex)
            {
                t.Enabled = true;
                Console.WriteLine("datetime：" + DateTime.Now + " Error, reason：" + ex.Message);
                log.Error(ex.Message);
            }
            Console.ReadLine();
        }



        



    }
}
