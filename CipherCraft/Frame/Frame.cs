using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;

namespace CipherCraft
{
    public class Frame
    {
        public Bitmap BACKGROUND;


        Pen PEN;
        Fonts FONT;
        Graphics g;
        public Frame(int w, int h)
        {
            BACKGROUND = new Bitmap(w, h);
            FONT = new Fonts();
            g = Graphics.FromImage(BACKGROUND);
        }


        public void clear()
        {
            Brush backcolor = Brushes.Black;
            g.FillRectangle(backcolor, 0, 0, BACKGROUND.Width, BACKGROUND.Height);
        }
        public void DRAWTEXT(string tx, int x, int y)
        {
            g.DrawString(tx, FONT.FONT[0], Brushes.White, x, y);
        }
        public void DRAWTEXT(string tx, int x, int y, Brush c, int FONT_INDEX)
        {
            g.DrawString(tx, FONT.FONT[FONT_INDEX], c, x, y);
        }
        public void DRAWRECT(int X, int Y, int W, int H)
        {
            PEN.Color = Color.LightGray;
            g.DrawRectangle(PEN, X, Y, W, H);
        }
        public void DRAWRECT(int X, int Y, int W, int H, Color c)
        {
            PEN.Color = c;
            g.DrawRectangle(PEN, X, Y, W, H);
        }
    }
}
