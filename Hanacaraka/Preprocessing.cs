using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;

namespace Hanacaraka
{
    static class Preprocessing
    {
        /// <summary>
        /// Returns the number indicate the character position on input bitmap with format x, y, width, height
        /// </summary>
        /// <param name="inp"></param>
        /// <returns>4 * n integer indicate the position of characters</returns>
        static public List<int> SegmentingImage(Bitmap inp)
        {
            Color c;
            List<int> res = new List<int>();
            for (int i = 0; i < inp.Width; i++)
            {
                bool isThere = false;
                int minj = 1000, maxj = -1;
                for (int j = 0; j < inp.Height; j++)
                {
                    c = inp.GetPixel(i, j);
                    if (c.R == 255 && c.G == 255 && c.B == 255)
                    {
                        isThere = true;
                        minj = minj > j ? j : minj;
                        maxj = maxj < j ? j : maxj;
                        break;
                    }
                }
                if (isThere && i < inp.Width - 1)
                {
                    res.Add(i);
                    i++;
                    bool isOnreg = true;
                    while (isOnreg)
                    {
                        int j;
                        for (j = 0; j < inp.Height; j++)
                        {
                            c = inp.GetPixel(i, j);
                            if (c.R == 255 && c.G == 255 && c.B == 255)
                            {
                                minj = minj > j ? j : minj;
                                break;
                            }
                        }
                        int k;
                        for (k = inp.Height - 1; k >= 0; k--)
                        {
                            c = inp.GetPixel(i, k);
                            if (c.R == 255 && c.G == 255 && c.B == 255)
                            {
                                maxj = maxj < k ? k : maxj;
                                break;
                            }
                        }
                        if (j == inp.Height)
                        {
                            isOnreg = false;
                            res.Add(i);
                            res.Add(-minj);
                            res.Add(inp.Width * -maxj);
                        }
                        else i++;
                    }
                }
            }
            List<int> FinalRest = new List<int>();
            for (int i = 0; i < res.Count - 3; i += 4)
            {
                FinalRest.Add(res[i]);                                //x pos
                FinalRest.Add(-res[i + 2]);                           //y pos
                FinalRest.Add(res[i + 1] - res[i]);                   //width
                FinalRest.Add(-res[i + 3] / inp.Width - -res[i + 2]); //height
            }
            res.Clear();
            return FinalRest;
        }
        
        ///----------------------------------------------------------------Converting to binary image--------------------------------------
        static public Bitmap GetBinaryImage(Bitmap inp)
        {
            Bitmap res = new Bitmap(inp.Width, inp.Height);
            int[] histogram = new int[256];
            for (int i = 0; i < inp.Width; i++)
            {
                for (int j = 0; j < inp.Height; j++)
                {
                    Color c = inp.GetPixel(i, j);
                    int av = (c.R + c.G + c.B) / 3;
                    res.SetPixel(i, j, Color.FromArgb(av, av, av));
                    histogram[av]++;
                }
            }

            int threshold = GetThreshold(histogram, (double)(res.Width * res.Height));
            if (threshold == 0) return res;

            for (int i = 0; i < inp.Width; i++)
            {
                for (int j = 0; j < inp.Height; j++)
                {
                    if (res.GetPixel(i, j).R < threshold) res.SetPixel(i, j, Color.Black);
                    else res.SetPixel(i, j, Color.White);
                }
            }
            return res;
        }

        static private int GetThreshold(int[] histogram, double total)
        {
            double sum = 0;

            for (int i = 0; i < 256; i++) sum += i * histogram[i];

            double sumB = 0;
            double wB = 0;
            double wF = 0;
            double mB, mF, max = 0, between, threshold = 0;

            for (int i = 0; i < 256; i++)
            {
                wB += histogram[i];
                if (wB == 0.0) continue;

                wF = total - wB;
                if (wF == 0.0) break;

                sumB += i * histogram[i];
                mB = sumB / wB;
                mF = (sum - sumB) / wF;
                between = wB * wF * Math.Pow(mB - mF, 2);

                if (between > max)
                {
                    max = between;
                    threshold = i;
                }
            }

            return (int)threshold;
        }

        ///----------------------------------------------------------------Thinning Process-----------------------------------------------
        static public Bitmap Thinning(Bitmap bmInput)
        {
            bool[,] mulmat = new bool[bmInput.Width, bmInput.Height];
            bool[,] currentImage = new bool[bmInput.Width, bmInput.Height];
            bool[,] tempImg = new bool[bmInput.Width, bmInput.Height];
            Bitmap bmResult = new Bitmap(bmInput.Width, bmInput.Height);

            for (int i = 0; i < bmInput.Width; i++)
                for (int j = 0; j < bmInput.Height; j++)
                {
                    if (bmInput.GetPixel(i, j).R == 255) currentImage[i, j] = true;
                    else currentImage[i, j] = false;
                    tempImg[i, j] = currentImage[i, j];
                }

            while (true)
            {
                for (int i = 1; i < bmInput.Width - 1; i++)
                    for (int j = 1; j < bmInput.Height - 1; j++)
                    {
                        bool[,] matIn = new bool[3, 3];
                        for (int x = i - 1; x <= i + 1; x++)
                            for (int y = j - 1; y <= j + 1; y++)
                                matIn[x - (i - 1), y - (j - 1)] = currentImage[x, y];
                        mulmat[i, j] = isSatisfy(0, matIn);
                    }

                for (int i = 0; i < bmInput.Width; i++)
                    for (int j = 0; j < bmInput.Height; j++)
                        currentImage[i, j] = currentImage[i, j] & mulmat[i, j];

                for (int i = 1; i < bmInput.Width - 1; i++)
                    for (int j = 1; j < bmInput.Height - 1; j++)
                    {
                        bool[,] matIn = new bool[3, 3];
                        for (int x = i - 1; x <= i + 1; x++)
                            for (int y = j - 1; y <= j + 1; y++)
                                matIn[x - (i - 1), y - (j - 1)] = currentImage[x, y];
                        mulmat[i, j] = isSatisfy(1, matIn);
                    }

                for (int i = 0; i < bmInput.Width; i++)
                    for (int j = 0; j < bmInput.Height; j++)
                        currentImage[i, j] = currentImage[i, j] & mulmat[i, j];

                bool step = true;

                for (int i = 0; i < bmInput.Width; i++)
                    for (int j = 0; j < bmInput.Height; j++)
                        if (tempImg[i, j] != currentImage[i, j])
                            step = false;

                if (step) break;

                for (int i = 0; i < bmInput.Width; i++)
                    for (int j = 0; j < bmInput.Height; j++)
                        tempImg[i, j] = currentImage[i, j];
            }

            for (int i = 0; i < bmInput.Width; i++)
                for (int j = 0; j < bmInput.Height; j++)
                {
                    if (!currentImage[i, j]) bmResult.SetPixel(i, j, Color.FromArgb(0, 0, 0));
                    else bmResult.SetPixel(i, j, Color.FromArgb(255, 255, 255));
                }

            return bmResult;
        }

        private static bool isSatisfy(int iter, bool[,] matIn)
        {
            int BP = 0;
            for (int i = 0; i < 3; i++) for (int j = 0; j < 3; j++) BP += Convert.ToInt32(matIn[i, j]);
            BP = BP - Convert.ToInt32(matIn[1, 1]);

            int SP = Convert.ToInt32((matIn[0, 1] == false && matIn[0, 2] == true)) + Convert.ToInt32((matIn[0, 2] == false && matIn[1, 2] == true)) +
                Convert.ToInt32((matIn[1, 2] == false && matIn[2, 2] == true)) + Convert.ToInt32((matIn[2, 2] == false && matIn[2, 1] == true)) +
                Convert.ToInt32((matIn[2, 1] == false && matIn[2, 0] == true)) + Convert.ToInt32((matIn[2, 0] == false && matIn[1, 0] == true)) +
                Convert.ToInt32((matIn[1, 0] == false && matIn[0, 0] == true)) + Convert.ToInt32((matIn[0, 0] == false && matIn[0, 1] == true));
            int m1 = 0;
            int m2 = 0;

            if (iter == 0)
            {
                m1 = Convert.ToInt32((matIn[1, 2] & matIn[0, 1] & matIn[1, 0]));
                m2 = Convert.ToInt32((matIn[0, 1] & matIn[2, 1] & matIn[1, 0]));
            }
            else
            {
                m1 = Convert.ToInt32((matIn[0, 1] & matIn[1, 2] & matIn[2, 1]));
                m2 = Convert.ToInt32((matIn[1, 2] & matIn[2, 1] & matIn[1, 0]));
            }

            if (SP == 1 && (BP >= 2 && BP <= 6) && m1 == 0 && m2 == 0)
                return false;
            else return true;
        }

        public static int GetFoot(Bitmap inp, string z)
        {
            int foot = 0;
            int HeightScan = (int)(0.5 * inp.Height);

            bool FootCandidate = false;
            bool isPrevTrue = false;
            for (int i = 0; i < inp.Width; i++)
            {
                for (int j = inp.Height - 1; j >= inp.Height - HeightScan; j--)
                {
                    FootCandidate = false;
                    if (inp.GetPixel(i, j).R == 255) // inp.GetPixel(i + 1, j).R == 255 || inp.GetPixel(i + 2, j).R == 255)
                    {
                        FootCandidate = true;
                        break;
                    }
                }

                if ((!isPrevTrue && FootCandidate))
                {
                    foot++;
                    isPrevTrue = true;
                }
                else if (!FootCandidate)
                {
                    isPrevTrue = false;
                }
            }

            return foot;
        }
    }
}
