using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.IO;

namespace CipherCraft
{
    class GF_2_8
    {
        int high = 0x100;
        public int[] primitive = new int[]
        {
            0x101, 0x107, 0x10D, 0x10F, 0x115, 0x119, 0x11B,
            0x125, 0x133, 0x137, 0x139, 0x13D, 0x14B, 0x151,
            0x15B, 0x15D, 0x161, 0x167, 0x16F, 0x175, 0x17B,
            0x17F, 0x185, 0x18D, 0x191, 0x199, 0x1A3, 0x1A5,
            0x1AF, 0x1B1, 0x1B7, 0x1BB, 0x1C1, 0x1C9, 0x1CD,
            0x1CF, 0x1D3, 0x1DF, 0x1E7, 0x1EB, 0x1F3, 0x1F7,
            0x1FD
        };

        public int[][] A;
        public int[][] iA;
        public int[][] mat = new int[][]
        {
            new int[] { 2, 3, 1, 1 }, //AES
            new int[] { 1, 4, 5, 1 }, //*new*
            new int[] { 0x52, 0x65, 0x6E, 0x61 }, //Rena
            new int[] { 0x73, 0xf1, 0xc0, 0xb5 } //jb23 (Test)
        };
        public int[][] imat = new int[][]
        {
            new int[] { 14, 11, 13, 9 },
            new int[] { 69, 81, 65, 84 },
            new int[] { 125, 154, 156, 137 },
            new int[] { 121, 97, 160, 52 }
        };
        public byte[][] mulS = new byte[256][];

        public GF_2_8()
        {

        }

        public GF_2_8(int MATRIX_INDEX)
        {
            getMulTable();
            getMatTable();
            setMat(MATRIX_INDEX % mat.Length);
        }

        public void getMulTable()
        {
            /*
            byte[] mulSS = File.ReadAllBytes("GF\\2_N\\2_8\\0x11B");
            for (int i = 0; i < mulS.Length; i++)
            {
                mulS[i] = new byte[256];
                for (int j = 0; j < mulS[i].Length; j++)
                {
                    mulS[i][j] = mulSS[(i * 256) + j];
                }
            }
             */
        }

        public int mulFast(int a, int b)
        {
            return mulS[a][b];
        }

        public void getMatTable()
        {
            byte[] dat = File.ReadAllBytes("GF\\2_N\\2_8\\0x11Bmat");
            mat = new int[dat.Length >> 3][];
            imat = new int[mat.Length][];
            for (int i = 0; i < mat.Length; i++)
            {
                mat[i] = new int[4];
                imat[i] = new int[4];
                for (int j = 0; j < 4; j++)
                {
                    mat[i][j] = dat[(i << 3) + j];
                    imat[i][j] = dat[(i << 3) + j + 4];
                }
            }
        }


        public void setMat(int i)
        {
            A = makeMat(mat[i]);
            iA = makeMat(imat[i]);
        }

        public int mul(int a, int b) //will use standard 0x11B
        {
            int ret = 0;
            while(b > 0)
            {
                if ((b & 1) == 1) ret ^= a;
                a <<= 1;
                b >>= 1;
                if ((a & high) == high) a ^= primitive[6];
            }
            return ret;
        }

        public int mul(int a, int b, int irreducible_index) //Choose a primitive
        {
            int ret = 0;
            while (b > 0)
            {
                if ((b & 1) == 1) ret ^= a;
                a <<= 1;
                b >>= 1;
                if ((a & high) == high) a ^= primitive[irreducible_index % primitive.Length];
            }
            return ret;
        }

        //A = 4x4, B = 4x1
        public int[] matMul(int[][] A, int[] B)
        {
            int[] ret = new int[B.Length];
            for (int i = 0; i < A.Length; i++)
            {
                for (int j = 0; j < A[i].Length; j++)
                {
                    ret[i] ^= mul(A[i][j], B[j]);
                }
            }
            return ret;
        }

        public int[] matMul(int[][] A, int[] B, int p)
        {
            //MessageBox.Show(Print.ARR_TO_STR(A) + "\n mul \n" + Print.ARR_TO_STR(B) + "\n with \n" + primitive[6]);
            int[] ret = new int[B.Length];
            for (int i = 0; i < A.Length; i++)
            {
                for (int j = 0; j < A[i].Length; j++)
                {
                    ret[i] ^= mul(A[i][j], B[j], p);
                }
            }
            //MessageBox.Show(Print.ARR_TO_STR(ret));
            return ret;
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

        public int[][] makeMat(int[] row)
        {
            int[][] ret = new int[row.Length][];
            for (int i = 0; i < ret.Length; i++)
            {
                ret[i] = new int[row.Length];
                for (int j = 0; j < ret[i].Length; j++)
                {
                    ret[i][j] = row[((j - i) + 4) % 4];
                }
            }
            return ret;
        }
        public int[][] rowToColumn(int[] a)
        {
            int[][] ret = new int[4][];
            for (int i = 0; i < ret.Length; i++)
            {
                ret[i] = new int[1];
                ret[i][0] = a[i];
            }
            return ret;
        }

        public int[] fullMulTable(int p)
        {
            int[] ret = new int[(int)Math.Pow(256, 2)];
            for (int i = 0; i < 256; i++)
            {
                mulTable(i, p).CopyTo(ret, i * 256);
            }
            return ret;
        }
        public int[] mulTable(int a, int p)
        {
            int[] ret = new int[256];
            for (int i = 0; i < ret.Length; i++)
            {
                ret[i] = mul(i, a, p);
            }
            return ret;
        }

        public int[] solveInv(int[][] A)
        {
            int[][] inv = new int[4][];
            inv[0] = new int[4];
            inv[1] = new int[4];
            inv[2] = new int[4];
            inv[3] = new int[4];

            int[] bef = new int[] { 97, 129, 7, 249 };
            int[] aft = matMul(A, bef);
            for (int a = 0; a < 256; a++)
            {
                for (int b = 0; b < 256; b++)
                {
                    for (int c = 0; c < 256; c++)
                    {
                        for (int d = 0; d < 256; d++)
                        {
                            inv[0][0] = a;
                            inv[0][1] = b;
                            inv[0][2] = c;
                            inv[0][3] = d;

                            inv[1][0] = d;
                            inv[1][1] = a;
                            inv[1][2] = b;
                            inv[1][3] = c;

                            inv[2][0] = c;
                            inv[2][1] = d;
                            inv[2][2] = a;
                            inv[2][3] = b;

                            inv[3][0] = b;
                            inv[3][1] = c;
                            inv[3][2] = d;
                            inv[3][3] = a;
                            if (compare(matMulFast(inv, aft), bef))
                            {
                                a = 256;
                                b = 256;
                                c = 256;
                                d = 256;
                            }
                        }
                    }
                }
                Console.WriteLine("a = " + a);
            }
            return new int[] { inv[0][0], inv[0][1], inv[0][2], inv[0][3] };
        }

        public bool compare(int[] a, int[] b)
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

    }
}
