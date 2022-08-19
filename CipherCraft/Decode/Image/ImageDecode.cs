using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;

namespace CipherCraft
{
    public abstract class ImageDecode
    {
        IRR irr = new IRR();
        public Bitmap btm;
        public string path;

        int numPix;
        int[] pFactNumPix;
        int[] avgRGB = new int[3];

        

        public ImageDecode()
        {

        }
        protected ImageDecode(string path)
        {
            this.path = path;
            btm = new Bitmap(path);
            facts();
        }
        void facts()
        {
            AVGRGB();
            pFactNumPix = irr.pFact(numPix);
        }
        void AVGRGB()
        {
            Color pix;
            numPix = btm.Width * btm.Height;
            for (int i = 0; i<btm.Width; i++) for (int j = 0; j < btm.Height; j++)
                {
                    pix = btm.GetPixel(i, j);
                    avgRGB[0] += pix.R;
                    avgRGB[1] += pix.G;
                    avgRGB[2] += pix.B;
                }

            avgRGB[0] /= numPix;
            avgRGB[1] /= numPix;
            avgRGB[2] /= numPix;
        }
        public abstract void design();
    }

}
