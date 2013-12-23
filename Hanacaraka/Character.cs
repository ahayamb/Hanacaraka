using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;
using System.IO;

namespace Hanacaraka
{
    class Character
    {
        public Bitmap img;
        public bool isRound()
        {
            if (roundPosition == 1 || roundPosition == 2) return true;
            else return false;
        }
        public int roundPosition;
        public string name;
        public int foot;
        public int numRegion;
        public int rightFoot, leftFoot;
        public Character(string path)
        {
            img = new Bitmap(Image.FromFile(path));
            name = Path.GetFileNameWithoutExtension(path);
        }
        public Character()
        {
        }
    }
}
