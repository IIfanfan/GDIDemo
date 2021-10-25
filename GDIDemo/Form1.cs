using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Drawing.Imaging;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace GDIDemo
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            //读取本地图片
            Bitmap mbitmap = new Bitmap(@"D:\FTLProject\TCard\TCardPrint\bin\Debug\Barcode\YXK1.jpg");
            mbitmap.Save("0.jpg");
            //生成位图
            Bitmap srcouce = new Bitmap(200, 200, PixelFormat.Format32bppRgb);
            //绘画操作
            Graphics g1 = Graphics.FromImage(srcouce); //创建b1的Graphics
            //修改颜色
            g1.FillRectangle(Brushes.White, new Rectangle(0, 0, 200, 200)); //把b1涂成红色
            //写字
            Font font = new Font("宋体", 12);
            //Point一样，只是值是浮点类型
            PointF point = new PointF(0, 0);
            g1.DrawString("我是Kimisme", font, Brushes.Black, point);
            Pen mypen = new Pen(Color.Black);
            g1.DrawRectangle(mypen, new Rectangle(0, 0, 50, 50));
            srcouce.Save("1.jpg");
            //合并图像
            copyBitmap(mbitmap, srcouce);

        }
        /**
         * 内存法复制图片
         * */
        private void copyBitmap(Bitmap bmpSrc, Bitmap bmpDest)
        {
            int w = bmpSrc.Width, h = bmpSrc.Height;
            PixelFormat format = bmpSrc.PixelFormat;

            // Lock the bitmap's bits.  锁定位图
            Rectangle rect = new Rectangle(0, 0, w, h);
            BitmapData bmpDataSrc = bmpSrc.LockBits(rect, ImageLockMode.ReadOnly, format);
            // Get the address of the first line.获取首行地址
            IntPtr ptrSrc = bmpDataSrc.Scan0;


            Rectangle rect12 = new Rectangle(100, 100, w, h);

            BitmapData bmpDataDest = bmpDest.LockBits(rect12, ImageLockMode.WriteOnly, format);
            IntPtr ptrDest = bmpDataDest.Scan0;


            // Declare an array to hold the bytes of the bitmap.定义数组保存位图
            int bytes = Math.Abs(bmpDataSrc.Stride) * h;
            byte[] rgbValues = new byte[bytes];

            // Copy the RGB values into the array.复制RGB值到数组
            System.Runtime.InteropServices.Marshal.Copy(ptrSrc, rgbValues, 0, bytes);
            //复制到新图
            System.Runtime.InteropServices.Marshal.Copy(rgbValues, 0, ptrDest, bytes);

            // Unlock the bits.解锁
            bmpSrc.UnlockBits(bmpDataSrc);
            bmpDest.UnlockBits(bmpDataDest);
            bmpDest.Save("2.jpg");
            //旋转图像
            GetRotateImage(bmpDest, 90).Save("3.jpg");

        }
        /// <summary>
        /// 计算矩形绕中心任意角度旋转后所占区域矩形宽高
        /// </summary>
        /// <param name="width">原矩形的宽</param>
        /// <param name="height">原矩形高</param>
        /// <param name="angle">顺时针旋转角度</param>
        /// <returns></returns>
        public Rectangle GetRotateRectangle(int width, int height, float angle)
        {
            double radian = angle * Math.PI / 180; ;
            double cos = Math.Cos(radian);
            double sin = Math.Sin(radian);
            //只需要考虑到第四象限和第三象限的情况取大值(中间用绝对值就可以包括第一和第二象限)
            int newWidth = (int)(Math.Max(Math.Abs(width * cos - height * sin), Math.Abs(width * cos + height * sin)));
            int newHeight = (int)(Math.Max(Math.Abs(width * sin - height * cos), Math.Abs(width * sin + height * cos)));
            return new Rectangle(0, 0, newWidth, newHeight);
        }

        /// <summary>
        /// 获取原图像绕中心任意角度旋转后的图像
        /// </summary>
        /// <param name="rawImg"></param>
        /// <param name="angle"></param>
        /// <returns></returns>
        public Bitmap GetRotateImage(Bitmap srcImage, int angle)
        {
            angle = angle % 360;
            //原图的宽和高
            int srcWidth = srcImage.Width;
            int srcHeight = srcImage.Height;
            //图像旋转之后所占区域宽和高
            Rectangle rotateRec = GetRotateRectangle(srcWidth, srcHeight, angle);
            int rotateWidth = rotateRec.Width;
            int rotateHeight = rotateRec.Height;
            //目标位图
            Bitmap destImage = null;
            Graphics graphics = null;
            try
            {
                //定义画布，宽高为图像旋转后的宽高
                destImage = new Bitmap(rotateWidth, rotateHeight);
                //graphics根据destImage创建，因此其原点此时在destImage左上角
                graphics = Graphics.FromImage(destImage);
                //要让graphics围绕某矩形中心点旋转N度，分三步
                //第一步，将graphics坐标原点移到矩形中心点,假设其中点坐标（x,y）
                //第二步，graphics旋转相应的角度(沿当前原点)
                //第三步，移回（-x,-y）
                //获取画布中心点
                Point centerPoint = new Point(rotateWidth / 2, rotateHeight / 2);
                //将graphics坐标原点移到中心点
                graphics.TranslateTransform(centerPoint.X, centerPoint.Y);
                //graphics旋转相应的角度(绕当前原点)
                graphics.RotateTransform(angle);
                //恢复graphics在水平和垂直方向的平移(沿当前原点)
                graphics.TranslateTransform(-centerPoint.X, -centerPoint.Y);
                //此时已经完成了graphics的旋转

                //计算:如果要将源图像画到画布上且中心与画布中心重合，需要的偏移量
                Point Offset = new Point((rotateWidth - srcWidth) / 2, (rotateHeight - srcHeight) / 2);
                //将源图片画到rect里（rotateRec的中心）
                graphics.DrawImage(srcImage, new Rectangle(Offset.X, Offset.Y, srcWidth, srcHeight));
                //重至绘图的所有变换
                graphics.ResetTransform();
                graphics.Save();
            }
            catch (Exception ex)
            {
                throw ex;
            }
            finally
            {
                if (graphics != null)
                    graphics.Dispose();
            }
            return destImage;
        }

    }
}
