using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;
using System.IO;

namespace Hanacaraka
{
    //position 0 : top, 1 : inline, 2 : bottom
    class Sandangan
    {
        public Bitmap img;
        public int horizontalPoint, verticalPoint, position;
        public string name;
        public float ratio;
        public Sandangan(int pos)
        {
            position = pos;
            name = "Un";
            horizontalPoint = verticalPoint = 0;
        }
        public void setCharacteristic(Size size)
        {
            int middle = (img.Width + 1) / 2;
            bool isPrev = false;
            for (int i = 0; i < img.Height; i++)
            {
                if (img.GetPixel(middle, i).R != 255) isPrev = false;
                else if (img.GetPixel(middle, i).R == 255 && !isPrev)
                {
                    isPrev = true;
                    verticalPoint++;
                }
            }
            isPrev = false;
            middle = (img.Height + 1) / 2;
            for (int i = 0; i < img.Width; i++)
            {
                if (img.GetPixel(i, middle).R != 255) isPrev = false;
                else if (img.GetPixel(i, middle).R == 255 && !isPrev)
                {
                    isPrev = true;
                    horizontalPoint++;
                }
            }
            ratio = (float)(img.Width * img.Height) / (float)(size.Width * size.Height);
        }
    }
}
