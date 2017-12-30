using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using static System.Configuration.ConfigurationManager;

namespace TraderBoi
{
    class ImageSearcher
    {
        //anan
        [DllImport(@"ImageSearchDLLx32.dll")]
        public static extern IntPtr ImageSearch(int x, int y, int right, int bottom, [MarshalAs(UnmanagedType.LPStr)]string imagePath);

        public static int RegionX1 = int.Parse(AppSettings["RegionX1"]);
        public static int RegionY1 = int.Parse(AppSettings["RegionY1"]);
        public static int RegionX2 = int.Parse(AppSettings["RegionX2"]);
        public static int RegionY2 = int.Parse(AppSettings["RegionY2"]);
        public static string[] UseImageSearch(string imgPath, string tolerance, string transparency)
        {
            imgPath = "*Trans" + transparency + " " + imgPath;
            imgPath = "*" + tolerance + " " + imgPath;

            IntPtr result = ImageSearch(RegionX1, RegionY1, RegionX2, RegionY2, imgPath);
            string res = Marshal.PtrToStringAnsi(result);

            if (res[0] == '0') return null;

            string[] data = res.Split('|');

            int x; int y;
            int.TryParse(data[1], out x);
            int.TryParse(data[2], out y);

            return data;
        }

        public static string[] UseImageSearchArea(int xCoord1, int yCoord1, int xCoord2, int yCoord2, string imgPath, string tolerance, string transparency)
        {
            imgPath = "*Trans" + transparency + " " + imgPath;
            imgPath = "*" + tolerance + " " + imgPath;

            IntPtr result = ImageSearch(xCoord1, yCoord1, xCoord2, yCoord2, imgPath);
            string res = Marshal.PtrToStringAnsi(result);

            if (res[0] == '0') return null;

            string[] data = res.Split('|');

            int x; int y;
            int.TryParse(data[1], out x);
            int.TryParse(data[2], out y);

            return data;
        }

        public static int IsitBlue(Color a)
        {
            Color black = Color.FromArgb(36, 50, 56);
            Color blue = Color.FromArgb(102, 148, 176);
            int BlackDistance = (int)(a.R - black.R) + (int)(a.G - black.G) + (int)(a.B - black.B);
            int BlueDistance = (int)(blue.R - a.R) + (int)(blue.G - a.G) + (int)(blue.B - a.B);
            if (BlueDistance < BlackDistance)
                return 1;
            return 0;
        }

        public static Color CalculateAverageColor(Bitmap bmp)
        {

            //Used for tally
            int r = 0;
            int g = 0;
            int b = 0;

            int total = 0;

            for (int x = 0; x < bmp.Width; x++)
            {
                for (int y = 0; y < bmp.Height; y++)
                {
                    Color clr = bmp.GetPixel(x, y);

                    r += clr.R;
                    g += clr.G;
                    b += clr.B;

                    total++;
                }
            }

            //Calculate average
            r /= total;
            g /= total;
            b /= total;

            return Color.FromArgb(r, g, b);
        }
    }
}
