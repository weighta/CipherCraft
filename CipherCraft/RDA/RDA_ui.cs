using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;

namespace CipherCraft
{

    public class RDA_ui
    {
        public bool IDLE;


        public Frame FRAME;
        public Bitmap ALGO;
        public RDA_ui()
        {
            FRAME = new Frame(512, 512);
            //ALGO = new Bitmap("RDA\\cipher_sequence.png");
        }
        public void refresh()
        {
            FRAME.clear();

        }
        
    }
}
