using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;
using System.Windows.Forms;

///This class was written to draw in the meaning behind color
///and understanding the context the color is used in

namespace CipherCraft
{
    public class Color_Decode
    {
        //Daniel Skrok and Interaction Design Foundation, CC BY-SA 3.0
        ///https://www.interaction-design.org/literature/topics/color-symbolism
        private string[] meanings = { "Sophistication, Formality, Sorrow, Boldness, Elegance, Death, Mystery",
        "Purity, Simplicity, Innocence, Peace, Cleanliness, Emptiness, Goodness",
        "Trust, Calm, Sadness, Peace, Loyalty, Depth, Authenticity",
        "Nature, Growth, Wealth, Luck, Envy, Freshness, Quality",
        "Optimism, Cheer, Happiness, Warmth, Caution, Energy, Intellect",
        "Warmth, Creativity, Adventure, Freshness, Happiness, Attraction, Success",
        "Love, Passion, Strength, Power, Danger, Excitement, Energy",
        "Creativity, royalty, wealth, power, mystery, intrigue, nobility"
        }; //black, white, blue, green, yellow, orange, red

        private string[] colorNames;

        public Color[] colors;
        public int[] score;
        private Brush[] brush;
        private Brush exactBrush;

        public Label colorTitle;
        public Label descLabel;
        public Panel colorPanel;
        public PictureBox pictureBox1;
        public PictureBox pictureBox2;

        const int DEF_NORMSIZE = 128;
        public Color color;
        public int index = -1;
        public Bitmap inputImage;
        public Bitmap closeImage;

        public Color_Decode()
        {
            brush = new Brush[] { Brushes.Black, Brushes.White, Brushes.Blue, Brushes.Green, Brushes.Yellow, Brushes.Orange, Brushes.Red, Brushes.Purple };
            colorNames = new string[8] { "Black", "White", "Blue", "Green", "Yellow", "Orange", "Red", "Purple" };
            colors = new Color[8] { Color.Black, Color.White, Color.Blue, Color.Green, Color.Yellow, Color.Orange, Color.Red, Color.Purple };
            score = new int[colors.Length];
            
            inputImage = new Bitmap(DEF_NORMSIZE, DEF_NORMSIZE);
            closeImage = new Bitmap(DEF_NORMSIZE, DEF_NORMSIZE);



            Font font = new Font("Arial", 24, FontStyle.Bold);
            Font font1 = new Font("Arial", 18, FontStyle.Regular, GraphicsUnit.Pixel);
            colorPanel = new Panel() { Width = 756, Height = 512, AutoScroll = true, BorderStyle = BorderStyle.FixedSingle, Location = new Point(256, 12) };
            colorTitle = new Label() { Font = font, Width = DEF_NORMSIZE, Height = 64, Location = new Point(12, 12) };

            pictureBox1 = new PictureBox() { Width = DEF_NORMSIZE, Height = DEF_NORMSIZE, Location = new Point(colorTitle.Location.X, colorTitle.Location.Y + 64), Image = inputImage };
            pictureBox2 = new PictureBox() { Width = DEF_NORMSIZE, Height = DEF_NORMSIZE, Location = new Point(pictureBox1.Location.X, (pictureBox1.Location.Y + pictureBox1.Height) + 16), Image = closeImage };

            descLabel = new Label() { Width = DEF_NORMSIZE << 1, Font = font1, Location = new Point(pictureBox2.Location.X, pictureBox2.Location.Y + pictureBox2.Height + 12) };

            colorPanel.Controls.Add(colorTitle);
            colorPanel.Controls.Add(pictureBox1);
            colorPanel.Controls.Add(pictureBox2);
            colorPanel.Controls.Add(descLabel);
        }
        public void Decode(Color c)
        {
            int blacknwhiteDiff = 10;
            index = -1;
            color = c;

            int whiteDiff = 255 - blacknwhiteDiff;
            if (color.R - blacknwhiteDiff > whiteDiff && color.G - blacknwhiteDiff > whiteDiff && color.B - blacknwhiteDiff > whiteDiff)
            {
                index = 1;
            }
            else if (color.R < blacknwhiteDiff && color.G < blacknwhiteDiff && color.B < blacknwhiteDiff)
            {
                index = 0;
            }
            else
            {
                float scale = 0.2f;
                Color temp = Color.FromArgb((int)(scale * color.R), (int)(scale * color.G), (int)(scale * color.B));

                for (int i = 0; i < score.Length; i++) score[i] = 0;
                int bestScore = 0xFFFF;
                for (int i = 2; i < colors.Length; i++)
                {
                    score[i] += Math.Abs((int)(scale * colors[i].R) - temp.R);
                    score[i] += Math.Abs((int)(scale * colors[i].G) - temp.G);
                    score[i] += Math.Abs((int)(scale * colors[i].B) - temp.B);
                }
                for (int i = 2; i < score.Length; i++)
                {
                    if (score[i] < bestScore)
                    {
                        bestScore = score[i];
                        index = i;
                    }
                }
            }
            if (index == -1) throw new Exception("Color unregistered");

            colorTitle.Text = colorNames[index];
            descLabel.Text = meanings[index];

            Graphics g = Graphics.FromImage(closeImage);
            Brush exactBrush = new SolidBrush(color);
            g.FillRectangle(brush[index], 0, 0, DEF_NORMSIZE, DEF_NORMSIZE);
            g = Graphics.FromImage(inputImage);
            g.FillRectangle(exactBrush, 0, 0, DEF_NORMSIZE, DEF_NORMSIZE);

            //MessageBox.Show(p.Controls.Count + "");
        }
    }
}
