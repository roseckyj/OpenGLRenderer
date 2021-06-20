using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;

namespace GLRenderer
{
    class ConsoleRenderer
    {
        private bool currentlyRendering = false;

        public void RenderBitmap(Bitmap bitmap, bool colors)
        {
            int width = Console.WindowWidth;
            int height = Console.WindowHeight * 2;

            float pixelPerCharX = bitmap.Width / (float)(width + 1);
            float pixelPerCharY = bitmap.Height / (float)(height + 1);

            StringBuilder sb = new StringBuilder();

            for (int y = height - 1; y >= 0; y -= 2)
            {
                for (int x = 0; x < width; x++)
                {
                    if (colors)
                    {
                        Color pixelColorUpper = PixelAt(ref bitmap, (x + 0.5) * pixelPerCharX, (y + 1.5) * pixelPerCharY, pixelPerCharX, pixelPerCharY);
                        Color pixelColorLower = PixelAt(ref bitmap, (x + 0.5) * pixelPerCharX, (y + 0.5) * pixelPerCharY, pixelPerCharX, pixelPerCharY);
                        sb.Append("\x1b[38;2;");
                        sb.Append(pixelColorUpper.R);
                        sb.Append(';');
                        sb.Append(pixelColorUpper.G);
                        sb.Append(';');
                        sb.Append(pixelColorUpper.B);
                        sb.Append('m');
                        sb.Append("\x1b[48;2;");
                        sb.Append(pixelColorLower.R);
                        sb.Append(';');
                        sb.Append(pixelColorLower.G);
                        sb.Append(';');
                        sb.Append(pixelColorLower.B);
                        sb.Append('m');
                        sb.Append('▀');
                    }
                    else
                    {
                        Color pixelColor = bitmap.GetPixel((int)(x * pixelPerCharX), (int)(y * pixelPerCharY));
                        float sum = pixelColor.GetBrightness();
                        sb.Append("\x1b[38;2;");
                        sb.Append(pixelColor.R);
                        sb.Append(';');
                        sb.Append(pixelColor.G);
                        sb.Append(';');
                        sb.Append(pixelColor.B);
                        sb.Append('m');
                        sb.Append(sum switch
                        {
                            < 0.3f => ' ',
                            < 0.4f => '.',
                            < 0.6f => 'x',
                            < 0.7f => 'X',
                            _ => '#',
                        });
                    }
                }
                if (y != 0) sb.Append('\n');
            }
            if (currentlyRendering) return;
            currentlyRendering = true;
            Console.SetCursorPosition(0, 0);
            Console.Write(sb.ToString());
            Console.SetCursorPosition(0, 0);
            currentlyRendering = false;
        }

        private Color PixelAt(ref Bitmap bitmap, double x, double y, float pixelPerCharX, float pixelPerCharY) {

            /*
            try
            {
                Color p_00 = bitmap.GetPixel((int)(x + pixelPerCharX * (-0.5)), (int)(y + pixelPerCharY * (-0.5)));
                Color p_10 = bitmap.GetPixel((int)(x + pixelPerCharX * (+0.5)), (int)(y + pixelPerCharY * (-0.5)));
                Color p_01 = bitmap.GetPixel((int)(x + pixelPerCharX * (-0.5)), (int)(y + pixelPerCharY * (+0.5)));
                Color p_11 = bitmap.GetPixel((int)(x + pixelPerCharX * (+0.5)), (int)(y + pixelPerCharY * (+0.5)));

                return Color.FromArgb(
                    (p_00.R + p_10.R + p_01.R + p_11.R) / 4,
                    (p_00.G + p_10.G + p_01.G + p_11.G) / 4,
                    (p_00.B + p_10.B + p_01.B + p_11.B) / 4
                );
            } catch {
                return Color.Black;
            }
            */

            const int interpolation = 3; // Lower = better (but slower)

            int r = 0;
            int g = 0;
            int b = 0;
            int n = 0;

            for (int x1 = (int)(x + pixelPerCharX * (-0.5)); x1 <= (int)(x + pixelPerCharX * (+0.5)); x1 += interpolation) {
                for (int y1 = (int)(y + pixelPerCharY * (-0.5)); y1 <= (int)(y + pixelPerCharY * (+0.5)); y1 += interpolation)
                {
                    try
                    {
                        Color c = bitmap.GetPixel(x1, y1);
                        r += c.R;
                        g += c.G;
                        b += c.B;
                        n++;
                    }
                    catch { }
                }
            }
            if (n == 0) return Color.Black;
            return Color.FromArgb(
                r / n,
                g / n,
                b / n
            );
        }
    }
}
