﻿using System.Drawing.Imaging;
using System.IO;
using System.Linq;

namespace System.Drawing
{
    /// <summary>
    /// 
    /// </summary>
    public static class ImageExtensions
    {
        /// <summary>
        /// 将图片转码为base64字符串
        /// </summary>
        /// <param name="image"></param>
        /// <param name="format"></param>
        /// <returns></returns>
        public static string Encode(this Image image, ImageFormat format = default)
        {
            using var memory = new MemoryStream();
            image.Save(memory, format ?? image.RawFormat);
            return Convert.ToBase64String(memory.ToArray());
        }

        /// <summary>
        /// 改变透明度
        /// </summary>
        /// <param name="image"></param>
        /// <param name="opacity"></param>
        public static Image Opacity(this Image image, float opacity)
        {
            if (image == null)
                throw new ArgumentNullException(nameof(image));

            if (opacity < 0 || opacity > 1)
                throw new ArgumentOutOfRangeException("不透明度必须为0~1之间的浮点数");

            var colorMatrix = new ColorMatrix(new[]
            {
                new float[] { 1, 0, 0, 0, 0 },
                new float[] { 0, 1, 0, 0, 0 },
                new float[] { 0, 0, 1, 0, 0 },
                new float[] { 0, 0, 0, opacity > 1 ? 1 : opacity < 0 ? 0 : opacity, 0 },
                new float[] { 0, 0, 0, 0, 1 }
            });
            var imageAttributes = new ImageAttributes();
            imageAttributes.SetColorMatrix(colorMatrix, ColorMatrixFlag.Default, ColorAdjustType.Bitmap);

            var output = new Bitmap(image.Width, image.Height);

            using var graphics = Graphics.FromImage(output);
            graphics.DrawImage(image, new Rectangle(new Point(0, 0), new Size(image.Width, image.Height)), 0, 0, image.Width, image.Height, GraphicsUnit.Pixel, imageAttributes);

            return output;
        }

        /// <summary>
        /// 清除整个图像并以指定颜色填充
        /// </summary>
        /// <param name="image"></param>
        /// <param name="color"></param>
        /// <returns></returns>
        public static void Clear(this Image image, Color color)
        {
            using var graphics = Graphics.FromImage(image);
            graphics.Clear(color);
        }

        /// <summary>
        /// 裁剪图片
        /// </summary>
        /// <param name="image">源图片</param>
        /// <param name="startX">裁剪开始 X 坐标</param>
        /// <param name="startY">裁剪开始 Y 坐标</param>
        /// <param name="width">裁剪宽度</param>
        /// <param name="height">裁剪高度</param>
        /// <returns></returns>
        public static Image Crop(this Image image, int startX, int startY, int width, int height)
        {
            if (startX >= image.Width || startY >= image.Height || width <= 0 || height <= 0)
                throw new ArgumentException();

            if (startX < 0)
                startX = 0;
            if (startY < 0)
                startY = 0;
            if (width > image.Width - startX)
                width = image.Width - startX;
            if (height > image.Height - startY)
                height = image.Height - startY;

            return Crop(image, new Rectangle(startX, startY, width, height));
        }

        /// <summary>
        /// 裁剪图片
        /// </summary>
        /// <param name="image"></param>
        /// <param name="rectangle"></param>
        /// <returns></returns>
        public static Image Crop(this Image image, Rectangle rectangle)
        {
            if (image == null)
                throw new ArgumentNullException(nameof(image));

            using var output = new Bitmap(image);
            return output.Clone(rectangle, image.PixelFormat);
        }

        /// <summary>
        /// 调整大小
        /// </summary>
        /// <param name="image"></param>
        /// <param name="width">宽</param>
        /// <param name="height">高</param>
        /// <param name="zoom">等比缩放</param>
        /// <param name="coverSize">Ture：在同时指定宽和高并且等比缩放的情况下，将裁剪图片以满足宽高比</param>
        /// <returns></returns>
        public static Image Resize(this Image image, int? width, int? height, bool zoom = true, bool coverSize = false)
        {
            if (image == null)
                throw new ArgumentNullException(nameof(image));

            width = width == 0 ? null : width;
            height = height == 0 ? null : height;

            if (width == null && height == null)
                return image;

            var w = image.Width;
            var h = image.Height;

            if (width.HasValue && height.HasValue)
            {
                var wW = (double)image.Width / width.Value;
                var hH = (double)image.Height / height.Value;
                if (zoom)
                {
                    if (coverSize)
                    {
                        w = width.Value;
                        h = height.Value;
                        if (wW > hH)
                        {
                            var _width = (int)(w * hH);
                            image = image.Crop(Math.Abs(image.Width - _width) / 2, 0, _width, image.Height);
                        }
                        else
                        {
                            var _height = (int)(h * wW);
                            image = image.Crop(0, Math.Abs(image.Height - _height) / 2, image.Width, _height);
                        }
                    }
                    else
                    {
                        if (wW > hH)
                        {
                            w = width.Value;
                            h = (int)(image.Height / wW);
                        }
                        else
                        {
                            w = (int)(image.Width / hH);
                            h = height.Value;
                        }
                    }
                }
                else
                {
                    w = width.Value;
                    h = height.Value;
                }
            }
            else if (width.HasValue)
            {
                w = width.Value;
                h = zoom ? (int)(image.Height / ((double)image.Width / width.Value)) : image.Height;
            }
            else if (height.HasValue)
            {
                w = zoom ? (int)(image.Width / ((double)image.Height / height.Value)) : image.Width;
                h = height.Value;
            }

            var output = new Bitmap(w, h);
            using var graphics = Graphics.FromImage(output);
            graphics.DrawImage(image, 0, 0, w, h);

            return output;
        }

        /// <summary>
        /// 组合多张图片
        /// </summary>
        /// <param name="image">底图</param>
        /// <param name="images">图片集合</param>
        /// <returns></returns>
        public static void Combine(this Image image, params (Point Point, Size Size, Image Image)[] images)
        {
            if (image == null)
                throw new ArgumentNullException(nameof(image));
            if (images == null)
                throw new ArgumentNullException(nameof(images));

            if (images.Length == 0)
                return;

            using var graphics = Graphics.FromImage(image);
            foreach (var item in images)
            {
                graphics.DrawImage(item.Image, new Rectangle(item.Point, item.Size));
            }
        }

        /// <summary>
        /// 拼接图片
        /// </summary>
        /// <param name="image"></param>
        /// <param name="image2"></param>
        /// <param name="direction">true=水平方向，false=垂直方向</param>
        /// <returns></returns>
        public static Image Concatenate(this Image image, Image image2, bool direction = false)
        {
            if (image == null)
                throw new ArgumentNullException(nameof(image));

            if (image2 == null)
                return image.Clone() as Image;

            if (direction)
            {
                var output = new Bitmap(image.Width + image2.Width, Math.Max(image.Height, image2.Height));

                using var graphics = Graphics.FromImage(output);
                graphics.DrawImage(image, 0, 0);
                graphics.DrawImage(image2, image.Width, 0);

                return output;
            }
            else
            {
                var output = new Bitmap(Math.Max(image.Width, image2.Width), image.Height + image2.Height);

                using var graphics = Graphics.FromImage(output);
                graphics.DrawImage(image, 0, 0);
                graphics.DrawImage(image2, 0, image.Height);

                return output;
            }
        }

        /// <summary>
        /// 压缩存储
        /// </summary>
        /// <param name="image"></param>
        /// <param name="path"></param>
        /// <param name="quality">质量，0~100</param>
        /// <param name="format"></param>
        public static void CompressSave(this Image image, string path, long quality, ImageFormat format = default)
        {
            if (quality < 0)
                quality = 0;
            if (quality > 100)
                quality = 100;

            var imageCodecInfo = ImageCodecInfo.GetImageEncoders().FirstOrDefault(_ => _.FormatID == GetFormat(Path.GetExtension(path)).Guid);
            if (imageCodecInfo != null)
            {
                using var encoderParameters = new EncoderParameters(1);
                encoderParameters.Param[0] = new EncoderParameter(Encoder.Quality, quality);
                image.Save(path, imageCodecInfo, encoderParameters);
            }
            else
                image.Save(path, format ?? image.RawFormat);
        }

        static ImageFormat GetFormat(string extension)
        {
            switch (extension.ToLower())
            {
                case ".bmp":
                    return ImageFormat.Bmp;
                case ".emf":
                    return ImageFormat.Emf;
                case ".gif":
                    return ImageFormat.Gif;
                case ".ico":
                case ".icon":
                    return ImageFormat.Icon;
                case ".jpg":
                case ".jpeg":
                    return ImageFormat.Jpeg;
                case ".png":
                    return ImageFormat.Png;
                case ".tif":
                case ".tiff":
                    return ImageFormat.Tiff;
                case ".wmf":
                    return ImageFormat.Wmf;
                default:
                    return ImageFormat.Jpeg;
            }
        }
    }
}
