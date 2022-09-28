using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AsciiArtGeneratorGUI
{
    class AsciiGenerator
    {
        public static string ThreeLevel(Bitmap bitmap, double ratio, string displayCharacter = "\u2588\u2593\u2591\u3000", double[] levelBoundary = null)
        {
            levelBoundary = levelBoundary ?? new double[] { 3.0 *16, 9.5*16 ,15*16 };
            Bitmap resizedBitmap = new Bitmap(bitmap, new Size((int)(bitmap.Width * ratio), (int)(bitmap.Height * ratio)));
            Bitmap grayScaledBitmap = GrayScaleFilter(resizedBitmap);
            StringBuilder sb = new StringBuilder();
            List<Color> colorList = new List<Color>();
            for (int y = 0; y < grayScaledBitmap.Height; y++)
            {
                for (int x = 0; x < grayScaledBitmap.Width; x++)
                {
                    Color c = grayScaledBitmap.GetPixel(x, y);
                    if (!colorList.Contains(c))
                        colorList.Add(c);
                }
            }
            int min = 255;
            int max = 0;
            foreach (Color c in colorList)
            {
                Console.WriteLine(c.ToString());
                if (min > c.R)
                    min = c.R;
                if (max < c.R)
                    max = c.R;
            }
            // 255 (0~16) (0~255)
            // \u3000 : 0% 공백 (0~3)
            // \u2591 : 25%  (3~8)
            // \u2592 : 50%  (8~13)
            // \u2593 : 75%  (13~16)
            // \u2588 : 100%
            for (int y = 0; y < grayScaledBitmap.Height; y++)
            {
                for (int x = 0; x < grayScaledBitmap.Width; x++)
                {
                    char code;
                    int c = grayScaledBitmap.GetPixel(x, y).R;
                    if (c < levelBoundary[0])
                        code = displayCharacter[0];
                    else if (c >= levelBoundary[0] && c < levelBoundary[1])
                        code = displayCharacter[1];
                    else if (c >= levelBoundary[1] && c < levelBoundary[2])
                        code = displayCharacter[2];
                    else if (c >= levelBoundary[2])
                        code = displayCharacter[3];
                    else
                        code = '?';
                    sb.Append(code);
                }
                sb.AppendLine();
            }
            return sb.ToString();
        }
        public static Bitmap GrayScaleFilter(Bitmap image)
        {
            Bitmap grayScale = new Bitmap(image.Width, image.Height);

            for (Int32 y = 0; y < grayScale.Height; y++)
                for (Int32 x = 0; x < grayScale.Width; x++)
                {
                    Color c = image.GetPixel(x, y);

                    Int32 gs = (Int32)(c.R * 0.3 + c.G * 0.59 + c.B * 0.11);

                    grayScale.SetPixel(x, y, Color.FromArgb(gs, gs, gs));
                }
            return grayScale;
        }
    }
}
