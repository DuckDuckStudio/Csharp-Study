using System;
using System.Drawing;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;
using Spectre.Console;

namespace ImageOutput
{
    class Program
    {
        static async Task Main(string[] args)
        {
            // 远程图片 URL
            string imageUrl = "远程图片 URL";

            // 下载远程图片并加载到 Image 对象
            var image = await DownloadImageAsync(imageUrl);

            // 缩放图像到指定大小（例如 50x50）
            var resizedImage = ResizeImage(image, 9, 9);

            // 将缩放后的图片保存到临时路径
            string tempImagePath = Path.Combine(Path.GetTempPath(), "tempImage.png");
            resizedImage.Save(tempImagePath);

            // 在控制台显示缩放后的图片
            AnsiConsole.Write(new CanvasImage(tempImagePath));

            // 删除临时文件
            File.Delete(tempImagePath);
        }

        // 下载远程图片的方法
        static async Task<Image> DownloadImageAsync(string url)
        {
            using (HttpClient client = new HttpClient())
            using (var stream = await client.GetStreamAsync(url))
            {
                return Image.FromStream(stream);
            }
        }

        // 缩放图像的方法
        static Image ResizeImage(Image originalImage, int width, int height)
        {
            var resizedImage = new Bitmap(width, height);
            using (var graphics = Graphics.FromImage(resizedImage))
            {
                // 设置高质量的缩放方式
                graphics.CompositingQuality = System.Drawing.Drawing2D.CompositingQuality.HighQuality;
                graphics.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.HighQuality;
                graphics.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.HighQualityBicubic;

                // 绘制缩放后的图像
                graphics.DrawImage(originalImage, 0, 0, width, height);
            }
            return resizedImage;
        }
    }
}
