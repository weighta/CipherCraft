using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Windows.Forms;

namespace CipherCraft
{
    public class GF_2_N
    {
        Random ran = new Random();
        Debug d = new Debug();
        NBase nb = new NBase();

        int FIELD_SIZE;
        public int N;
        public int[] primitive;

        public int[][] mulS;

        public GF_2_N()
        {
            setFieldSize(8);
        }
        public GF_2_N(int N)
        {
            setFieldSize(N);
        }

        public void setFieldSize(int N)
        {
            this.N = N;
            FIELD_SIZE = nb.pow(2, N);
            mulS = new int[FIELD_SIZE][];
            primitive = getPrimes(FIELD_SIZE, nb.pow(2, N + 1));
        }

        public void setMulTable(int PRIMITIVE)
        {
            int B = ceil(N / 8.0);
            byte[] mulSS = File.ReadAllBytes("GF\\2_N\\2_" + N + "\\0x" + primitive[PRIMITIVE % primitive.Length].ToString("X2"));
            for (int i = 0; i < mulS.Length; i++)
            {
                mulS[i] = new int[FIELD_SIZE];
                for (int j = 0; j < mulS[i].Length; j++)
                {
                    for (int g = 0; g < B; g++)
                    {
                        mulS[i][j] |= (mulSS[(i * (FIELD_SIZE * B)) + (j * B) + g] << (((B - 1) - g) << 3));
                    }
                }
            }
        }
        public int[] matMulFast(int[][] A, int[] B)
        {
            int[] ret = new int[B.Length];
            for (int i = 0; i < A.Length; i++)
            {
                for (int j = 0; j < A[i].Length; j++)
                {
                    ret[i] ^= mulFast(A[i][j], B[j]);
                }
            }
            return ret;
        }

        public int mulFast(int a, int b)
        {
            return mulS[a][b];
        }

        public int mul(int a, int b, int p)
        {
            int ret = 0;
            while (b > 0)
            {
                if ((b & 1) == 1) ret ^= a;
                a <<= 1;
                b >>= 1;
                if ((a & FIELD_SIZE) == FIELD_SIZE) a ^= primitive[p % primitive.Length];
            }
            return ret;
        }

        public void genAllPrimitiveMulTables()
        {
            Directory.CreateDirectory("GF\\2_N\\2_" + N);
            int B = ceil(N / 8.0);
            for (int i = 0; i < primitive.Length; i++)
            {
                int[] table = fullMulTable(i);
                byte[] toWrite = new byte[B * table.Length];
                for (int j = 0; j < table.Length; j++)
                {
                    for (int g = 0; g < B; g++)
                    {
                        toWrite[(j * B) + g] = (byte)(table[j] >> (((B - 1) - g) << 3));
                    }
                }
                File.WriteAllBytes("GF\\2_N\\2_" + N + "\\0x" + primitive[i].ToString("X2"), toWrite);
            }
        }
        public int[] fullMulTable(int p)
        {
            int[] ret = new int[(int)Math.Pow(FIELD_SIZE, 2)];
            for (int i = 0; i < FIELD_SIZE; i++)
            {
                mulTable(i, p).CopyTo(ret, i * FIELD_SIZE);
            }
            return ret;
        }
        public int[] mulTable(int a, int p)
        {
            int[] ret = new int[FIELD_SIZE];
            for (int i = 0; i < ret.Length; i++)
            {
                ret[i] = mul(i, a, p);
            }
            return ret;
        }


        public int[] getPrimes(int a, int b)
        {
            int j = 0;
            for (int i = a; i < b; i++)
            {
                if (bPrime(i)) j++;
            }
            int[] p = new int[j];
            j = 0;
            for (int i = a; i < b; i++)
            {
                if (bPrime(i))
                {
                    p[j] = i;
                    j++;
                }
            }
            return p;
        }

        public void solveInv(int[] A, int PRIMITIVE)
        {
            setMulTable(PRIMITIVE);
            Console.WriteLine(Print.ARR_TO_STR(squareMat(getInv(A))));
        }
        public int[] getInv(int[] A)
        {
            int[] primes = getPrimes(2, pow(2, N));
            int[][] inv = new int[A.Length][];
            for (int i = 0; i < inv.Length; i++) inv[i] = new int[inv.Length];
            int[] bef = new int[inv.Length];
            for (int i = 0; i < bef.Length; i++) bef[i] = primes[pow(i + 3, 2) % primes.Length];
            int[] aft = matMulFast(squareMat(A), bef);

            //MessageBox.Show(Matrix.printMat(squareMat(A)) + "times\n" + d.intArrToHexString(bef) + "\nequals\n" + d.intArrToHexString(aft));

            int[] abcd = new int[A.Length];
            int s = A.Length - 1;

            int[] pT = new int[A.Length];
            for (int i = 0; i < pT.Length; i++)
            {
                pT[i] = pow(FIELD_SIZE, i);
            }

           //MessageBox.Show(pow(FIELD_SIZE, inv.Length) + " : pow(" + FIELD_SIZE + ", " + inv.Length + ")");

            for (int i = 0; i < pow((long)FIELD_SIZE, inv.Length); i++)
            {
                for (int j = 0; j < A.Length; j++)
                {
                    for (int g = 0; g < A.Length; g++)
                    {
                        inv[j][g] = abcd[((s * j) + g) % A.Length];
                    }
                }
                if (bCompare(matMulFast(inv, aft), bef))
                {
                    break;
                }
                for (int j = 0; j < A.Length; j++)
                {
                    if (i == 0)
                    {
                        if (j == 0)
                        {
                            abcd[j] = (abcd[j] + 1) % FIELD_SIZE;
                        }
                    }
                    else
                    {
                        if (i % pT[j] == 0) abcd[j] = (abcd[j] + 1) % FIELD_SIZE;
                    }
                }
                if (i % pT[A.Length - 1] == 0)
                {
                    Console.WriteLine("a = " + abcd[abcd.Length - 1]);
                }
            }
            return getRow(inv, 0);
        }

        public int[][] squareMat(int[] row)
        {
            int[][] ret = new int[row.Length][];
            for (int i = 0; i < ret.Length; i++)
            {
                ret[i] = new int[row.Length];
                for (int j = 0; j < ret[i].Length; j++)
                {
                    ret[i][j] = row[((j - i) + row.Length) % row.Length];
                }
            }
            return ret;
        }

        public int[] getRow(int[][] A, int r)
        {
            int[] ret = new int[A.Length];
            for (int i = 0; i < ret.Length; i++)
            {
                ret[i] = A[r][i];
            }
            return ret;
        }

        public bool bCompare(int[] a, int[] b)
        {
            bool ret = true;
            for (int i = 0; i < a.Length; i++)
            {
                ret &= a[i] == b[i];
                if (!ret)
                {
                    return false;
                }
            }
            return true;
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

        public bool bPrime(int a)
        {
            if (a % 2 == 1)
            {
                for (int i = 3; i < a >> 1; i += 2)
                {
                    if (((double)a / i) == (a / i))
                    {
                        return false;
                    }
                }
                return true;
            }
            else if (a == 2)
            {
                return true;
            }
            else return false;
        }

        public override string ToString()
        {
            return "GF(2^" + N + ") (GF("+ FIELD_SIZE + "))\nϵ " + d.intArrToHexString(primitive);
        }
        public int ceil(double d)
        {
            return (int)Math.Ceiling(d);
        }
        public void msgP(int p)
        {
            MessageBox.Show("0x" + primitive[p % primitive.Length].ToString("X2"));
        }
    }
}
