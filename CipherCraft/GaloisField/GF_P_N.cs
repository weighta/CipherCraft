using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Windows.Forms;

namespace CipherCraft
{
    public class GF_P_N
    {
        Random ran = new Random();
        NBase nb = new NBase();
        PolyMul PolyM = new PolyMul();
        PolyDiv PolyD = new PolyDiv();
        IRR irr = new IRR();
        Matrix mat = new Matrix();

        
        int n;
        int[] mulSub;
        int[] addSub;
        private int[] primes;
        public int[][] irr_;


        public GF_P_N()
        {
            primes = irr.getPrimes(0, 25000);
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

                        //Print.say(Print.ARR_TO_STR(nb.rep(j, p)) + " " +  j + " *\n" + Print.ARR_TO_STR(nb.rep(k, p)) + " " + k + " =\n" + Print.ARR_TO_STR(PolyM.MUL(nb.rep(j, p), nb.rep(k, p), p)) +  "\n" + Print.ARR_TO_STR(a));
                        if (nb.equal(PolyM.MUL(nb.rep(j, p), nb.rep(k, p), p), a))
                        {
                            //MessageBox.Show("the candidate irreducible is not primitive because\n" + Print.ARR_TO_STR(nb.rep(j, p)) + " x " + Print.ARR_TO_STR(nb.rep(k, p)) + " = " + Print.ARR_TO_STR(PolyM.MUL(nb.rep(j, p), nb.rep(k, p), p)));
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
                return Print.strToNumArray(File.ReadAllLines(dir));
            }
            List<int[]> prim = new List<int[]>();
            for (int i = a; i < (int)Math.Pow(p_k[0], p_k[1]) << 1; i++)
            {
                int[] b = nb.rep(i, p_k[0]);
                if (IRR(b, p_k[0]))
                {
                    prim.Add(b);
                }
            }
            Directory.CreateDirectory(@"GF\IRR\" + p_k[0] + "\\");
            int[][] ret = Print.ListToARR(prim);
            File.WriteAllLines(@"GF\IRR\" + p_k[0] + "\\" + p_k[1] + ".txt", Print.intARRARRtoStrARR(ret));
            return ret;
        }

        public int add(int a, int b, int p)
        {
            return nb.add(a, b, p);
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
            Console.WriteLine("Patience while creating add table " + p + "^" + k + " with irr ");
            string path = @"GF\P_N\ADD\" + p;
            int n = pow(p, k);
            int[] t = new int[pow(n, 2)];
            try
            {
                for (int i = 0; i < n; i++)
                {
                    for (int j = 0; j < n; j++)
                    {
                        t[(i * n) + j] = add(i, j, p);
                    }
                }
                File.WriteAllBytes(path + "\\" + k.ToString("X"), Print.IntARRToByteARR(t));
            }
            catch
            {
                Directory.CreateDirectory(path);
                addFastMake(p, k);
            }

        }
        public void mulFastMake(int p, int k, int IRR)
        {
            Console.WriteLine("Patience while creating mul table " + p + "^" + k + " with irr " + IRR);
            string path = @"GF\P_N\" + p + "\\" + k;
            int n = pow(p, k);
            int[] t = new int[pow(n, 2)];
            try
            {
                for (int i = 0; i < n; i++)
                {
                    for (int j = 0; j < n; j++)
                    {
                        t[(i * n) + j] = mul(p, k, i, j, IRR);
                    }
                }
                File.WriteAllBytes(path + "\\" + IRR.ToString("X"), Print.IntARRToByteARR(t));
            }
            catch
            {
                Directory.CreateDirectory(path);
                mulFastMake(p, k, IRR);
            }
        }

        public void mulFastSet(int p, int k, int IRR)
        {
            addFastSet(p, k);
            string path = @"GF\P_N\" + p + "\\" + k + "\\" + IRR.ToString("X");
            try
            {
                mulSub = Print.ByteARRToIntARR(File.ReadAllBytes(path));
            }
            catch
            {
                mulFastMake(p, k, IRR);
                mulFastSet(p, k, IRR);
            }
            n = pow(p, k);
        }
        public void addFastSet(int p, int k)
        {
            string path = @"GF\P_N\ADD\" + p + "\\" + k.ToString("X");
            try
            {
                addSub = Print.ByteARRToIntARR(File.ReadAllBytes(path));
            }
            catch
            {
                addFastMake(p, k);
                addFastSet(p, k);
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
            GaloisMatMulFastFastSet(decomp[0], decomp[1], getIRRbyIndex(decomp[0], decomp[1], irr_index), A_rows, B_columns);
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

        public int[][] GaloisFieldMatINVFull(int[][] a, int GF, int irr_index)
        {
            int[] f = decField(GF);
            IRRSet(GF);
            GaloisFieldMatINVFAST(ref a, f[0], f[1], getIRRbyIndex(f[0], f[1], irr_index));
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
        public void GaloisFieldMatINVFAST(ref int[][] A, int p, int k, int irr)
        {
            GaloisMatMulFastFastSet(p, k, irr, A.Length, 1);

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

        public void mulRow(ref int[][] A, int row, int a)
        {
            for (int i = 0; i < A[0].Length; i++)
            {
                A[row][i] = mulFast(A[row][i], a);
            }
        }
        public void mulRow(ref int[] row, int a)
        {
            for (int i = 0; i < row.Length; i++)
            {
                row[i] = mulFast(row[i], a);
            }
        }
        public void addRow(ref int[][] A, int row, int[] a)
        {
            for (int i = 0; i < A[0].Length; i++)
            {
                A[row][i] = addFast(A[row][i], a[i]);
            }
        }

        public void IRRSet(int p_k) //Read all irreducibles for field p_k
        {
            irr_ = IRR(p_k);
        }
        public int getIRRbyIndex(int p, int k, int i) //Run IRRSet before use!
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
                double log = Math.Log(a, primes[i]);
                int logi = (int)log;
                if (Math.Ceiling(log) == logi)
                {
                    fieldDec[0] = primes[i];
                    fieldDec[1] = logi;
                    found = true;
                    break;
                }
            }
            if (!found) throw new Exception(a + " is not a Galois Field");
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
            //Print.say(IRR);
            if (PROD.Length > k) //rem div if prod > k
            {
                ret = PolyD.DIV_REM(PROD, nb.rep(IRR, p));
                //db += "Over field size! Reducing with " + Print.ARR_TO_STR(nb.rep(irreducible, p)) + "\n";
            }
            else
            {
                ret = PROD;
            }

            //db += "REM: " + Print.ARR_TO_STR(ret) + "\n";

            for (int i = 0; i < ret.Length; i++) //fix negative coefs
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
        public void Reduce(ref int[] a, int gf, int p, int k, int irr_index)
        {
            if (isValidField(gf))
            {
                //MessageBox.Show(irr_index + " NEW");
                for (int i = 0; i < a.Length; i++) a[i] = reduce(a[i], p, k, getIRRbyIndex(p, k, irr_index));
            }
            else for (int i = 0; i < a.Length; i++) a[i] %= gf;
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
                Reduce(ref buffer, gf, p, k, irr_index);
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
