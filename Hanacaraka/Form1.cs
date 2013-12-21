using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.IO;
using System.Media;
using System.Threading;

namespace Hanacaraka
{
    public partial class Form1 : Form
    {
        Bitmap inp, mod;
        List<Character> dataset, SegmentedChar;
        string StringResult;
        public Form1()
        {
            InitializeComponent();
            SoundPlayer sp = new SoundPlayer(@"Asset\MusikEntrance.wav");
            sp.Play();
            checkBox1.Checked = false;
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            this.MaximizeBox = false;
            this.StartPosition = FormStartPosition.CenterScreen;
            LoadDataSet();
            StringResult = "AhmadHayam";
            label2.Text = "Inputkan";
        }

        private void LoadDataSet()
        {
            dataset = new List<Character>();
            string parent = "D:\\Bluetooth\\caraka ds";
            string[] lines = File.ReadAllLines(parent + "\\data.txt");
            foreach (string line in lines)
            {
                string[] part = line.Split('|');
                Character cs = new Character(parent + "\\" + part[0]);
                cs.foot = Convert.ToInt32(part[1]);
                cs.roundPosition = Convert.ToInt32(part[2]);
                cs.numRegion = Convert.ToInt32(part[3]);
                cs.leftFoot = Convert.ToInt32(part[4]);
                cs.rightFoot = Convert.ToInt32(part[5]);
                dataset.Add(cs);
                //Console.WriteLine(dataset.Count + " " + dataset[dataset.Count - 1].name + " " + dataset[dataset.Count - 1].foot + " " + dataset[dataset.Count - 1].roundPosition);
            }

            //ds = new List<Bitmap>();
            //dsName = new List<string>();
            //string[] listFile = Directory.GetFiles(@"D:\Bluetooth\caraka ds\");
            //foreach (string s in listFile)
            //{
            //    if (Path.GetExtension(s) == ".bmp")
            //    {
            //        Bitmap t = new Bitmap(Image.FromFile(s));
            //        ds.Add(t);
            //        dsName.Add(s);
            //    }
            //}
        }

        private void button1_Click(object sender, EventArgs e)
        {
            OpenFileDialog ofd = new OpenFileDialog();
            ofd.ShowDialog();
            if (!String.IsNullOrEmpty(ofd.FileName))
            {
                inp = new Bitmap(Image.FromFile(ofd.FileName));
                pictureBox1.Image = inp;
            }
            label2.Text = "Inputkan";
        }

        private void button2_Click(object sender, EventArgs e)
        {
            if (inp != null)
            {
                SegmentedChar = new List<Character>();
                mod = Preprocessing.GetBinaryImage(inp);
                
                List<int> Square = Preprocessing.SegmentingImage(mod);
                string parent = "D:\\Bluetooth\\caraka ds";
                
                for (int z = 0; z < Square.Count - 3; z += 4)
                {
                    Bitmap tt = new Bitmap(Square[z + 2], Square[z + 3]);
                    Bitmap sandangan, karakter;
                
                    for (int i = Square[z]; i < Square[z] + Square[z + 2]; i++)
                        for (int j = Square[z + 1]; j < Square[z + 1] + Square[z + 3]; j++)
                            tt.SetPixel(i - Square[z], j - Square[z + 1], mod.GetPixel(i, j));

                    int sandanganBawah = 0;
                    int karakterAtas = 0;

                    bool prev = false;
                    bool onWhite = false;
                    for (int i = 0; i < tt.Height; i++)
                    {
                        for (int x = 0; x < tt.Width; x++)
                        {
                            onWhite = false;
                            if (tt.GetPixel(x, i).R == 255)
                            {
                                onWhite = true;
                                break;
                            }
                        }
                        if (!prev && onWhite) karakterAtas = i;
                        if (prev && !onWhite || i == tt.Width - 1 && sandanganBawah == 0) sandanganBawah = i;
                        prev = onWhite;
                    }

                    int kiriSandangan = 0, kananSandangan = 0;
                    prev = false;
                    onWhite = false;
                    for (int i = 0; i < tt.Width; i++)
                    {
                        for (int x = 0; x < sandanganBawah; x++)
                        {
                            onWhite = false;
                            if (tt.GetPixel(i, x).R == 255)
                            {
                                onWhite = true;
                                break;
                            }
                        }
                        if (!prev && onWhite) kiriSandangan= i;
                        if (prev && !onWhite || i == tt.Width - 1 && kananSandangan == 0) kananSandangan = i;
                        prev = onWhite;
                    }

                    int kiriKarakter = 0, kananKarakter = 0;
                    prev = false;
                    onWhite = false;
                    for (int i = 0; i < tt.Width; i++)
                    {
                        for (int x = karakterAtas; x < tt.Height; x++)
                        {
                            onWhite = false;
                            if (tt.GetPixel(i, x).R == 255)
                            {
                                onWhite = true;
                                break;
                            }
                        }
                        if (!prev && onWhite) kiriKarakter = i;
                        if (prev && !onWhite || i == tt.Width - 1 && kananKarakter == 0) kananKarakter= i;
                        prev = onWhite;
                    }

                    sandangan = new Bitmap(kananSandangan - kiriSandangan, sandanganBawah + 1);
                    for (int i = kiriSandangan; i < kananSandangan; i++)
                        for (int j = 0; j < sandanganBawah; j++)
                            sandangan.SetPixel(i - kiriSandangan, j, tt.GetPixel(i, j));
                    //sandangan.Save(z.ToString() + "sikat.jpg", System.Drawing.Imaging.ImageFormat.Jpeg);

                    karakter = new Bitmap(kananKarakter - kiriKarakter, tt.Height - karakterAtas + 1);
                    for (int i = kiriKarakter; i < kananKarakter; i++)
                        for (int j = karakterAtas; j < tt.Height; j++)
                            karakter.SetPixel(i - kiriKarakter, j - karakterAtas, tt.GetPixel(i, j));
                    //karakter.Save(z.ToString() + "sikat2.jpg", System.Drawing.Imaging.ImageFormat.Jpeg);

                    if (karakterAtas - sandanganBawah > 0)
                        tt = karakter;

                    Character cs = new Character();
                    cs.img = Preprocessing.Thinning(tt);
                    cs.name = "Un";
                    cs.foot = Preprocessing.GetFoot(cs.img);
                    cs.roundPosition = Preprocessing.isRound(cs.img);
                    cs.numRegion = Preprocessing.GetRegion(cs.img);
                    int[] aa = Preprocessing.GetRLFoot(cs.img);
                    cs.leftFoot = --aa[0];
                    cs.rightFoot = --aa[1];
                    Console.WriteLine(cs.foot + " " + cs.roundPosition + " " + cs.numRegion + " " + cs.leftFoot+ " " + cs.rightFoot);
                    SegmentedChar.Add(cs);
                    cs.img.Save(parent + "\\temp\\" + z.ToString() + ".bmp", System.Drawing.Imaging.ImageFormat.Bmp);
                }
            }
        }

        private double GetSimiliarity(Bitmap cand, Bitmap tester)
        {
            Bitmap testers = new Bitmap(tester, cand.Size);
            double tot = 0, curr = 0;

            for (int i = 0; i < cand.Height; i++)
                for (int j = 0; j < cand.Width; j++)
                {
                    if (cand.GetPixel(j, i).R == 255)
                    {
                        tot += 1;
                        if (testers.GetPixel(j, i).R == 255) curr += 1;
                    }
                }
            return curr / tot * 100;
        }

        private void bHasil_Click(object sender, EventArgs e)
        {
            List<string> result = new List<string>();
            for (int i = 0; i < SegmentedChar.Count; i++)
            {
                double max = -inp.Height * inp.Width;
                double mindistance = 10000;

                List<Character> candidateF = new List<Character>();
                List<Character> candidateH = new List<Character>();
                List<Character> candidateHP = new List<Character>();
                List<Character> candidateR = new List<Character>();
                List<Character> candidateLR = new List<Character>();
                List<Character> candidateFinal;

                for (int j = 0; j < dataset.Count; j++)
                    if (SegmentedChar[i].foot == dataset[j].foot)// && SegmentedChar[i].isRound() == dataset[j].isRound())
                        candidateF.Add(dataset[j]);

                for (int j = 0; j < candidateF.Count; j++)
                    if (candidateF[j].isRound() == SegmentedChar[i].isRound())
                        candidateH.Add(candidateF[j]);

                for (int j = 0; j < candidateH.Count; j++)
                {
                    if (candidateH[j].numRegion == SegmentedChar[i].numRegion)
                        candidateR.Add(candidateH[j]);
                }

                for (int j = 0; j < candidateR.Count; j++)
                {
                    if (candidateR[j].roundPosition == SegmentedChar[i].roundPosition)
                        candidateHP.Add(candidateR[j]);
                }

                for (int j = 0; j < candidateHP.Count; j++)
                {
                    if (candidateHP[j].rightFoot == SegmentedChar[i].rightFoot && candidateHP[j].leftFoot == SegmentedChar[i].leftFoot)
                        candidateLR.Add(candidateHP[j]);
                }

                
                Console.WriteLine(candidateF.Count);
                Console.WriteLine(candidateH.Count);
                Console.WriteLine(candidateHP.Count);
                Console.WriteLine(candidateR.Count);
                Console.WriteLine(candidateLR.Count);
                Console.WriteLine("------------------------");

                if (candidateLR.Count != 0) candidateFinal = candidateLR;
                else if (candidateHP.Count != 0) candidateFinal = candidateHP;
                else if (candidateR.Count != 0) candidateFinal = candidateR;
                else if (candidateH.Count != 0) candidateFinal = candidateH;
                else candidateFinal = candidateF;

                //<AdditionalChecking>
                bool isException = false;
                
                if (candidateFinal.Count == 2 && (candidateFinal[0].name.Equals("Ha") || candidateFinal[0].name.Equals("Ta")))
                {
                    int middle = (SegmentedChar[i].img.Width + 1) / 2;
                    int count = 0;
                    for (int y = 0; y < SegmentedChar[i].img.Height; y++)
                    {
                        if (SegmentedChar[i].img.GetPixel(middle, y).R == 255) count++;
                    }

                    if (count == 1) SegmentedChar[i].name = "Ha";
                    else SegmentedChar[i].name = "Ta";
                    isException = true;
                }

                else if (candidateFinal.Count == 2 && (candidateFinal[0].name.Equals("Ca") || candidateFinal[0].name.Equals("Sa")))
                {
                    int middle = (SegmentedChar[i].img.Width + 1) * 6 / 10;
                    int count = 0;
                    bool isPrev = false;
                    for (int y = 0; y < SegmentedChar[i].img.Height; y++)
                    {
                        if (SegmentedChar[i].img.GetPixel(middle, y).R == 255 && !isPrev)
                        {
                            count++;
                            isPrev = true;
                        }
                        else isPrev = false;
                    }

                    if (count == 1) SegmentedChar[i].name = "Sa";
                    else SegmentedChar[i].name = "Ca";
                    isException = true;
                }

                else if (candidateFinal.Count == 4 && (candidateFinal[0].name.Equals("Pa") || candidateFinal[0].name.Equals("Dha") || candidateFinal[0].name.Equals("Wa") || candidateFinal[0].name.Equals("Ma")))
                {
                    int middle = (SegmentedChar[i].img.Height + 1) * 8 / 10;
                    int count = 0;
                    bool isPrev = false;
                    for (int x = 0; x < SegmentedChar[i].img.Width; x++)
                    {
                        if (SegmentedChar[i].img.GetPixel(x, middle).R == 255 && !isPrev)
                        {
                            count++;
                            isPrev = true;
                        }
                        else isPrev = false;
                    }

                    if (count == 6) SegmentedChar[i].name = "Dha";
                    else
                    {
                        middle = (SegmentedChar[i].img.Width + 1) * 4 / 10;
                        count = 0;
                        isPrev = false;
                        for (int y = 0; y < SegmentedChar[i].img.Height; y++)
                        {
                            if (SegmentedChar[i].img.GetPixel(middle, y).R == 255 && !isPrev)
                            {
                                count++;
                                isPrev = true;
                            }
                            else isPrev = false;
                        }
                        if (count > 1) SegmentedChar[i].name = "Ma";
                        else
                        {
                            middle = (SegmentedChar[i].img.Width + 1) * 6 / 10;
                            count = 0;
                            isPrev = false;
                            for (int y = 0; y < SegmentedChar[i].img.Height; y++)
                            {
                                if (SegmentedChar[i].img.GetPixel(middle, y).R == 255 && !isPrev)
                                {
                                    count++;
                                    isPrev = true;
                                }
                                else isPrev = false;
                            }
                            if (count == 1) SegmentedChar[i].name = "Pa";
                            else SegmentedChar[i].name = "Wa";
                        }
                    }
                    isException = true;
                }
                //</AdditionalChecking>

                if (!isException)
                {
                    for (int j = 0; j < candidateFinal.Count; j++)
                    {
                        double temp = GetSimiliarity(SegmentedChar[i].img, candidateFinal[j].img);
                        double Wpattern = (double)(candidateFinal[j].img.Width * SegmentedChar[i].img.Height) / candidateFinal[j].img.Height;

                        if (max < temp && mindistance > Math.Abs(SegmentedChar[i].img.Width - Wpattern))
                        {
                            max = temp;
                            SegmentedChar[i].name = candidateFinal[j].name;
                            mindistance = Math.Abs(SegmentedChar[i].img.Width - Wpattern);
                        }
                    }
                }
                result.Add(Path.GetFileNameWithoutExtension(SegmentedChar[i].name));
            }

            label2.Text = "";
            for (int i = 0; i < result.Count; i++)
                label2.Text += result[i];

            if (checkBox1.Checked)
            {
                for (int i = 0; i < result.Count; i++)
                {
                    SoundPlayer sp = new SoundPlayer("Asset\\" + result[i] + ".wav");
                    sp.PlaySync();
                    //Thread.Sleep(300);
                }
            }
        }
    }
}
