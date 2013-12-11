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

namespace Hanacaraka
{
    public partial class Form1 : Form
    {
        Bitmap inp, mod;
        List<Bitmap> SegmentedChar, ds;
        string StringResult;
        List<string> dsName;
        public Form1()
        {
            InitializeComponent();
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            this.MaximizeBox = false;
            this.StartPosition = FormStartPosition.CenterScreen;
            LoadDataSet();
            StringResult = "AhmadHayam";
        }

        private void LoadDataSet()
        {
            ds = new List<Bitmap>();
            dsName = new List<string>();
            string[] listFile = Directory.GetFiles(@"D:\Bluetooth\caraka ds\");
            foreach (string s in listFile)
            {
                if (Path.GetExtension(s) == ".bmp")
                {
                    Bitmap t = new Bitmap(Image.FromFile(s));
                    ds.Add(t);
                    dsName.Add(s);
                }
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
        }

        private void button2_Click(object sender, EventArgs e)
        {
            if (inp != null)
            {
                SegmentedChar = new List<Bitmap>();
                mod = Preprocessing.GetBinaryImage(inp);
                pictureBox2.Image = mod;
                
                List<int> Square = Preprocessing.SegmentingImage(mod);
                
                for (int z = 0; z < Square.Count - 3; z += 4)
                {
                    Bitmap tt = new Bitmap(Square[z + 2], Square[z + 3]);
                
                    for (int i = Square[z]; i < Square[z] + Square[z + 2]; i++)
                        for (int j = Square[z + 1]; j < Square[z + 1] + Square[z + 3]; j++)
                            tt.SetPixel(i - Square[z], j - Square[z + 1], mod.GetPixel(i, j));

                    //Preprocessing.Thinning(tt).Save(@"D:\Sn.jpg", System.Drawing.Imaging.ImageFormat.Jpeg);
                    SegmentedChar.Add(Preprocessing.Thinning(tt));
                    SegmentedChar[SegmentedChar.Count - 1].Save(@"D:\Bluetooth\caraka ds\" + z.ToString() + z.ToString() + z.ToString() + ".jpg", System.Drawing.Imaging.ImageFormat.Jpeg);
                    Console.WriteLine(Preprocessing.GetFoot(SegmentedChar[SegmentedChar.Count - 1], @"D:\Bluetooth\caraka ds\" + z.ToString() + z.ToString() + z.ToString() + ".jpg"));
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
                    if (testers.GetPixel(j, i).R == 255)
                    {
                        tot += 1;
                        if (cand.GetPixel(j, i).R == 255) curr += 1;
                    }
                }
            return curr / tot * 100;
        }

        private void bHasil_Click(object sender, EventArgs e)
        {
            StringResult = "";
            for (int i = 0; i < SegmentedChar.Count; i++)
            {
                double max = -inp.Height * inp.Width;
                double mindistance = 10000;
                string name = "";
                for (int j = 0; j < ds.Count; j++)
                {
                    double temp = GetSimiliarity(SegmentedChar[i], ds[j]);
                    double Wpattern = (double)(ds[j].Width * SegmentedChar[i].Height) / ds[j].Height;

                    if (max < temp && mindistance > Math.Abs(SegmentedChar[i].Width - ds[j].Width))
                    {
                        max = temp;
                        name = dsName[j];
                        mindistance = Math.Abs(SegmentedChar[i].Width - ds[j].Width);
                    }
                }
                StringResult += Path.GetFileNameWithoutExtension(name);
                SegmentedChar[i].Save(@"D:\Bluetooth\caraka ds\" + i.ToString() + i.ToString() + ".jpg", System.Drawing.Imaging.ImageFormat.Jpeg);
            }
            Console.WriteLine(StringResult);
        }
    }
}
