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
        List<Sandangan>[] listSandangan;
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
            }
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
                
                List<Bitmap> Square = Preprocessing.SegmentingImage(mod, 0);
                listSandangan = new List<Sandangan>[Square.Count];
                Queue<Sandangan> temp = new Queue<Sandangan>();
                string parent = "D:\\Bluetooth\\caraka ds";

                for (int z = 0; z < Square.Count; z++)
                {
                    Bitmap sandangan, karakter;
                    List<Bitmap> sandangans = new List<Bitmap>(), karakters = new List<Bitmap>();
                    listSandangan[z] = new List<Sandangan>();
                
                    int sandanganBawah = 0;
                    int karakterAtas = 0;

                    bool prev = false;
                    bool onWhite = false;
                    for (int i = 0; i < Square[z].Height; i++)
                    {
                        for (int x = 0; x < Square[z].Width; x++)
                        {
                            onWhite = false;
                            if (Square[z].GetPixel(x, i).R == 255)
                            {
                                onWhite = true;
                                break;
                            }
                        }
                        if (!prev && onWhite) karakterAtas = i;
                        if (prev && !onWhite || i == Square[z].Width - 1 && sandanganBawah == 0) sandanganBawah = i;
                        prev = onWhite;
                    }

                    if (karakterAtas - sandanganBawah > 0)
                    {
                        sandangan = new Bitmap(Square[z].Width, sandanganBawah);
                        karakter = new Bitmap(Square[z].Width, Square[z].Height - karakterAtas);
                        for (int i = 0; i < Square[z].Width; i++)
                            for (int j = 0; j < sandanganBawah; j++)
                                sandangan.SetPixel(i, j, Square[z].GetPixel(i, j));
                        for (int i = 0; i < Square[z].Width; i++)
                            for (int j = karakterAtas; j < Square[z].Height; j++)
                                karakter.SetPixel(i, j - karakterAtas, Square[z].GetPixel(i, j));
                        sandangans = Preprocessing.SegmentingImage(sandangan, 0);
                        //Console.WriteLine(sandangans.Count);
                        karakters = Preprocessing.SegmentingImage(karakter, 0);
                        if (karakters.Count > 0) Square[z] = karakters[0];
                        else Square[z] = karakter;
                    }

                    Character cs = new Character();
                    cs.img = Preprocessing.Thinning(Square[z]);
                    cs.name = "Un";
                    cs.foot = Preprocessing.GetFoot(cs.img);
                    if (cs.foot == 1)
                    {
                        Sandangan s = new Sandangan(1);
                        s.img = cs.img;
                        s.setCharacteristic(Square[z].Size);
                        if (s.customHorizontalPoint >= 3)
                        {
                            s.name = "e";
                            temp.Enqueue(s);
                        }
                        else
                        {
                            s.name = "h";
                            listSandangan[SegmentedChar.Count - 1].Add(s);
                        }

                        //if (s.name == "h") listSandangan[SegmentedChar.Count - 1].Add(s);
                        //if (z == Square.Count - 1) listSandangan[SegmentedChar.Count - 1].Add(s);
                        //temp.Enqueue(s);
                        continue;
                    }
                    cs.roundPosition = Preprocessing.isRound(cs.img);
                    cs.numRegion = Preprocessing.GetRegion(cs.img);
                    int[] aa = Preprocessing.GetRLFoot(cs.img);
                    cs.leftFoot = --aa[0];
                    cs.rightFoot = --aa[1];
                    Console.WriteLine(cs.foot + " " + cs.roundPosition + " " + cs.numRegion + " " + cs.leftFoot+ " " + cs.rightFoot);
                    SegmentedChar.Add(cs);
                    cs.img.Save(parent + "\\temp\\" + z.ToString() + ".bmp", System.Drawing.Imaging.ImageFormat.Bmp);

                    if (temp.Count > 0)
                    {
                        //Console.Write("tricky detected");
                        //if (temp.Peek().name == "h") listSandangan[SegmentedChar.Count - 2].Add(temp.Peek());
                        //else listSandangan[SegmentedChar.Count - 1].Add(temp.Peek());
                        listSandangan[SegmentedChar.Count - 1].Add(temp.Peek());
                        temp.Dequeue();
                    }

                    for (int i = 0; i < sandangans.Count; i++)
                    {
                        Sandangan s = new Sandangan(0);
                        s.img = Preprocessing.Thinning(sandangans[i]);
                        s.setCharacteristic(Square[z].Size);
                        listSandangan[SegmentedChar.Count - 1].Add(s);
                        //Console.WriteLine("sandangan " + s.ratio + " " + s.horizontalPoint + " " + s.verticalPoint);
                        s.img.Save(parent + "\\temp\\" + z.ToString() + "sandangan " + i.ToString() + ".bmp", System.Drawing.Imaging.ImageFormat.Bmp);
                    }
                    //Square[z].Save(parent + "\\temp\\" + z.ToString() + ".bmp", System.Drawing.Imaging.ImageFormat.Bmp);
                }
            }

            for (int i = 0; i < SegmentedChar.Count; i++)
                Console.WriteLine(listSandangan[i].Count);
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
                    if (SegmentedChar[i].foot == dataset[j].foot)
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


                //Console.WriteLine(candidateF.Count);
                //Console.WriteLine(candidateH.Count);
                //Console.WriteLine(candidateHP.Count);
                //Console.WriteLine(candidateR.Count);
                //Console.WriteLine(candidateLR.Count);
                //Console.WriteLine("------------------------");

                if (candidateLR.Count != 0) candidateFinal = candidateLR;
                else if (candidateHP.Count != 0) candidateFinal = candidateHP;
                else if (candidateR.Count != 0) candidateFinal = candidateR;
                else if (candidateH.Count != 0) candidateFinal = candidateH;
                else candidateFinal = candidateF;

                //<AdditionalChecking>
                bool isException = false;
                
                if (candidateFinal.Count == 2 && (candidateFinal[0].name.Equals("Ha") || candidateFinal[0].name.Equals("Ta")))
                {
                    int middle = (int)((float)(SegmentedChar[i].img.Width) * 0.4);
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

                for (int c = 0; c < listSandangan[i].Count; c++)
                {
                    if (listSandangan[i][c].position == 0)
                    {
                        if (listSandangan[i][c].horizontalPoint == 1 && listSandangan[i][c].verticalPoint == 1) listSandangan[i][c].name = "r";
                        else if (listSandangan[i][c].ratio > 0.1667f && listSandangan[i][c].horizontalPoint == 2) listSandangan[i][c].name = "é";
                        else if (listSandangan[i][c].horizontalPoint == 2) listSandangan[i][c].name = "i";
                        else listSandangan[i][c].name = "ng";
                        Console.Write(listSandangan[i][c].name + " ");
                    }
                }
                Console.Write("\n");

                result.Add(Path.GetFileNameWithoutExtension(SegmentedChar[i].name));
            }

            //<Concating character and sandangan>
            for (int x = 0; x < SegmentedChar.Count; x++)
            {
                bool haveVocal = false;
                bool haveConsonant = false;
                bool haveDouble = false;
                if (listSandangan[x].Count > 0 && listSandangan[x][0].name == "e" && listSandangan[x][listSandangan[x].Count - 1].name == "h")
                {
                    StringBuilder name = new StringBuilder(result[x]);
                    name[name.Length - 1] = 'o';
                    result[x] = name.ToString();
                    haveVocal = true;
                    haveDouble = true;
                }
                for (int c = 0; c < listSandangan[x].Count; c++)
                {
                    if ((listSandangan[x][c].name == "i" || listSandangan[x][c].name == "é" || listSandangan[x][c].name == "e") && !haveVocal)
                    {
                        StringBuilder name = new StringBuilder(result[x]);
                        name[name.Length - 1] = Convert.ToChar(listSandangan[x][c].name);
                        result[x] = name.ToString();
                        haveVocal = true;
                    }
                    if ((listSandangan[x][c].name == "ng" || listSandangan[x][c].name == "r") && !haveConsonant)
                    {
                        result[x] += listSandangan[x][c].name;
                        haveConsonant = true;
                    }
                    if (listSandangan[x][c].name == "h" && !haveDouble)
                    {
                        result[x] += listSandangan[x][c].name;
                        haveConsonant = true;
                    }
                }
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
                }
            }
        }
    }
}
