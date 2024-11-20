using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Windows.Forms;
using System.Reflection.Emit;
using System.Threading;
using System.Security.Policy;

namespace CipherCraft
{
    public class GF_P_N
    {
        Form3 form = new Form3();

        Random ran = new Random();
        NBase nb = new NBase();
        PolyMul PolyM = new PolyMul();
        PolyDiv PolyD = new PolyDiv();
        IRR irr = new IRR();
        Matrix mat = new Matrix();

        
        int n;
        int[] mulSub;
        int[] addSub;
        int[] subSub;


        int[][] mulSub_threaded;
        int[][] addSub_threaded;
        int[][] subSub_threaded;

        private int[] primes;
        public int[][] irr_;
        public int[] irr__; //p_k
        public int reduciblesNeeded;
        public int reduciblesSolved;

        public InverseMatEx[][][] invMat;
        public int numInvMatSolved;

        public GF_P_N()
        {
            primes = irr.getPrimes(0, 25000);
        }

        int[][] ReduciblePolys;
        struct SolveReduciblePolysStruct
        {
            public int p_k;
        }
        public void SolveReduciblePolys(object e)
        {
            SolveReduciblePolysStruct s = (SolveReduciblePolysStruct)e;
            SolveReduciblePolys(s.p_k);
        }
        public void SolveReduciblePolys(int p_k)
        {
            int[] pandk = decField(p_k);
            int p = pandk[0];
            int deg = pandk[1];
            int numTerms = deg + 1;
            int size = 0;
            for (int i = 1; i <= deg >> 1; i++)
            {
                for (int j = (int)Math.Pow(p, i); j < (int)Math.Pow(p, i + 1); j++)
                {
                    size += (int)Math.Pow(p, deg - i + 1) - (int)Math.Pow(p, deg - i);
                }
            }
            reduciblesNeeded = size;
            ReduciblePolys = new int[size][];
            for (int i = 0; i < ReduciblePolys.Length; i++) ReduciblePolys[i] = new int[numTerms];

            int index = 0;
            for (int i = 1; i <= deg >> 1; i++)
            {
                for (int j = (int)Math.Pow(p, i); j < (int)Math.Pow(p, i + 1); j++)
                {
                    for (int k = (int)Math.Pow(p, deg - i); k < (int)Math.Pow(p, deg - i + 1); k++)
                    {
                        nb.rep(j, p, 0);
                        nb.rep(k, p, 1);
                        PolyM.MUL(nb.intArr_rep[0], nb.intArr_rep[1], p);
                        for (int l = 0; l < numTerms; l++)
                        {
                            ReduciblePolys[index][l] = PolyM.MUL_arr.intArr[l];
                        }
                        index++;
                        reduciblesSolved = index;
                    }
                }
            }
        }
        public void SolveReduciblePolys_wait(Thread thread)
        {
            while (thread.IsAlive)
            {
                form.label1.Text = reduciblesSolved + "/" + reduciblesNeeded;
                form.progressBar1.Value = (int)(((float)reduciblesSolved / reduciblesNeeded) * 100);
                Thread.Sleep(100);
            }
        }

        public bool IRR(int[] a, int p) //i think this works now
        {
            int deg = a.Length - 1;
            int p_k = nb.pow(p, deg);
            for (int i = 1; i <= deg >> 1; i++)
            {
                //Print.say(i + " " + (deg - i));
                for (int j = (int)Math.Pow(p, i); j < (int)Math.Pow(p, i + 1); j++)
                {
                    for (int k = (int)Math.Pow(p, deg - i); k < (int)Math.Pow(p, deg - i + 1); k++)
                    {
                        nb.rep(j, p, 0);
                        nb.rep(k, p, 1);
                        //MessageBox.Show(Print.ARR_TO_STR(nb.intArr_rep[0].intArr) + "\n" + Print.ARR_TO_STR(nb.intArr_rep[1].intArr) + "\n a and b");
                        PolyM.MUL(nb.intArr_rep[0], nb.intArr_rep[1], p);
                        if (nb.equal(PolyM.MUL_arr, a))
                        {
                            return false;
                        }
                    }
                }
            }

            //Print.say(p_k);
            //Print.say(Print.ARR_TO_STR(nb.rep(i, p)) + " * " + Print.ARR_TO_STR(nb.rep(j, p)) + " = \n" + Print.ARR_TO_STR(PolyM.MUL(nb.rep(i, p), nb.rep(j, p))) + "\n" + Print.ARR_TO_STR(a));
            //if (nb.equal(PolyM.MUL(nb.rep(i, p), nb.rep(j, p), p), a))
            //{
            //    //MessageBox.Show("the candidate irreducible is not primitive because " + i + " x " + j + " = " + p + "\nwhich would be\n" + Print.ARR_TO_STR(nb.rep(i, p)) + " x " + Print.ARR_TO_STR(nb.rep(j, p)) + " = " + Print.ARR_TO_STR(PolyM.MUL(nb.rep(i, p), nb.rep(j, p))));
            //    return false;
            //}
            return true;
        }         

        public int[][] IRR(int a) //Get all irreducibles of GF(a)
        {
            int[] p_k = new int[2];
            decField(a, ref p_k);
            string dir = @"GF\IRR\" + p_k[0] + "\\" + p_k[1] + ".txt";
            if (File.Exists(dir))
            {
                int[][] irrs = Print.strToNumArray(File.ReadAllLines(dir));
                irr__ = new int[irrs.Length];
                for (int i = 0; i < irr__.Length; i++) irr__[i] = nb.sum(irrs[i], p_k[0]);
                return irrs;
            }
            form.Show();
            SolveReduciblePolysStruct solveReduciblePolysStruct = new SolveReduciblePolysStruct();
            solveReduciblePolysStruct.p_k = a;
            Thread thread = new Thread(new ParameterizedThreadStart(SolveReduciblePolys));
            thread.Start(solveReduciblePolysStruct);
            SolveReduciblePolys_wait(thread);
            List<int[]> irr = new List<int[]>();
            for (int i = a; i < (int)Math.Pow(p_k[0], p_k[1]) << 1; i++)
            {
                int[] b = nb.rep(i, p_k[0]);
                if (!nb.exists(ReduciblePolys, b))
                {
                    irr.Add(b);
                }
            }
            Directory.CreateDirectory(@"GF\IRR\" + p_k[0] + "\\");
            int[][] ret = Print.ListToARR(irr);
            File.WriteAllLines(@"GF\IRR\" + p_k[0] + "\\" + p_k[1] + ".txt", Print.intARRARRtoStrARR(ret));
            form.Hide();
            return IRR(a);
        }

        public int add(int a, int b, int p)
        {
            return nb.add(a, b, p);
        }
        public int sub(int a, int b, int p)
        {
            return nb.sub(a, b, p);
        }
        public int mul(int p, int k, int a, int b, int IRR)
        {
            int[] PROD = PolyM.MUL(nb.rep(a, p), nb.rep(b, p)); //mul

            //string db = "MUL: " + Print.ARR_TO_STR(PROD) + "\n";

            for (int i = 0; i < PROD.Length; i++) PROD[i] %= p; //coef mod

           //db += "REDUCE: " + Print.ARR_TO_STR(PROD) + "\n";
            
            //db += "REM FIX: " + Print.ARR_TO_STR(ret);

            //MessageBox.Show(db);

            return reduce(PROD, p, k, IRR); //otherwise dont reduce
        }

        public int mulinv(int a)
        {
            for (int i = 1; i < 256; i++)
            {
                if (mulFast(a, i) == 1)
                {
                    return i;
                }
            }
            return 0;
        }

        public void addFastMake(int p, int k)
        {
            Console.WriteLine("add table " + p + "^" + k);
            //form.label1.Text = "Lookup Table for GF(" + p + "^" + k + ")";
            //form.Text = form.label1.Text;
            string path = @"GF\P_N\ADD\" + p;
            int n = pow(p, k);
            int[] t = new int[pow(n, 2)];

            //form.Show();
            for (int i = 0; i < n; i++)
            {
                //form.progressBar1.Value = (int)(((float)i / n) * 100.0);
                for (int j = 0; j < n; j++)
                {
                    t[(i * n) + j] = add(i, j, p);
                }
            }
            //form.Hide();
            if (!Directory.Exists(path)) Directory.CreateDirectory(path);
            File.WriteAllBytes(path + "\\" + k.ToString("X"), Print.IntARRToByteARR(t));
        }
        public void subFastMake(int p, int k)
        {
            Console.WriteLine("subtraction table " + p + "^" + k);
            //form.label1.Text = "Lookup Table for GF(" + p + "^" + k + ")";
            //form.Text = form.label1.Text;
            string path = @"GF\P_N\SUB\" + p;
            int n = pow(p, k);
            int[] t = new int[pow(n, 2)];

            //form.Show();
            for (int i = 0; i < n; i++)
            {
                //form.progressBar1.Value = (int)(((float)i / n) * 100.0);
                for (int j = 0; j < n; j++)
                {
                    t[(i * n) + j] = sub(i, j, p);
                }
            }
            //form.Hide();
            if (!Directory.Exists(path)) Directory.CreateDirectory(path);
            File.WriteAllBytes(path + "\\" + k.ToString("X"), Print.IntARRToByteARR(t));
        }
        public void mulFastMake(int p, int k, int IRR)
        {
            //form.label1.Text = "Lookup Table for GF(" + p + "^" + k + ") irr(" + IRR + ") = " + Print.ARR_TO_STR(nb.rep(IRR, p));
            //form.Text = form.label1.Text;
            Console.WriteLine("mul table " + p + "^" + k + " with irr " + IRR);
            string path = @"GF\P_N\" + p + "\\" + k;
            int n = pow(p, k);
            int[] t = new int[pow(n, 2)];
            //form.Show();
            for (int i = 0; i < n; i++)
            {
                //form.progressBar1.Value = (int)(((float)i / n) * 100.0);
                for (int j = 0; j < n; j++)
                {
                    t[(i * n) + j] = mul(p, k, i, j, IRR);
                }
            }
            //form.Hide();
            if (!Directory.Exists(path)) Directory.CreateDirectory(path);
            File.WriteAllBytes(path + "\\" + IRR.ToString("X"), Print.IntARRToByteARR(t));

        }

        public void mulFastSet(int p, int k, int IRR)
        {
            addFastSet(p, k);
            string path = @"GF\P_N\" + p + "\\" + k + "\\" + IRR.ToString("X");
            if (File.Exists(path))
            {
                mulSub = Print.ByteARRToIntARR(File.ReadAllBytes(path));
            }
            else
            {
                mulFastMake(p, k, IRR);
                mulFastSet(p, k, IRR);
            }
            n = pow(p, k);
        }
        public void addFastSet(int p, int k)
        {
            string path = @"GF\P_N\ADD\" + p + "\\" + k.ToString("X");
            if (File.Exists(path))
            {
                addSub = Print.ByteARRToIntARR(File.ReadAllBytes(path));
            }
            else
            {
                addFastMake(p, k);
                addFastSet(p, k);
            }
        }
        public void subFastSet(int p, int k)
        {
            string path = @"GF\P_N\SUB\" + p + "\\" + k.ToString("X");
            if (File.Exists(path))
            {
                subSub = Print.ByteARRToIntARR(File.ReadAllBytes(path));
            }
            else
            {
                subFastMake(p, k);
                subFastSet(p, k);
            }
        }

        public void mulFastSet_threaded(int p, int k, int IRR, int threadID)
        {
            addFastSet_threaded(p, k, threadID);
            string path = @"GF\P_N\" + p + "\\" + k + "\\" + IRR.ToString("X");
            if (File.Exists(path))
            {
                mulSub_threaded[threadID] = Print.ByteARRToIntARR(File.ReadAllBytes(path));
            }
            else
            {
                mulFastMake(p, k, IRR);
                mulFastSet_threaded(p, k, IRR, threadID);
            }
            n = pow(p, k);
        }
        public void addFastSet_threaded(int p, int k, int threadID)
        {
            string path = @"GF\P_N\ADD\" + p + "\\" + k.ToString("X");
            if (File.Exists(path))
            {
                addSub_threaded[threadID] = Print.ByteARRToIntARR(File.ReadAllBytes(path));
            }
            else
            {
                addFastMake(p, k);
                addFastSet_threaded(p, k, threadID);
            }
        }
        public void subFastSet_threaded(int p, int k, int threadID)
        {
            string path = @"GF\P_N\SUB\" + p + "\\" + k.ToString("X");
            if (File.Exists(path))
            {
                subSub_threaded[threadID] = Print.ByteARRToIntARR(File.ReadAllBytes(path));
            }
            else
            {
                subFastMake(p, k);
                subFastSet_threaded(p, k, threadID);
            }
        }

        //RUN mulFastSet before use!
        public int mulFast(int a, int b)
        {
            return mulSub[(a * n) + b];
        }
        public int addFast(int a, int b)
        {
            return addSub[(a * n) + b];
        }
        public int subFast(int a, int b)
        {
            return subSub[(a * n) + b];
        }

        public int mulFast_threaded(int a, int b, int threadID)
        {
            return mulSub_threaded[threadID][(a * n) + b];
        }
        public int addFast_threaded(int a, int b, int threadID)
        {
            return addSub_threaded[threadID][(a * n) + b];
        }
        public int subFast_threaded(int a, int b, int threadID)
        {
            return subSub_threaded[threadID][(a * n) + b];
        }

        public int pow(int a, int b)
        {
            int aa = a;
            if (b == 0) a = 1;
            for (int i = 0; i < b - 1; i++)
            {
                a *= aa;
            }
            return a;
        }
        public long pow(long a, long b)
        {
            long aa = a;
            if (b == 0) a = 1;
            for (int i = 0; i < b - 1; i++)
            {
                a *= aa;
            }
            return a;
        }
        
        public int[][] GaloisMatMul(int[][] a, int[][] b, int p, int k, int irreducible)
        {
            //MessageBox.Show(Print.ARR_TO_STR(a) + "\n and b is \n" + Print.ARR_TO_STR(b));
            if (a[0].Length != b.Length)
            {
                throw new Exception("Matrix A's columns not equal to number of rows in matrix b");
            }
            int[][] ret = new int[a.Length][];
            for (int i = 0; i < ret.Length; i++)
            {
                ret[i] = new int[b[0].Length];
            }
            for (int i = 0; i < b[0].Length; i++)//mat B x Coord
            {
                for (int j = 0; j < a.Length; j++)//mat A y Coord
                {
                    for (int m = 0; m < a[0].Length; m++)//mul sweep
                    {
                        ret[j][i] = add(ret[j][i], mul(p, k, a[j][m], b[m][i], irreducible), p);
                    }
                }
            }
            return ret;
        } //slow
        public int[][] GaloisMatMulFast(int[][] a, int[][] b, int p, int k, int irreducible) //run mulFastSet before use!
        {
            if (a[0].Length != b.Length)
            {
                throw new Exception("Matrix A's columns not equal to number of rows in matrix b");
            }
            int[][] ret = new int[a.Length][];
            for (int i = 0; i < ret.Length; i++)
            {
                ret[i] = new int[b[0].Length];
            }
            for (int i = 0; i < b[0].Length; i++)//mat B x Coord
            {
                for (int j = 0; j < a.Length; j++)//mat A y Coord
                {
                    for (int m = 0; m < a[0].Length; m++)//mul sweep
                    {
                        ret[j][i] = addFast(ret[j][i], mulFast(a[j][m], b[m][i]));
                    }
                }
            }
            //Print.say(Print.ARR_TO_STR(ret));
            return ret;
        }
        int[][] fastFastMatRet;
        int[] nullBuffer;
        public void GaloisMatMulFastFastSet(int p_k, int irr_index, int A_rows, int B_columns)
        {
            IRRSet(p_k);
            decField(p_k, ref decomp);
            GaloisMatMulFastFastSet(decomp[0], decomp[1], getIRRbyIndex(decomp[0], irr_index), A_rows, B_columns);
        }
        public void GaloisMatMulFastFastSet(int p, int k, int irr, int A_rows, int B_columns) //Matrix A must be a square
        {
            mulFastSet(p, k, irr);
            nullBuffer = new int[B_columns];
            fastFastMatRet = new int[A_rows][];
            for (int i = 0; i < fastFastMatRet.Length; i++)
            {
                fastFastMatRet[i] = new int[B_columns];
            }
        }
        void fastMatReset()
        {
            for (int i = 0; i < fastFastMatRet.Length; i++) nullBuffer.CopyTo(fastFastMatRet[i], 0);
        }
        public int[][] GaloisMatMulFastFast(int[][] a, int[][] b) //run GaloisMatMulFastFastSet before use!
        {
            fastMatReset();

            for (int i = 0; i < b[0].Length; i++)//mat B x Coord
            {
                for (int j = 0; j < a.Length; j++)//mat A y Coord
                {
                    for (int m = 0; m < a[0].Length; m++)//mul sweep
                    {
                        fastFastMatRet[j][i] = addFast(fastFastMatRet[j][i], mulFast(a[j][m], b[m][i]));
                    }
                }
            }
            //Print.say(Print.ARR_TO_STR(fastFastMatRet));
            return fastFastMatRet;
        }

        public int[] GaloisFieldMatINV(int[] a, int p, int k, int irr)
        {
            //Print.say(p + " " + k + " " + irr);
            GaloisMatMulFastFastSet(p, k, irr, a.Length, 1);
            int[][] solveThis = Matrix.rowToColumn(new int[a.Length]);
            for (int i = 0; i < solveThis.Length; i++) solveThis[i][0] = ran.Next(0, n);
            int[][] crackThis = GaloisMatMulFast(Matrix.squareMat(a), solveThis, p, k, irr);
            int[][] bruteBuffer = Matrix.squareMat(new int[a.Length]);
            int timestamp = pow(n, a.Length - 1) * 10;
            Console.WriteLine("solving " + Print.ARR_TO_STR(a) + "...");
            //Print.say(Print.ARR_TO_STR(solveThis));
            for (int i = 0; i < (long)Math.Pow(n, a.Length); i++)
            {
                //Print.say(Print.ARR_TO_STR(bruteBuffer));
                if (Matrix.columnEqual(GaloisMatMulFastFast(bruteBuffer, crackThis), solveThis)) return bruteBuffer[0];
                mat.diagClockRight(ref bruteBuffer, n);
                //if (i == timestamp) Console.WriteLine("Getting somewhere...");
            }
            return bruteBuffer[0];
        }
        public void GaloisFieldMatInvFull_ExThreaded(object obj)
        {
            ThreadInfo p = (ThreadInfo)obj;
            GaloisFieldMatINVFull_threaded(p.data, p.p, p.k, p.threadID, p.fieldIndex);
        }

        public void GaloisFieldMatINVFull_threaded_set(int numFields, int numThreads)
        {
            invMat = new InverseMatEx[numFields][][];
            for (int i = 0; i < invMat.Length; i++)
            {
                invMat[i] = new InverseMatEx[numThreads][]; //thread, poly
            }
            aug_threaded = new int[numThreads][][];
            mulSub_threaded = new int[numThreads][];
            addSub_threaded = new int[numThreads][];
        }

        public void GaloisFieldMatINVFull_threaded(int[][] A, int p, int k, int threadID, int fieldIndex) //set GalisFieldMatINVFull_threaded_set before use!
        {
            int numThreads = aug_threaded.Length;
            int numPolysPerThread = irr_.Length / numThreads;
            int start = threadID * numPolysPerThread;
            int end = (threadID + 1) * numPolysPerThread;
            if (threadID == numThreads - 1) end = irr_.Length;
            invMat[fieldIndex][threadID] = new InverseMatEx[end - start];
            for (int irr_index = start; irr_index < end; irr_index++)
            {
                invMat[fieldIndex][threadID][irr_index - start] = GaloisFieldMatINVFAST_threaded(A, p, k, irr_index, threadID);
            }
        }

        public int[][] GaloisFieldMatINVFull(int[][] a, int GF, int irr_index)
        {
            int[] f = decField(GF);
            IRRSet(GF);
            GaloisFieldMatINVFAST(ref a, f[0], f[1], getIRRbyIndex(f[0], irr_index));
            return a;
        }
        public int[] GaloisFieldMatINVFAST(int[] a, int p, int k, int irr)
        {
            int[] ret = new int[a.Length];
            int[][] ret0 = Matrix.squareMat(a);

            //Print.say(irr + " new");
            GaloisFieldMatINVFAST(ref ret0, p, k, irr);
            
            Matrix.getRow(ret0, 0, ref ret);
            return ret;
        }
        public int[][] aug;
        public int[][][] aug_threaded;
        public void GaloisFieldMatINVFAST(ref int[][] A, int p, int k, int irr)
        {
            mulFastSet(p, k, irr);

            aug = new int[A.Length][];
            int[] tmp = new int[A.Length << 1];
            bool inversefound = false;
            for (int i = 0; i < A.Length; i++) //create augment
            {
                aug[i] = new int[tmp.Length];
                for (int j = 0; j < A.Length; j++)
                {
                    aug[i][j] = A[i][j];
                }
                aug[i][A.Length + i] = 1;
            }
            for (int i = 0; i < A.Length; i++)
            {
                if (i <= 7)
                {
                    //diag ones
                    bool onefound = false;
                    for (int j = 1; j < n; j++)
                    {
                        if (mulFast(aug[i][i], j) == 1)
                        {
                            mulRow(ref aug, i, j);
                            //Print.say(aug);
                            onefound = true;
                        }
                    }
                    //string ps = "p's = ";
                    if (onefound)
                    {

                        //work on zeros
                        for (int j = 0; j < A.Length - 1; j++)
                        {
                            int row = (i + j + 1) % aug.Length;
                            bool znf = true; //zero NOT found
                            for (int l = 0; l < n; l++)
                            {
                                if (addFast(aug[(row) % aug.Length][i], l) == 0)
                                {
                                    //ps += l + ", ";
                                    Matrix.getRow(aug, i, ref tmp);
                                    mulRow(ref tmp, l);
                                    addRow(ref aug, row, tmp);
                                    //Print.say(aug);
                                    znf = false;
                                    //found, so apply to row
                                    if (i == A.Length - 1 && j == A.Length - 2) inversefound = true;
                                }
                            }
                            if (znf)
                            {
                                Print.print("0 cannot be solved at (" + i + ", " + row + ")");
                                i = A.Length;
                                j = A.Length;
                            }
                        }
                        //Print.say(aug);
                        //Print.printhex(aug);
                        //MessageBox.Show(ps);
                    }
                    else
                    {
                        //Print.print("1 cannot be solved at row " + i);
                        i = A.Length;
                    }
                }
            }
            if (inversefound)
            {
                for (int i = 0; i < A.Length; i++)
                {
                    for (int j = 0; j < A.Length; j++)
                    {
                        A[i][j] = aug[i][A.Length + j];
                    }
                }
                //Print.print("inverse: ");
                //Print.print(A);
            }
            else
            {
                Matrix.nullMAT(ref A);
            }
        }
        public InverseMatEx GaloisFieldMatINVFAST_threaded(int[][] A, int p, int k, int irr_index, int threadID)
        {
            mulFastSet_threaded(p, k, getIRRbyIndex(p, irr_index), threadID);

            aug_threaded[threadID] = new int[A.Length][];
            int[] tmp = new int[A.Length << 1];
            bool inversefound = false;
            for (int i = 0; i < A.Length; i++) //create augment
            {
                aug_threaded[threadID][i] = new int[tmp.Length];
                for (int j = 0; j < A.Length; j++)
                {
                    aug_threaded[threadID][i][j] = A[i][j];
                }
                aug_threaded[threadID][i][A.Length + i] = 1;
            }
            for (int i = 0; i < A.Length; i++)
            {
                if (i <= 7)
                {
                    bool onefound = false;
                    for (int j = 1; j < n; j++)
                    {
                        if (mulFast_threaded(aug_threaded[threadID][i][i], j, threadID) == 1)
                        {
                            mulRow_threaded(ref aug_threaded[threadID], i, j, threadID);
                            onefound = true;
                        }
                    }
                    if (onefound)
                    {
                        for (int j = 0; j < A.Length - 1; j++)
                        {
                            int row = (i + j + 1) % aug_threaded[threadID].Length;
                            bool znf = true; //zero NOT found
                            for (int l = 0; l < n; l++)
                            {
                                if (addFast_threaded(aug_threaded[threadID][(row) % aug_threaded[threadID].Length][i], l, threadID) == 0)
                                {
                                    Matrix.getRow(aug_threaded[threadID], i, ref tmp);
                                    mulRow_threaded(ref tmp, l, threadID);
                                    addRow_threaded(ref aug_threaded[threadID], row, tmp, threadID);
                                    znf = false;
                                    if (i == A.Length - 1 && j == A.Length - 2) inversefound = true;
                                }
                            }
                            if (znf)
                            {
                                Print.print("0 cannot be solved at (" + i + ", " + row + ")");
                                i = A.Length;
                                j = A.Length;
                            }
                        }
                    }
                    else
                    {
                        i = A.Length;
                    }
                }
            }
            InverseMatEx invMat = new InverseMatEx();
            invMat.poly_index = irr_index;
            int[] values = new int[pow(A.Length, 2)];
            if (inversefound)
            {
                invMat.inverse = true;
                for (int i = 0; i < A.Length; i++)
                {
                    for (int j = 0; j < A.Length; j++)
                    {
                        values[(i * A.Length) + j] = aug_threaded[threadID][i][A.Length + j];
                    }
                }
                invMat.values = values;
                return invMat;
            }
            else
            {
                invMat.inverse = false;
                return invMat;
            }
            numInvMatSolved++;
        }

        public void mulRow(ref int[][] A, int row, int a)
        {
            for (int i = 0; i < A[0].Length; i++)
            {
                A[row][i] = mulFast(A[row][i], a);
            }
        }
        public void mulRow_threaded(ref int[][] A, int row, int a, int threadID)
        {
            for (int i = 0; i < A[0].Length; i++)
            {
                A[row][i] = mulFast_threaded(A[row][i], a, threadID);
            }
        }
        public void mulRow(ref int[] row, int a)
        {
            for (int i = 0; i < row.Length; i++)
            {
                row[i] = mulFast(row[i], a);
            }
        }
        public void mulRow_threaded(ref int[] row, int a, int threadID)
        {
            for (int i = 0; i < row.Length; i++)
            {
                row[i] = mulFast_threaded(row[i], a, threadID);
            }
        }
        public void addRow(ref int[][] A, int row, int[] a)
        {
            for (int i = 0; i < A[0].Length; i++)
            {
                A[row][i] = addFast(A[row][i], a[i]);
            }
        }
        public void addRow_threaded(ref int[][] A, int row, int[] a, int threadID)
        {
            for (int i = 0; i < A[0].Length; i++)
            {
                A[row][i] = addFast_threaded(A[row][i], a[i], threadID);
            }
        }

        public void IRRSet(int p_k) //Read all irreducibles for field p_k
        {
            irr_ = IRR(p_k);
        }
        public int getIRRbyIndex(int p, int i) //Run IRRSet before use!
        {
            //Print.say(irr_[i]);
            return nb.sum(irr_[i % irr_.Length], p);
        }
        public bool isValidField(int a)
        {
            if (irr.prime(a)) return true;
            for (int i = 2; i < (a >> 1) + 1; i++)
            {
                if (irr.prime(i))
                {
                    double log = Math.Log(a, i);
                    //MessageBox.Show("log_" + i + "(" + a + ") = " + log + "\n" + (int)log + " == " + Math.Ceiling(log));
                    if ((int)log == Math.Ceiling(log)) return true;
                }
            }
            return false;
        }
        public bool isValidField(int p, int k)
        {
            if (irr.prime(p)) return true;
            return false;
        }
        /*
        int[] decomp = new int[2]; //p, k
        public int[] decompField(int a)
        {
            for (int i = 2; i < (a >> 1) + 1; i++)
            {
                double log = Math.Log(a, i);
                if ((int)log == Math.Ceiling(log))
                {
                    decomp[0] = i;
                    decomp[1] = (int)log;
                    return decomp;
                }
            }
            decomp[0] = a;
            decomp[1] = 1;
            return decomp;
        }
        */
        public int[] getAllFields(int a, int b)
        {
            List<int> ret = new List<int>();
            for (int i = a; i < b; i++)
            {
                if (isValidField(i)) ret.Add(i);
            }
            return Print.ListToARR(ret);
        }
        public int[] getCompositeFields(int a, int b)
        {
            List<int> ret = new List<int>();
            for (int i = a; i < b; i++)
            {
                if (!irr.prime(i))
                {
                    if (isValidField(i)) ret.Add(i);
                }
            }
            return Print.ListToARR(ret);
        }
        public int[] getPrimeFields(int a, int b)
        {
            List<int> ret = new List<int>();
            for (int i = a; i < b; i++)
            {
                if (irr.prime(i))
                {
                    ret.Add(i);
                }
            }
            return Print.ListToARR(ret);
        }
        public int[] decField(int a)
        {
            int[] ret = new int[2];
            decField(a, ref ret);
            return ret;
        }
        public void decField(int a, ref int[] fieldDec) //ref p, k
        {
            bool found = false;
            for (int i = 0; i < primes.Length; i++)
            {
                float log = (float)Math.Log(a, primes[i]);
                int logi = (int)log;
                if (Math.Ceiling(log) == logi)
                {
                    fieldDec[0] = primes[i];
                    fieldDec[1] = logi;
                    found = true;
                    break;
                }
            }
            if (!found)
            {
                fieldDec[0] = 2;
                fieldDec[1] = 8;
                Console.WriteLine(a + " is not a Galois Field");
            }
        }
        public string fF(int[] a) //format field
        {
            return "GF(" + a[0] + "^" + a[1] + ")";
        }
        int[] decomp = new int[2];
        public string fF(int a) //format field
        {
            decField(a, ref decomp);
            return "GF(" + decomp[0] + "^" + decomp[1] + ")";
        }
        public int numFields(int a, int b)
        {
            int ret = 0;
            for (int i = a; i < b; i++) ret += Convert.ToInt32(isValidField(i));
            return ret;
        }
        public int reduce(int[] PROD, int p, int k, int IRR)
        {
            int[] ret = new int[0];
            if (PROD.Length > k)
            {
                ret = PolyD.DIV_REM(PROD, nb.rep(IRR, p));
            }
            else
            {
                ret = PROD;
            }
            for (int i = 0; i < ret.Length; i++)
            {
                if (ret[i] < 0)
                {
                    ret[i] = (p - (-ret[i] % p));
                }
                ret[i] %= p;
            }
            return nb.sum(ret, p);
        }
        public int reduce(int PROD, int p, int k, int IRR)
        {
            return reduce(nb.rep(PROD, p), p, k, IRR);
        }
        public int[] Reduce(int[] a, int gf, int p, int k, int irr_index)
        {
            if (isValidField(gf))
            {
                //MessageBox.Show(irr_index + " NEW");
                for (int i = 0; i < a.Length; i++) a[i] = reduce(a[i], p, k, getIRRbyIndex(p, irr_index));
            }
            else for (int i = 0; i < a.Length; i++) a[i] %= gf;
            return a;
        }
        public char[] ReducetoChars(int[] a, int gf, int p, int k, int irr_index, int phase)
        {
            char[] chars = new char[a.Length];
            if (isValidField(gf))
            {
                //MessageBox.Show(irr_index + " NEW");
                int irr = irr__[irr_index];
                for (int i = 0; i < a.Length; i++) chars[i] = Convert.ToChar(reduce(a[i], p, k, irr) + phase);
            }
            else for (int i = 0; i < a.Length; i++) chars[i] = Convert.ToChar((a[i] % gf) + phase);
            return chars;
        }
        public int[][] Reduce(int[][] a, int gf, int p, int k, int irr_index)
        {
            int[][] ret = new int[a.Length][];
            for (int i = 0; i < ret.Length; i++) ret[i] = new int[a[i].Length];

            //Print.say(irr_index + " ind");

            int[] buffer = new int[a[0].Length];
            for (int i = 0; i < a.Length; i++)
            {
                a[i].CopyTo(buffer, 0);
                Reduce(buffer, gf, p, k, irr_index);
                buffer.CopyTo(ret[i], 0);
            }
            return ret;
        }


        /* no reason to really have this anymore :/
        public void SayPrimes()
        {
            MessageBox.Show(Print.ARR_TO_STR(primitive));
        }
        */
    }
}
