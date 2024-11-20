using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;
using System.Threading;
using System.Windows.Forms;

namespace CipherCraft
{
    public abstract class GraphicalBase
    {
        private string log;
        private string finishText;
        public Bitmap bitmap;
        public RichTextBox controlToPrintTo;
        private bool problemSolving=true;
        private bool progress1Available;
        private bool progress2Available;
        private bool progress3Available;
        private bool logAvailable;
        private int progress1=0;
        private int progress2=0;
        private int progress3=0;
        public PictureBox pictureBox1;
        public Font font = new Font("Arial", 10, FontStyle.Bold);
        public GraphicalBase()
        {

        }
        public GraphicalBase(PictureBox pictureBox1)
        {
            bitmap = new Bitmap(pictureBox1.Width, pictureBox1.Height);
            this.pictureBox1 = pictureBox1;
        }
        public void updateProgress(int x)
        {
            progress1 = x;
        }
        public void setProgress1(int a)
        {
            progress1 = a;
            progress1Available = true;
        }
        public void setProgress2(int a)
        {
            progress2 = a;
            progress2Available = true;
        }
        public void setProgress3(int a)
        {
            progress3 = a;
            progress3Available = true;
        }
        public bool isProgress1Available()
        {
            if (progress1Available)
            {
                progress1Available = false;
                return true;
            }
            return false;
        }
        public bool isProgress2Available()
        {
            if (progress2Available)
            {
                progress2Available = false;
                return true;
            }
            return false;
        }
        public bool isProgress3Available()
        {
            if (progress3Available)
            {
                progress3Available = false;
                return true;
            }
            return false;
        }
        public int getProgress1() { return progress1; }
        public int getProgress2() { return progress2; }
        public int getProgress3() { return progress3; }

        public void ClearLog()
        {
            log = "";
        }
        public bool isLogAvailable()
        {
            if (logAvailable)
            {
                logAvailable = false;
                return true;
            }
            return false;
        }
        public string getLog() { return log; }
        public void WriteToLog(string a)
        {
            log += a + "\n";
            logAvailable = true;
            Thread.Sleep(100);
        }
        public void WriteToLog_n(string a)
        {
            log += a;
            logAvailable = true;
            Thread.Sleep(100);
        }
        public void SolvingProblem()
        {
            problemSolving = true;
        }
        public void ProblemSolved()
        {
            problemSolving = false;
        }
        public bool isProblemSolved() { return !problemSolving; }
        public string getFinishText() { return finishText; }

        public void setFinishText(string a)
        {
            finishText = a;
        }
        public void SetPictureBox()
        {
            pictureBox1.Image = bitmap;
        }
        public abstract void bitmapFunction(int x);
    }
}
