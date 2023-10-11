using O2S.Components.PDFRender4NET;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OcrToPdfOnConsole2
{
   
    public class O2S
    {
        /// <summary>
        /// 图片1
        /// </summary>
        /// <param name="pdfPath"></param>
        /// <param name="imageFolder"></param>
        /// <param name="definition"></param>
        /// <param name="f"></param>
        /// <returns></returns>
        public static List<ModelList> ConvertImg(string pdfPath, string imageFolder, int definition, bool f = false)
        {
            try
            {
                var resultList = new List<ModelList>();

                PDFFile pdfFile = PDFFile.Open(pdfPath);

                if (!Directory.Exists(imageFolder))
                {
                    Directory.CreateDirectory(imageFolder);
                }

                var startPageNum = 1;
                var endPageNum = pdfFile.PageCount;

                for (int i = startPageNum; i <= endPageNum; i++)
                {
                    ModelList m = new ModelList();
                    Bitmap pageImage = pdfFile.GetPageImage(i - 1, 100 * definition);
                    if (f == true)
                    {
                        Bitmap pageImage2 = KiRotate(pageImage);
                        Bitmap temp = GetPart(pageImage2, 0, 0, 538, 178, 1530, 1500);
                        pageImage = temp;
                    }
                    var savePath = Path.Combine(imageFolder, Path.GetFileNameWithoutExtension(pdfPath) +  "." + ImageFormat.Png.ToString());
                    File.Delete(savePath);
                    pageImage.Save(savePath, ImageFormat.Png);
                    pageImage.Dispose();
                    m.imgPath = savePath;
                    m.pdfPath = pdfPath;
                    m.pdfName = Path.GetFileNameWithoutExtension(pdfPath);
                    resultList.Add(m);
                }
                pdfFile.Dispose();
                return resultList;
            }
            catch (Exception)
            {

                throw;
            }
        }




        /// </summary>
        /// <param name="pPartStartPointX">目标图片开始绘制处的坐标X值(通常为0)</param>
        /// <param name="pPartStartPointY">目标图片开始绘制处的坐标Y值(通常为0)</param>
        /// <param name="pPartWidth">目标图片的宽度</param>
        /// <param name="pPartHeight">目标图片的高度</param>
        /// <param name="pOrigStartPointX">原始图片开始截取处的坐标X值</param>
        /// <param name="pOrigStartPointY">原始图片开始截取处的坐标Y值</param>
        public static Bitmap GetPart(Image originalImg, int pPartStartPointX, int pPartStartPointY, int pPartWidth, int pPartHeight, int pOrigStartPointX, int pOrigStartPointY)
        {
            System.Drawing.Bitmap partImg = new System.Drawing.Bitmap(pPartWidth, pPartHeight);
            System.Drawing.Graphics graphics = System.Drawing.Graphics.FromImage(partImg);
            System.Drawing.Rectangle destRect = new System.Drawing.Rectangle(new System.Drawing.Point(pPartStartPointX, pPartStartPointY), new System.Drawing.Size(pPartWidth, pPartHeight));
            System.Drawing.Rectangle origRect = new System.Drawing.Rectangle(new System.Drawing.Point(pOrigStartPointX, pOrigStartPointY), new System.Drawing.Size(pPartWidth, pPartHeight));
            graphics.DrawImage(originalImg, destRect, origRect, System.Drawing.GraphicsUnit.Pixel);
            return partImg;
        }

        public static Bitmap KiRotate(Bitmap img)
        {
            try
            {
                img.RotateFlip(RotateFlipType.Rotate90FlipNone);
                return img;
            }
            catch
            {
                return null;
            }
        }
    }

    public class ModelList
    {
        public string imgPath { get; set; }
        public string pdfPath { get; set; }
        public string pdfName { get; set; }
    }





}
