using System;
using System.Collections.Generic;
using System.Drawing.Imaging;
using System.Drawing;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Drawing.Drawing2D;
using Haruka.Arcade.SEGA835Lib.Devices.Printer.CHC;

namespace Haruka.Arcade.SEGA835Lib.Misc {
    internal static class BitmapExtensions {

        /// <summary>
        /// Converts this bitmap into a 24bpp row-major sequential byte array without padding bytes. This is the format that <see cref="INativeTrampolineCHC.CHC_write(byte*, ref uint, ref ushort)" /> expects.
        /// </summary>
        /// <param name="bitmap">The bitmap</param>
        /// <returns>A 24bpp row-major byte array of this bitmap's pixels</returns>
        public static byte[] GetRawPixelsRGBNoPadding(this Bitmap bitmap) {
            int w = bitmap.Width;
            int h = bitmap.Height;
            byte[] data = new byte[w * h * 3];

            for (int i = 0; i < h; i++) {
                int num2 = i;
                for (int j = 0; j < w; j++) {
                    Color p = bitmap.GetPixel(j, i);
                    int num4 = (num2 * w + j) * 3;
                    data[num4] = p.R;
                    data[num4 + 1] = p.G;
                    data[num4 + 2] = p.B;
                }
            }

            return data;
        }

        /// <summary>
        /// Converts this bitmap into a 8bpp row-major sequential byte array without padding bytes. This is the format that <see cref="INativeTrampolineCHC.CHC_writeHolo(byte*, ref uint, ref ushort)" /> expects.
        /// </summary>
        /// <param name="bitmap">The bitmap</param>
        /// <returns>A 8bpp row-major byte array of this bitmap's pixels</returns>
        public static byte[] GetRawPixelsMonochrome(this Bitmap bitmap) {
            int w = bitmap.Width;
            int h = bitmap.Height;
            byte[] data = new byte[w * h];

            for (int i = 0; i < h; i++) {
                int num = h - i - 1;
                int num2 = i;
                for (int j = 0; j < w; j++) {
                    Color p = bitmap.GetPixel(j, num);
                    int num4 = (num2 * w + j);
                    data[num4] = p.R;
                }
            }

            return data;
        }

        /// <summary>
        /// Creates a stretched copy of this bitmap for the given size.
        /// </summary>
        /// <param name="bitmap">The bitmap.</param>
        /// <param name="size">The new size of the bitmap.</param>
        /// <returns>A stretched copy of the input bitmap.</returns>
        public static Bitmap CopyStretched(this Bitmap bitmap, Size size) {
            Bitmap copy = new Bitmap(size.Width, size.Height);
            using (Graphics g = Graphics.FromImage(copy)) {
                g.InterpolationMode = InterpolationMode.HighQualityBicubic;
                g.DrawImage(bitmap, 0, 0, size.Width, size.Height);
            }
            return copy;
        }

        /// <summary>
        /// Creates a centered copy of this bitmap for the given size. The bitmap may be cropped.
        /// </summary>
        /// <param name="bitmap">The bitmap.</param>
        /// <param name="size">The new size of the bitmap.</param>
        /// <param name="background">The color to fill the background with, if the bitmap is smaller than size.</param>
        /// <returns>A centered copy of the input bitmap.</returns>
        public static Bitmap CopyCentered(this Bitmap bitmap, Size size, Color? background = null) {
            PointF oversize = new PointF((bitmap.Width - size.Width) / -2, (bitmap.Height - size.Height) / -2);
            Bitmap copy = new Bitmap(size.Width, size.Height);
            using (Graphics g = Graphics.FromImage(copy)) {
                if (background != null) {
                    g.Clear(background.Value);
                }
                g.InterpolationMode = InterpolationMode.HighQualityBicubic;
                g.DrawImage(bitmap, new RectangleF(oversize, bitmap.PhysicalDimension));
            }
            return copy;
        }

    }
}
