using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Hanacaraka
{
    public partial class Form1 : Form
    {
        Bitmap inp, mod;
        public Form1()
        {
            InitializeComponent();
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            this.MaximizeBox = false;
            this.StartPosition = FormStartPosition.CenterScreen;
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
                mod = Preprocessing.Thinning(Preprocessing.GetBinaryImage(inp));
                pictureBox2.Image = mod;
            }
        }

        private void button3_Click(object sender, EventArgs e)
        {
            List<int> Border = Preprocessing.SegmentingImage(mod);
            Graphics g = Graphics.FromImage(mod);
            for (int i = 0; i < Border.Count - 3; i += 4)
            {
                g.DrawRectangle(new Pen(Color.FromArgb(255, 255, 255)), Border[i], -Border[i + 2], Border[i + 1] - Border[i], -Border[i + 3] / inp.Width - -Border[i + 2]);
                //if (Border[i] < -inp.Width) g.DrawLine(new System.Drawing.Pen(Color.FromArgb(255, 255, 255), 1), new Point(Border[i - 3], -Border[i] / inp.Width), new Point(Border[i - 2], -Border[i] / inp.Width));
                //else if (Border[i] < 0) g.DrawLine(new System.Drawing.Pen(Color.FromArgb(255, 255, 255), 1), new Point(Border[i - 2], -Border[i]), new Point(Border[i - 1], -Border[i]));
                //else g.DrawLine(new System.Drawing.Pen(Color.FromArgb(255, 255, 255), 1), new Point(Border[i], 0), new Point(Border[i], mod.Height));
            }
            pictureBox2.Image = mod;
        }
    }
}
