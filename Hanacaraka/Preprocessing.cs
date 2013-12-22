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
        public class coord
        {
            public int x, y;
            public coord() { }
            public coord(int _x, int _y)
            {
                x = _x;
                y = _y;
            }
        }
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
                if (isThere && i < inp.Width)
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

        static public List<Bitmap> SegmentingImage(Bitmap inp, int dummy)
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
                if (isThere && i < inp.Width)
                {
                    res.Add(i);
                    i++;
                    bool isOnreg = true;
                    while (isOnreg && i < inp.Width)
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
                        if (j == inp.Height || i == inp.Width - 1)
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
            //Console.WriteLine("finalRest " + FinalRest.Count);
            res.Clear();
            List<Bitmap> result = new List<Bitmap>();
            for (int z = 0; z < FinalRest.Count - 3; z += 4)
            {
                Bitmap tt = new Bitmap(FinalRest[z + 2], FinalRest[z + 3]);

                for (int i = FinalRest[z]; i < FinalRest[z] + FinalRest[z + 2]; i++)
                    for (int j = FinalRest[z + 1]; j < FinalRest[z + 1] + FinalRest[z + 3]; j++)
                        tt.SetPixel(i - FinalRest[z], j - FinalRest[z + 1], inp.GetPixel(i, j));
                result.Add(tt);
            }

            return result;
        }

        ///----------------------------------------------------------------Convert to binary image--------------------------------------
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
                    if (res.GetPixel(i, j).R < threshold) res.SetPixel(i, j, Color.White);
                    else res.SetPixel(i, j, Color.Black);
                }
            }
            return res;
        }

        public static void PlotPath(Bitmap inp)
        {
            bool mod = false;
            coord c = new coord();
            for (int i = inp.Height - 1; i >= inp.Height * 1 / 10; i--)
            {
                for (int j = 0; j < inp.Width * 1 / 10; j++)
                {
                    if (inp.GetPixel(j, i).R == 255)
                    {
                        c.x = i;
                        c.y = j;
                        mod = true;
                        break;
                    }
                }
                if (mod) break;
            }
            bool[,] visit = new bool[inp.Height, inp.Width];
            bool start = true;
            int prev = 1;
            int count = 0;
            int maxCount = 0;
            int itBranch = 999;
            int branchCount = 0;
            int limit = 1;
            int hitungPath = 0;
            String result = "";
            coord temp = new coord();
            Queue<coord> branch = new Queue<coord>();
            for (int z = 0; z < 1000; z++)
            {
                itBranch++;
                hitungPath++;
                visit[c.x, c.y] = true;
                temp.x = c.x;
                temp.y = c.y;
                Queue<coord> neighbourList = new Queue<coord>();



                #region kanan_atas
                if (inp.GetPixel(temp.y + 1, temp.x - 1).R == 255 && !visit[temp.x - 1, temp.y + 1])
                {
                    if (inp.GetPixel(c.y, c.x - 1).R == 255)
                    {
                        visit[c.x - 1, c.y] = true; inp.SetPixel(c.y, c.x - 1, Color.Yellow);

                    }
                    if (inp.GetPixel(c.y + 1, c.x).R == 255)
                    {
                        visit[c.x, c.y + 1] = true; inp.SetPixel(c.y + 1, c.x, Color.Yellow);
                    }

                    coord neighbour = new coord();
                    neighbour.x = c.x - 1;
                    neighbour.y = c.y + 1;
                    neighbourList.Enqueue(neighbour);

                    if (maxCount < count) maxCount = count;
                    count = 0;

                    inp.SetPixel(c.y, c.x, Color.Red);
                }
                #endregion

                #region kanan_bawah
                if (inp.GetPixel(temp.y + 1, temp.x + 1).R == 255 && !visit[temp.x + 1, temp.y + 1])
                {
                    if (inp.GetPixel(c.y + 1, c.x).R == 255)
                    {
                        visit[c.x, c.y + 1] = true; inp.SetPixel(c.y + 1, c.x, Color.Yellow);
                    }
                    if (inp.GetPixel(c.y, c.x + 1).R == 255)
                    {
                        visit[c.x + 1, c.y] = true; inp.SetPixel(c.y, c.x + 1, Color.Yellow);
                    }

                    coord neighbour = new coord();
                    neighbour.x = c.x + 1;
                    neighbour.y = c.y + 1;
                    neighbourList.Enqueue(neighbour);

                    if (maxCount < count) maxCount = count;
                    count = 0;

                    inp.SetPixel(c.y, c.x, Color.Red);

                }
                #endregion

                #region kiri_bawah
                if (inp.GetPixel(temp.y - 1, temp.x + 1).R == 255 && !visit[temp.x + 1, temp.y - 1])
                {
                    if (inp.GetPixel(c.y, c.x + 1).R == 255)
                    {
                        visit[c.x + 1, c.y] = true; inp.SetPixel(c.y, c.x + 1, Color.Yellow);
                    }
                    if ( inp.GetPixel(c.y - 1, c.x).R == 255)
                    {
                        visit[c.x, c.y - 1] = true; inp.SetPixel(c.y - 1, c.x, Color.Yellow);
                    }
                    coord neighbour = new coord();
                    neighbour.x = c.x + 1;
                    neighbour.y = c.y - 1;
                    neighbourList.Enqueue(neighbour);

                    if (maxCount < count) maxCount = count;
                    count = 0;

                    inp.SetPixel(c.y, c.x, Color.Red);
                }
                #endregion

                #region kiri_atas
                if (inp.GetPixel(temp.y - 1, temp.x - 1).R == 255 && !visit[temp.x - 1, temp.y - 1])
                {
                    if (inp.GetPixel(c.y - 1, c.x).R == 255)
                    {
                        visit[c.x, c.y - 1] = true; inp.SetPixel(c.y - 1, c.x, Color.Yellow);
                    }
                    if (inp.GetPixel(c.y, c.x - 1).R == 255)
                    {
                        visit[c.x - 1, c.y] = true; inp.SetPixel(c.y, c.x - 1, Color.Yellow);
                    }

                    coord neighbour = new coord();
                    neighbour.x = c.x - 1;
                    neighbour.y = c.y - 1;
                    neighbourList.Enqueue(neighbour);

                    if (maxCount < count) maxCount = count;
                    count = 0;

                    inp.SetPixel(c.y, c.x, Color.Red);
                }
                #endregion

                #region atas
                if (inp.GetPixel(temp.y, temp.x - 1).R == 255 && !visit[temp.x - 1, temp.y])
                {
                    if (prev != 1 && maxCount > limit)
                    {
                        if (result[result.Length - 1] != prev.ToString()[prev.ToString().Length - 1])
                            result += prev;
                        count = 0;
                        maxCount = 0;
                    }
                    prev = 1;
                    count++;

                    coord neighbour = new coord();
                    neighbour.x = c.x - 1;
                    neighbour.y = c.y;
                    neighbourList.Enqueue(neighbour);

                    inp.SetPixel(c.y, c.x, Color.Red);
                }
                #endregion

                #region kanan
                if (inp.GetPixel(temp.y + 1, temp.x).R == 255 && !visit[temp.x, temp.y + 1])
                {
                    if (prev != 0 && maxCount > limit)
                    {
                        result += prev;
                        count = 0;
                        maxCount = 0;
                    }
                    prev = 0;
                    count++;

                    coord neighbour = new coord();
                    neighbour.x = c.x;
                    neighbour.y = c.y + 1;
                    neighbourList.Enqueue(neighbour);

                    inp.SetPixel(c.y, c.x, Color.Red);
                }
                #endregion

                #region bawah
                if (inp.GetPixel(temp.y, temp.x + 1).R == 255 && !visit[temp.x + 1, temp.y])
                {
                    if (prev != 3 && maxCount > limit)
                    {
                        result += prev;
                        count = 0;
                        maxCount = 0;
                    }
                    prev = 3;
                    count++;

                    coord neighbour = new coord();
                    neighbour.x = c.x + 1;
                    neighbour.y = c.y;
                    neighbourList.Enqueue(neighbour);

                    inp.SetPixel(c.y, c.x, Color.Red);
                }
                #endregion

                #region kiri
                if (inp.GetPixel(temp.y - 1, temp.x).R == 255 && !visit[temp.x, temp.y - 1])
                {
                    if (prev != 2 && maxCount > limit)
                    {
                        result += prev;
                        count = 0;
                        maxCount = 0;
                    }
                    prev = 2;
                    count++;

                    coord neighbour = new coord();
                    neighbour.x = c.x;
                    neighbour.y = c.y - 1;
                    neighbourList.Enqueue(neighbour);

                    inp.SetPixel(c.y, c.x, Color.Red);
                }
                #endregion

                if (neighbourList.Count == 0)
                {
                    if (branch.Count == 0) break;
                    else
                    {
                        c.x = branch.Peek().x;
                        c.y = branch.Peek().y;
                        branch.Dequeue();
                        hitungPath = 0;
                    }
                }
                else
                {
                    c.x = neighbourList.Peek().x;
                    c.y = neighbourList.Peek().y;
                    if (neighbourList.Count > 1)
                    {
                        neighbourList.Dequeue();
                        branch.Enqueue(neighbourList.Dequeue());
                        if (itBranch > 10)
                        {
                            itBranch = 0;
                            branchCount++;
                        }
                    }
                }
            }
            result += prev;

            String tmp = result;
            result = tmp[0].ToString();
            for (int i = 1; i < tmp.Length; i++)
                if (tmp[i] != tmp[i - 1])
                    result += tmp[i].ToString();

            Console.WriteLine("code: " + result);
            Console.WriteLine("jumlah cabang: " + branchCount);

        }

        static public int GetRegion(Bitmap inp)
        {
            int region = 0;
            bool[,] M = new bool[inp.Height, inp.Width];

            for (int i = 0; i < inp.Height; i++)
                for (int j = 0; j < inp.Width; j++)
                {
                    if (inp.GetPixel(j, i).R == 255) M[i, j] = true;
                    else M[i, j] = false;
                }

            for (int i = 0; i < inp.Height; i++)
                for (int j = 0; j < inp.Width; j++)
                {
                    if (M[i, j])
                    {
                        region++;
                        Queue<coord> waitingList = new Queue<coord>();
                        waitingList.Enqueue(new coord(i, j));
                        while (waitingList.Count > 0)
                        {
                            int xx = waitingList.Peek().x, yy = waitingList.Peek().y;
                            if (xx >= 0 && xx < inp.Height && yy >= 0 && yy < inp.Width && M[xx, yy] == true)
                            {
                                M[xx, yy] = false;
                                waitingList.Enqueue(new coord(xx + 1, yy));
                                waitingList.Enqueue(new coord(xx - 1, yy));
                                waitingList.Enqueue(new coord(xx, yy + 1));
                                waitingList.Enqueue(new coord(xx, yy - 1));

                                waitingList.Enqueue(new coord(xx + 1, yy + 1));
                                waitingList.Enqueue(new coord(xx + 1, yy - 1));
                                waitingList.Enqueue(new coord(xx - 1, yy + 1));
                                waitingList.Enqueue(new coord(xx - 1, yy - 1));
                            }
                            waitingList.Dequeue();
                        }
                        break;
                    }
                }
            
            return region;
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

        public static int[] GetRLFoot(Bitmap inp)
        {
            int[] foot = new int[2];
            int HeightScan = (int)(0.5 * inp.Height);

            bool FootCandidate = false;
            bool isPrevTrue = false;
            for (int i = (inp.Width + 1) / 2; i >= 0; i--)
            {
                for (int j = inp.Height - 1; j >= inp.Height - HeightScan; j--)
                {
                    FootCandidate = false;
                    if (inp.GetPixel(i, j).R == 255 || i == (inp.Width + 1) / 2)
                    {
                        FootCandidate = true;
                        break;
                    }
                }

                if ((!isPrevTrue && FootCandidate))
                {
                    foot[0]++;
                    isPrevTrue = true;
                }
                else if (!FootCandidate)
                {
                    isPrevTrue = false;
                }
            }

            for (int i = (inp.Width + 1) / 2; i < inp.Width; i++)
            {
                for (int j = inp.Height - 1; j >= inp.Height - HeightScan; j--)
                {
                    FootCandidate = false;
                    if (inp.GetPixel(i, j).R == 255 || i == (inp.Width + 1) / 2)
                    {
                        FootCandidate = true;
                        break;
                    }
                }

                if ((!isPrevTrue && FootCandidate))
                {
                    foot[1]++;
                    isPrevTrue = true;
                }
                else if (!FootCandidate)
                {
                    isPrevTrue = false;
                }
            }

            return foot;
        }

        public static int GetFoot(Bitmap inp)
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
                    if (inp.GetPixel(i, j).R == 255)
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

        public static int isRound(Bitmap inp)
        {
            Size sT = new Size((int)((float)inp.Width * 0.7), (int)((float)inp.Height * 0.7));
            Bitmap t = new Bitmap(inp, sT);
            bool[,] X = new bool[sT.Height, sT.Width];
            int[,] M = new int[sT.Height, sT.Width];

            for (int i = 0; i < sT.Height; i++)
                for (int j = 0; j < sT.Width; j++)
                    M[i, j] = t.GetPixel(j, i).R;

            FillTheMat(X, M, 0, 0, sT.Height, sT.Width);

            int roundPosition = 0;

            for (int i = 0; i < sT.Height; i++)
                for (int j = 0; j < sT.Width; j++)
                    if (M[i, j] == 0)
                        roundPosition = j < (int)((float)sT.Width / 2) ? 1 : 2;

            return roundPosition;
        }

        private static void FillTheMat(bool[,] X, int[,] M, int x, int y, int xP, int yP)
        {
            Queue<coord> waitingList = new Queue<coord>();
            waitingList.Enqueue(new coord(x, y));
            while (waitingList.Count > 0)
            {
                int xx = waitingList.Peek().x, yy = waitingList.Peek().y;
                if (xx >= 0 && xx < xP && yy >= 0 && yy < yP && X[xx, yy] == false && M[xx, yy] == 0)
                {
                    X[xx, yy] = true;
                    M[xx, yy] = 255;

                    waitingList.Enqueue(new coord(xx + 1, yy));
                    waitingList.Enqueue(new coord(xx - 1, yy));
                    waitingList.Enqueue(new coord(xx, yy + 1));
                    waitingList.Enqueue(new coord(xx, yy - 1));
                }
                waitingList.Dequeue();
            }
        }
    }
}
