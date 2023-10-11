using iTextSharp.text.pdf;
using O2S.Components.PDFRender4NET;
using OcrToPdfOnConsole2.TableModels;
using PaddleOCRSharp;
using System;
using System.Collections.Generic;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Configuration;
using SqlSugar;
using iTextSharp.text;

namespace OcrToPdfOnConsole2
{
    public class Function
    {
        //全局实例化
        private static OCRModelConfig config = null;
        private static OCRParameter oCRParameter = new OCRParameter();
        public static PaddleOCREngine engine = new PaddleOCREngine(config, oCRParameter);
        public static string Merged_path = ConfigurationManager.AppSettings["Merged_path"];

        /// <summary>
        /// ocr识别
        /// </summary>
        /// <param name="fileName"></param>
        /// <returns></returns>
        public static Result CreateOCRParameter(string fileName)
        {
            OCRModelConfig config = null;

            Result result = new Result();
            result.flag = true;
            var imagebyte = File.ReadAllBytes(fileName);
            Bitmap bitmap = new Bitmap(new MemoryStream(imagebyte));
            OCRResult ocrResult = new OCRResult();

            ocrResult = engine.DetectText(bitmap);

            if (ocrResult != null)
            {
                var btxt = ocrResult.TextBlocks;
                var r1 = "";
                var r2 = "";
                var r3 = "";
                foreach (var val in btxt)
                {
                    var rtemp = val.Text.ToString();
                    var r = Regex.Replace(rtemp, @"\s", "");
                    if (r.Contains("("))
                    {
                        if (!r2.Contains('('))
                        {
                            r3 = r.Replace(" ", "");
                        }
                    }
                    else if (Regex.IsMatch(r, @"^[0-9]\S*$"))
                    {
                        r2 = r.Replace(" ", "");
                    }
                    else if (r.ToLower().Contains("no"))
                    {
                        r1 = r.Replace(" ", "");
                    }
                }
                result.text = r1 + r2 + r3;
            }
            else
            {
                result.flag = false;
            }
            return result;
        }



        /// <summary>
        /// 
        /// </summary>
        /// <param name="fileName"></param>
        /// <returns></returns>
        public static Result2 OCRtxt(string fileName)
        {
            Result2 result = new Result2();
            OCRModelConfig config = null;
            var imagebyte = File.ReadAllBytes(fileName);
            Bitmap bitmap = new Bitmap(new MemoryStream(imagebyte));
            OCRResult ocrResult = new OCRResult();
            ocrResult = engine.DetectText(bitmap);
            result.count = 0;
            result.flag = false;
            foreach(var item in ocrResult.TextBlocks)
            {
                var result1 = item.Text.Contains("額外條款");
                var result2 = item.Text.Contains("额外款");
                var result3 = item.Text.Contains("Extra");
                var result4 = item.Text.Contains("Terms");
                if (result1 == true || result2 == true || result3 == true || result4 == true)
                {
                    result.flag = true;
                    break;
                }
                result.count++;
            }
            return result;
        }



        /// <summary>
        /// 
        /// </summary>
        /// <param name="fileName"></param>
        /// <returns></returns>
        public static bool OCRpdf(string fileName)
        {
            try
            {
                string pdffilename = fileName;
                iTextSharp.text.pdf.PdfReader pdfReader = new iTextSharp.text.pdf.PdfReader(pdffilename);
                int numberOfPages = pdfReader.NumberOfPages;
                StringBuilder text = new StringBuilder();
                for (int i = 1; i <= numberOfPages; ++i)
                {
                    text.Append(iTextSharp.text.pdf.parser.PdfTextExtractor.GetTextFromPage(pdfReader, i));
                }
                pdfReader.Close();
                return false;
            }
            catch (Exception ex)
            {
                return false;
            }
        }



        /// <summary>
        /// 移动文件到指定位置
        /// </summary>
        /// <param name="oldPath">原文件位置+文件名(例如E:\TEST\1.txt)</param>
        /// <param name="newPath">移动文件位置+文件名(例如E:\TEST\File\1.txt)</param>
        /// <param name="path">移动文件夹位置(例如E:\TEST\File)</param>>
        public static Result MoveFile(string oldPath, string newPath, string path)
        {
            Result result = new Result();//1:成功 0:失败
            try
            {
                //判断是否是拆分文件
                if (oldPath.Contains("Split"))
                {
                    File.Delete(oldPath);
                }
                else
                {
                    //判断移动的文件夹是否存在
                    if (Directory.Exists(path))
                    {
                        //先删除再移动
                        File.Delete(newPath);
                        File.Move(oldPath, newPath);
                    }
                    else
                    {
                        Directory.CreateDirectory(path);
                        File.Delete(newPath);
                        File.Move(oldPath, newPath);
                    }
                }
                result.flag = true;
            }
            catch (Exception ex)
            {
                result.flag = false;
                Console.WriteLine("File move failed:" + ex.ToString());
            }
            return result;
        }



        /// <summary>
        /// 压缩图片并且转base64
        /// </summary>
        /// <param name="width"></param>
        /// <param name="height"></param>
        /// <param name="filePath">图片地址</param>
        /// <returns></returns>
        public static string DocumentToBase64Str(string filePath, int width = 150, int height = 30)
        {
            var aa = File.ReadAllBytes(filePath);
            MemoryStream msSource = new MemoryStream(aa);
            Bitmap btImage = new Bitmap(msSource);
            msSource.Close();
            System.Drawing.Image serverImage = btImage;
            //画板大小
            int finalWidth = width, finalHeight = height;
            int srcImageWidth = serverImage.Width;
            int srcImageHeight = serverImage.Height;
            if (srcImageWidth > srcImageHeight)
            {
                finalHeight = srcImageHeight * width / srcImageWidth;
            }
            else
            {
                finalWidth = srcImageWidth * height / srcImageHeight;
            }
            //新建一个bmp图片
            System.Drawing.Image newImage = new Bitmap(width, height);
            //新建一个画板
            Graphics g = Graphics.FromImage(newImage);
            //设置高质量插值法
            g.InterpolationMode = InterpolationMode.High;
            //设置高质量,低速度呈现平滑程度
            g.SmoothingMode = SmoothingMode.HighQuality;
            //清空画布并以透明背景色填充
            g.Clear(Color.White);
            //在指定位置并且按指定大小绘制原图片的指定部分
            g.DrawImage(serverImage, new System.Drawing.Rectangle((width - finalWidth) / 2, (height - finalHeight) / 2, finalWidth, finalHeight), 0, 0, srcImageWidth, srcImageHeight, GraphicsUnit.Pixel);
            //以jpg格式保存缩略图
            MemoryStream msSaveImage = new MemoryStream();
            newImage.Save(msSaveImage, ImageFormat.Jpeg);
            serverImage.Dispose();
            newImage.Dispose();
            g.Dispose();
            byte[] imageBytes = msSaveImage.ToArray();
            msSaveImage.Close();
            return "data:image/jpeg;base64," + Convert.ToBase64String(imageBytes);
        }



        /// <summary>
        /// 图片清晰度
        /// </summary>
        public enum Definition
        {
            One = 1, Two = 2, Three = 3, Four = 4, Five = 5, Six = 6, Seven = 7, Eight = 8, Nine = 9, Ten = 10
        }



        /// <summary>
        /// 将PDF转为图片
        /// </summary>
        /// <param name="pdfPath">PDF文件路径</param>
        /// <param name="imagePath">图片输出路径</param>
        /// <param name="imageName">图片名称</param>
        /// <param name="imagePathFormat">图片格式</param>
        /// <param name="imageFormat">图片输出格式</param>
        /// <param name="startPageNum">从PDF文档的第几页开始转换</param>
        /// <param name="endPageNum">从PDF文档的第几页开始停止转换</param>
        /// <param name="definition">图片清晰度，数字越大越清晰</param>
        public static void PdfToImage2(string pdfPath, string imagePath, string imageName, string imagePathFormat, System.Drawing.Imaging.ImageFormat imageFormat, int startPageNum, int endPageNum, Definition definition)
        {
            PDFFile pdfFile = PDFFile.Open(pdfPath);
            if (!System.IO.Directory.Exists(imagePath))
            {
                System.IO.Directory.CreateDirectory(imagePath);
            }
            if (startPageNum <= 0) { startPageNum = 1; }
            if (endPageNum > pdfFile.PageCount) { endPageNum = pdfFile.PageCount; }
            if (startPageNum > endPageNum)
            {
                int tempPageNum = startPageNum;
                startPageNum = endPageNum;
                endPageNum = startPageNum;
            }
            for (int i = startPageNum; i <= endPageNum; i++)
            {
                System.Drawing.Bitmap pageImage = pdfFile.GetPageImage(i - 1, 56 * (int)definition);
                pageImage.Save($"{imagePath}{imageName}{imagePathFormat}", imageFormat);
                pageImage.Dispose();
            }
            pdfFile.Dispose();
        }



        /// <summary>
        /// 合并pdf
        /// </summary>
        /// <param name="pdfFiles">pdf文件路径</param>
        /// <param name="outputPdf">输出路径</param>
        public static void MergePDFs(List<DataModel> pdfFiles, string outputPdf)
        {
            using (FileStream stream = new FileStream(outputPdf, FileMode.OpenOrCreate))
            {
                iTextSharp.text.Document document = new iTextSharp.text.Document();
                PdfCopy pdf = new PdfCopy(document, stream);
                document.Open();
                foreach (var file in pdfFiles)
                {
                    using (iTextSharp.text.pdf.PdfReader reader = new iTextSharp.text.pdf.PdfReader(file.PdfFileName))
                    {
                        for (int i = 1; i <= reader.NumberOfPages; i++)
                        {
                            PdfImportedPage page = pdf.GetImportedPage(reader, i);
                            pdf.AddPage(page);
                        }
                    }
                }
                document.Close();
            }
        }



        /// <summary>
        /// 马赛克处理
        /// </summary>
        /// <param name="bitmap"></param>
        /// <param name="effectWidth"> 影响范围 每一个格子数 </param>
        /// <param name="offfset">离顶点的高度</param>
        /// <returns></returns>
        public static Bitmap AdjustTobMosaic(System.Drawing.Bitmap bitmap, int effectWidth, int offfset)
        {
            var Height = bitmap.Height  - 1900;//固定高度
            var Width = bitmap.Width - 230;
            offfset = offfset * 101;
            // 差异最多的就是以照一定范围取样完之后直接去下一个范围
            for (int heightOfffset = offfset; heightOfffset < Height; heightOfffset += effectWidth)
            {
                for (int widthOffset = 0; widthOffset < Width; widthOffset += effectWidth)
                {
                    int avgR = 0, avgG = 0, avgB = 0;
                    int blurPixelCount = 0;

                    for (int x = widthOffset; (x < widthOffset + effectWidth && x < Width); x++)
                    {
                        for (int y = heightOfffset; (y < heightOfffset + effectWidth && y < Height); y++)
                        {
                            System.Drawing.Color pixel = bitmap.GetPixel(x, y);

                            avgR += pixel.R;
                            avgG += pixel.G;
                            avgB += pixel.B;

                            blurPixelCount++;
                        }
                    }

                    // 计算范围平均
                    avgR = avgR / blurPixelCount;
                    avgG = avgG / blurPixelCount;
                    avgB = avgB / blurPixelCount;

                    // 所有范围内都设定此值
                    for (int x = widthOffset; (x < widthOffset + effectWidth && x < Width); x++)
                    {
                        for (int y = heightOfffset; (y < heightOfffset + effectWidth && y < Height); y++)
                        {

                            System.Drawing.Color newColor = System.Drawing.Color.FromArgb(avgR, avgG, avgB);
                            bitmap.SetPixel(x, y, newColor);
                        }
                    }
                }
            }
            return bitmap;
        }



        /// <summary>
        /// JPG转PDF
        /// </summary>
        /// <param name="jpgfile">图片路径</param>
        /// <param name="pdf">生成的PDF路径</param>
        /// <param name="pageSize">A4，A5</param>
        /// <param name="Vertical">True:纵向，False横向</param>
        public static void ConvertJPG2PDF(string jpgfile, string pdf, string pageSize, bool Vertical = true)
        {
            float width = 0, height = 0;
            Document document;

            #region 根据纸张大小，纵横向，设置画布长宽
            if (pageSize.ToUpper() == "A4")
            {
                if (Vertical)//纵向
                {
                    width = iTextSharp.text.PageSize.A4.Width;
                    height = iTextSharp.text.PageSize.A4.Height;
                }
                else//横向
                {
                    width = iTextSharp.text.PageSize.A4.Height;
                    height = iTextSharp.text.PageSize.A4.Width;
                }
            }
            else if (pageSize.ToUpper() == "A5")
            {
                if (Vertical)
                {
                    width = iTextSharp.text.PageSize.A5.Width;
                    height = iTextSharp.text.PageSize.A5.Height;
                }
                else
                {
                    width = iTextSharp.text.PageSize.A5.Height;
                    height = iTextSharp.text.PageSize.A5.Width;
                }
            }

            iTextSharp.text.Rectangle pageSizeNew = new iTextSharp.text.Rectangle(width, height);
            document = new Document(pageSizeNew);
            #endregion 


            using (var stream = new FileStream(pdf, FileMode.Create, FileAccess.Write, FileShare.None))
            {

                PdfWriter.GetInstance(document, stream);

                document.Open();

                using (var imageStream = new FileStream(jpgfile, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
                {
                    var image = iTextSharp.text.Image.GetInstance(imageStream);

                    //缩放图像比例
                    image.ScaleToFit(width, height);
                    image.SetAbsolutePosition(0, 0);

                    image.Alignment = iTextSharp.text.Image.ALIGN_MIDDLE;

                    document.Add(image);
                }
                document.Close();

            }

        }

    }
}
