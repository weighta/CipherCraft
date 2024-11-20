using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace CipherCraft
{
    public struct IntArrFast
    {
        public int[] intArr;
        public int Length;
    }
    class PolyMul
    {
        public IntArrFast MUL_arr;

        int DEF_MON_SIZE = 32;
        int[] RES;
        int[][] MON;
        PolyDiv polyD;
        public PolyMul() //CONSTRUTOR
        {
            polyD = new PolyDiv();
            MON = new int[DEF_MON_SIZE][];
            for (int i = 0; i < MON.Length; i++)
            {
                MON[i] = new int[DEF_MON_SIZE];
            }
            RES = new int[DEF_MON_SIZE];
            MUL_arr = new IntArrFast() { intArr = new int[DEF_MON_SIZE], Length = 0 };
        }
        public void MUL(IntArrFast a, IntArrFast b)
        {
            CLEAR();
            int resLen = a.Length + b.Length - 1;
            int numRows = Larger(a.Length, b.Length);
            for (int i = 0; i < b.Length; i++) //do mul
            {
                for (int j = 0; j < a.Length; j++)
                {
                    MON[i][MON.Length - 1 - j - i] = b.intArr[b.Length - 1 - i] * a.intArr[a.Length - 1 - j];
                }
            }

            for (int i = 0; i < resLen; i++) //sum columns
            {
                for (int j = 0; j < numRows; j++)
                {
                    RES[i] += MON[j][MON[i].Length - resLen + i];
                }
            }
            MUL_arr.Length = resLen;
            
            RES.CopyTo(MUL_arr.intArr, 0);
        }

        public int[] MUL(int[] a, int[] b)
        {
            CLEAR();
            int resLen = a.Length + b.Length - 1;
            int numRows = Larger(a.Length, b.Length);
            for (int i = 0; i < b.Length; i++) //do mul
            {
                for (int j = 0; j < a.Length; j++)
                {
                    MON[i][MON.Length - 1 - j - i] = b[b.Length - 1 - i] * a[a.Length - 1 - j];
                }
            }

            for (int i = 0; i < resLen; i++) //sum columns
            {
                for (int j = 0; j < numRows; j++)
                {
                    RES[i] += MON[j][MON[i].Length - resLen + i];
                }
            }
            int[] tmp = new int[resLen];
            for (int i = 0; i < tmp.Length; i++) tmp[i] = RES[i];
            return tmp;
        }
        public void MUL(int[] a, int[] b, int p) //with COEF mod
        {
            MUL(a, b);
            for (int i = 0; i < MUL_arr.Length; i++) MUL_arr.intArr[i] %= p;
        }
        public void MUL(IntArrFast a, IntArrFast b, int p) //with COEF mod
        {
            MUL(a, b);
            for (int i = 0; i < MUL_arr.Length; i++) MUL_arr.intArr[i] %= p;
        }
        public int[] MUL(string a, string b)
        {
            MUL(Print.strToIntArr(a), Print.strToIntArr(b));
            int[] ret = new int[MUL_arr.Length];
            for (int i = 0; i < ret.Length; i++) ret[i] = MUL_arr.intArr[i];
            return ret;
        }

        public int Larger(int a, int b)
        {
            if (a > b) return a;
            return b;
        }
        public int Smaller(int a, int b)
        {
            if (a < b) return a;
            return b;
        }
        private void CLEAR() //Clear montisa workspace and result
        {
            for (int i = 0; i < MON.Length; i++)
            {
                for (int j = 0; j < MON.Length; j++)
                {
                    MON[i][j] = 0;
                }

            }
            for (int i = 0; i < RES.Length; i++)
            {
                RES[i] = 0;
            }
        }

        void db()
        {
            MessageBox.Show(Print.ARR_TO_STR(MON) + "\n" + Print.ARR_TO_STR(RES));
        }
    }
}
