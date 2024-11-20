using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;

namespace CipherCraft
{
    public class Fonts
    {
        public Font SANS;
        public Font CONSOLE;
        public Font[] FONT;


        public Fonts()
        {
            SANS = new Font("Microsoft Sans Serif", 12.0f, FontStyle.Bold);
            CONSOLE = new Font("Courier New", 12.0f, FontStyle.Bold);

            FONT = new Font[2];
            FONT[0] = SANS;
            FONT[1] = CONSOLE;
        }
    }
}
