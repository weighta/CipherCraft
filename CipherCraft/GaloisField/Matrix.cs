using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CipherCraft
{
    public class Matrix
    {
        public Matrix()
        {

        }
        public void shiftRowsRight(ref int[][] a)
        {
            int[] tmp = new int[a[0].Length];
            for (int i = 1; i < a.Length; i++)
            {
                tmp[0] = a[i][a.Length - 1];
                for (int j = 1; j < a[j].Length; j++)
                {
                    tmp[a.Length - j] = a[i][a.Length - j - 1];
                }
                for (int j = 0; j < tmp.Length; j++) a[i][j] = tmp[j];
            }
        }
        public void diagClockRight(ref int[][] a, int mod)
        {
            for (int i = 0; i < a[0].Length; i++) //only worry about top
            {
                a[0][i]++;
                if (a[0][i] == mod) a[0][i] = 0;
                else i = a[0].Length;
            }
            for (int i = 1; i < a.Length; i++) //now apply to rows 1-3
            {
                for (int j = 0; j < a[i].Length; j++)
                {
                    a[i][j] = a[0][easyMod(j + (a.Length - i), a.Length)];
                }
            }
        }
        public int easyMod(int a, int p)
        {
            if (a >= p) return a - p;
            else return a;
        }
        public static bool columnEqual(int[][] a, int[][] b)
        {
            for (int i = 0; i < a.Length; i++)
            {
                if (a[i][0] != b[i][0]) return false;
            }
            return true;
        }
        public static int[][] squareMat(int[] row)
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
        public static int[][] rowToColumn(int[] a)
        {
            int[][] ret = new int[a.Length][];
            for (int i = 0; i < ret.Length; i++)
            {
                ret[i] = new int[1];
                ret[i][0] = a[i];
            }
            return ret;
        }
        public static int[] columnToRow(int[][] a)
        {
            int[] ret = new int[a.Length];
            for (int i = 0; i < a.Length; i++)
            {
                ret[i] = a[i][0];
            }
            return ret;
        }
        public static void getRow(int[][] A, int row, ref int[] a)
        {
            for (int i = 0; i < A[0].Length; i++)
            {
                a[i] = A[row][i];
            }
        }
        public static void nullMAT(ref int[][] A)
        {
            for (int i = 0; i < A.Length; i++) for (int j = 0; j < A[i].Length; j++) A[i][j] = 0;
        }
        public static bool isNull(int[] a)
        {
            for (int i = 0; i < a.Length; i++) if (a[i] != 0) return false;
            return true;
        }
        public static bool isNull(int[][] a)
        {
            for (int i = 0; i < a.Length; i++) for (int j = 0; j < a[i].Length; j++) if (a[i][j] != 0) return false;
            return true;
        }
    }
}
