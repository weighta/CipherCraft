using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Drawing;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace CipherCraft
{
    public struct ThreadInfo
    {
        public int fieldIndex;
        public int threadID;
        public int[][] data;
        public int p;
        public int k;
    }
    public struct InverseMatEx
    {
        public bool inverse;
        public int poly_index;
        public int[] values;
    }

    public class MatrixGFeXtrap : GraphicalBase
    {
        public int[] fields;
        public int[][] data;
        public int numElements;
        public int threadCount;
        public const int invProgressTotal = 3;
        public int invProgress=0;

        public bool findsSolved;

        GF_P_N gfp_n;
        NumberSetGFDecode gfDecode;
        MatrixGraphics matGraphics = new MatrixGraphics();
        Thread threadTetra;
        public MatrixGFeXtrap(GF_P_N gfp_n, NumberSetGFDecode gfDecode, PictureBox pictureBox1) : base(pictureBox1)
        {
            this.gfp_n = gfp_n;
            this.gfDecode = gfDecode;
            tetra = new Tetrahedron();
            tetra.vec3s = new Vec3[5];
            threadTetra = new Thread(new ThreadStart(tetraLoop));
            bitmapFunction(0);
        }
        Tetrahedron tetra;
        public override void bitmapFunction(int x)
        {
            pictureBox1.Click += new EventHandler(pictureBoxClick);
            SetPictureBox();
            tetra.vec3s[0].x = 0;
            tetra.vec3s[0].y = 0;
            tetra.vec3s[0].z = 1; // top vert
            tetra.vec3s[1].x = 1;
            tetra.vec3s[1].y = 0;
            tetra.vec3s[1].z = 0; // front vert
            tetra.vec3s[2].x = -0.5;
            tetra.vec3s[2].y = 0.86;
            tetra.vec3s[2].z = 0; // back right vert
            tetra.vec3s[3].x = -0.5;
            tetra.vec3s[3].y = -0.86;
            tetra.vec3s[3].z = 0; // back left vert
            tetra.vec3s[4].x = 0; 
            tetra.vec3s[4].y = 0;
            tetra.vec3s[4].z = -1; // bottom vert
            int scale = 100;
            for (int i = 0; i < tetra.vec3s.Length; i++) //scale
            {
                tetra.vec3s[i].x *= scale;
                tetra.vec3s[i].y *= scale;
                tetra.vec3s[i].z *= scale;
            }
            threadTetra.Start();
        }
        public void tetraLoop()
        {
            int xCenter = bitmap.Width >> 1;
            int yCenter = bitmap.Height >> 1;
            double yawVelocity = 1;
            double rollVelocity = 1;
            double pitchVelocity = 1;

            int rolldeg = 0;
            int yawdeg = 0;
            int pitchdeg = 0;

            Tetrahedron tetraCurr = new Tetrahedron();
            tetraCurr.vec3s = new Vec3[5];


            Graphics g = Graphics.FromImage(bitmap);

            Brush whiteBrush = Brushes.White;
            Brush blackBrush = Brushes.Black;
            Pen whitePen = Pens.White;
            Pen blackPen = Pens.Black;

            Vec2[] toScreen = new Vec2[5];
            double DEPTH = 100;

            tetraCurr.Copy(tetra);
            matGraphics.rot(rolldeg, yawdeg, pitchdeg, tetraCurr.vec3s);
            matGraphics.trans(0, 0, 100, tetraCurr.vec3s);
            matGraphics.ConvertToScreen(tetraCurr.vec3s, toScreen, DEPTH, xCenter, yCenter);

            //Draw new
            g.DrawLine(whitePen, toScreen[0].x, toScreen[0].y, toScreen[1].x, toScreen[1].y);
            g.DrawLine(whitePen, toScreen[0].x, toScreen[0].y, toScreen[2].x, toScreen[2].y);
            g.DrawLine(whitePen, toScreen[0].x, toScreen[0].y, toScreen[3].x, toScreen[3].y);
            g.DrawLine(whitePen, toScreen[4].x, toScreen[4].y, toScreen[1].x, toScreen[1].y);
            g.DrawLine(whitePen, toScreen[4].x, toScreen[4].y, toScreen[2].x, toScreen[2].y);
            g.DrawLine(whitePen, toScreen[4].x, toScreen[4].y, toScreen[3].x, toScreen[3].y);
            g.DrawLine(whitePen, toScreen[1].x, toScreen[1].y, toScreen[2].x, toScreen[2].y);
            g.DrawLine(whitePen, toScreen[2].x, toScreen[2].y, toScreen[3].x, toScreen[3].y);
            g.DrawLine(whitePen, toScreen[3].x, toScreen[3].y, toScreen[1].x, toScreen[1].y);

            rolldeg++;
            yawdeg++;
            pitchdeg++;

            Thread.Sleep(10);
            while (true)
            {
                tetraCurr.Copy(tetra);
                matGraphics.rot(rolldeg, yawdeg, pitchdeg, tetraCurr.vec3s);
                matGraphics.trans(0, 0, 1000, tetraCurr.vec3s);
                matGraphics.ConvertToScreen(tetraCurr.vec3s, toScreen, DEPTH, xCenter, yCenter);

                //Cover up prev
                g.FillRectangle(blackBrush, 0, 0, bitmap.Width, bitmap.Height);

                //Draw new
                g.DrawLine(whitePen, toScreen[0].x, toScreen[0].y, toScreen[1].x, toScreen[1].y);
                g.DrawLine(whitePen, toScreen[0].x, toScreen[0].y, toScreen[2].x, toScreen[2].y);
                g.DrawLine(whitePen, toScreen[0].x, toScreen[0].y, toScreen[3].x, toScreen[3].y);
                g.DrawLine(whitePen, toScreen[4].x, toScreen[4].y, toScreen[1].x, toScreen[1].y);
                g.DrawLine(whitePen, toScreen[4].x, toScreen[4].y, toScreen[2].x, toScreen[2].y);
                g.DrawLine(whitePen, toScreen[4].x, toScreen[4].y, toScreen[3].x, toScreen[3].y);
                g.DrawLine(whitePen, toScreen[1].x, toScreen[1].y, toScreen[2].x, toScreen[2].y);
                g.DrawLine(whitePen, toScreen[2].x, toScreen[2].y, toScreen[3].x, toScreen[3].y);
                g.DrawLine(whitePen, toScreen[3].x, toScreen[3].y, toScreen[1].x, toScreen[1].y);
                for (int i = 0; i < gfDecode.RF.Length; i++)
                {
                    if (gfDecode.RF[i].p == 0) g.DrawString("(mod " + gfDecode.RF[i].n + ")", font, whiteBrush, toScreen[i].x, toScreen[i].y);
                    else if (gfDecode.RF[i].k == 1) g.DrawString("GF(" + gfDecode.RF[i].p + ")", font, whiteBrush, toScreen[i].x, toScreen[i].y);
                    else g.DrawString("GF(" + gfDecode.RF[i].p + "^" + gfDecode.RF[i].k + ")", font, whiteBrush, toScreen[i].x, toScreen[i].y);
                }
                pictureBox1.Image = bitmap;
                rolldeg++;
                yawdeg+=5;
                pitchdeg++;
                if (++rolldeg >= 360) rolldeg = 0;
                if (++yawdeg >= 360) yawdeg = 0;
                if (++pitchdeg >= 360) pitchdeg = 0;
                Thread.Sleep(20);
            }
        }
        public void pictureBoxClick(object sender, EventArgs e)
        {

        }

        public void SolveData(int[] fields, int[][] data, int threadCount)
        {
            this.fields = fields;
            this.data = data;
            this.threadCount = threadCount;
            numElements = data.Length * data[0].Length;
            SolveInv_threaded();
        }
        public void SolveForPolys(int threadID)
        {

        }
        void SolveInv_threaded()
        {
            SolvingProblem();
            Thread thread = new Thread(new ThreadStart(SolveInv));
            thread.Start();
            //SolveInv();
        }
        public void SolveInv()
        {
            invProgress = 0;
            setProgress1((int)(((float)invProgress / invProgressTotal) * 100));
            int[][] fieldDec = new int[fields.Length][];
            for (int i = 0; i < fieldDec.Length; i++)
            {
                fieldDec[i] = gfp_n.decField(fields[i]);
                WriteToLog("GF(" + fieldDec[i][0] + "^" + fieldDec[i][1] + ") " + fields[i] + " elements");
            }
            Thread[] threads = new Thread[threadCount];
            gfp_n.GaloisFieldMatINVFull_threaded_set(fields.Length, threadCount);
            ThreadInfo[] threadInfo = new ThreadInfo[threadCount];
            int numPolys = 0;
            for (int i = 0; i < fields.Length; i++)
            {
                int GF = fields[i];
                int p = fieldDec[i][0];
                int k = fieldDec[i][1];
                WriteToLog("Cracking GF(" + p + "^" + k + ") " + fields[i] + "...");
                gfp_n.IRRSet(GF);
                numPolys = gfp_n.irr_.Length;
                WriteToLog(numPolys + " over GF(" + p + " ^ " + k + ")");
                gfp_n.numInvMatSolved = 0;
                for (int j = 0; j < threadCount; j++)
                {
                    WriteToLog("init Thread[" + (j + 1) + "]...");
                    threads[j] = new Thread(new ParameterizedThreadStart(gfp_n.GaloisFieldMatInvFull_ExThreaded));
                    if (i == 0)
                    {
                        threadInfo[j] = new ThreadInfo();
                        threadInfo[j].data = data;
                        threadInfo[j].threadID = j;
                    }
                    threadInfo[j].fieldIndex = i;
                    threadInfo[j].p = p;
                    threadInfo[j].k = k;
                    threads[j].Start(threadInfo[j]);
                }
                ThreadsWait(threads, i, fields.Length, numPolys);
            }
            setProgress1((int)(((float)++invProgress / invProgressTotal) * 100)); //Cracking finished

            WriteToLog("Reducing...");
            char[][][][] chars = new char[fields.Length][][][]; //field, reductions, poly, chars
            for (int i = 0; i < chars.Length;) //fields
            {
                numPolys = 0;
                for (int j = 0; j < gfp_n.invMat[i].Length; j++) numPolys += gfp_n.invMat[i][j].Length;
                InverseMatEx[] invmat = new InverseMatEx[numPolys];
                int c = 0;
                for (int j = 0; j < gfp_n.invMat[i].Length; j++) //Go through threads
                {
                    for (int k = 0; k < gfp_n.invMat[i][j].Length; k++) //Go through thread length
                    {
                        invmat[c] = gfp_n.invMat[i][j][k]; //Assign every poly each thread got to full segment
                        c++;
                    }
                }
                chars[i] = new char[gfDecode.RF.Length][][];
                for (int j = 0; j < gfDecode.RF.Length;)
                {
                    if (gfDecode.RF[j].p != 0)
                    {
                        gfp_n.IRRSet(gfDecode.RF[j].n);
                        gfDecode.RF[j].irr = gfp_n.irr__[gfDecode.RF[j].irr_index];
                    }
                    chars[i][j] = new char[numPolys][]; //polys, chars
                    for (int k = 0; k < numPolys; k++)
                    {
                        if (invmat[k].inverse) chars[i][j][k] = gfDecode.reduce(invmat[k].values, j);
                        else chars[i][j][k] = new char[numElements];
                    }
                    setProgress3((int)(((float)++j / gfDecode.RF.Length) * 100));
                }
                WriteToLog("Reduced GF(" + fieldDec[i][0] + "^" + fieldDec[i][1] + ")");
                setProgress2((int)(((float)++i / chars.Length) * 100));
            }
            setProgress1((int)(((float)++invProgress / invProgressTotal) * 100)); //Reducing finished

            WriteToLog("Dictionary...");
            List<WordMatchGF>[][][] wordMatches = gfDecode.dict.dictionaryCheckGF_threaded(fields, chars, threadCount, gfp_n);
            WriteToLog("Done");
            string test = "";
            for (int i = 0; i < wordMatches.Length; i++) //field
            {
                test += "Finds for GF(" + fieldDec[i][0] + "^" + fieldDec[i][1] + ") " + fields[i] + " elements\n";
                for (int j = 0; j < wordMatches[i].Length; j++) //reduction
                {
                    if (gfDecode.RF[j].p == 0) test += "\tReducing with (mod "+ gfDecode.RF[j].n + "):\n";
                    else test += "\tReducing with GF(" + gfDecode.RF[j].p + "^" + gfDecode.RF[j].k + ") irr[" + gfDecode.RF[j].irr_index + "]: " + gfDecode.RF[j].irr + ":\n";
                    for (int k = 0; k < wordMatches[i][j].Length; k++) //polys
                    {
                        if (wordMatches[i][j][k].Count > 0)
                        {
                            test += "\t\tmat" + k + ":\n";
                            for (int l = 0; l < wordMatches[i][j][k].Count; l++)
                            {
                                test += "\t\t\t"+wordMatches[i][j][k][l].Word + " (" + (wordMatches[i][j][k][l].loc) + ")\n";
                            }
                        }
                    }
                }
            }
            setProgress1((int)(((float)++invProgress / invProgressTotal) * 100)); //Dictionary finished

            setFinishText(test);
            ProblemSolved();
        }
        private void ThreadsWait(Thread[] threads, int fieldIndex, int numFields, int numPolys)
        {
            setProgress2((int)(((float)fieldIndex / numFields) * 100));
            for (int i = 0; i < threads.Length; i++)
            {
                while (threads[i].IsAlive)
                {
                    setProgress3((int)(((float)gfp_n.numInvMatSolved / numPolys) * 100));
                    Thread.Sleep(100);
                }
            }
        }
    }
}
