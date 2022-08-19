using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace CipherCraft
{
    public class PolyDiv
    {
        NBase nb = new NBase();
        public int[] A = new int[] { 1, 0, 7, 5, 2, 0, 3 };
        public int[] B = new int[] { 1, 3, 4, 5, 1, 7 };

        private int[][] MON;
        private int[] RES;
        private int[] DIVISOR;
        private int[][] PASCAL;
        private const int DEF_MONTISA_SIZE = 32;
        public PolyDiv() //constructor
        {
            MON = new int[DEF_MONTISA_SIZE][];
            for (int i = 0; i < MON.Length; i++) MON[i] = new int[DEF_MONTISA_SIZE];
            RES = new int[DEF_MONTISA_SIZE];
            PASCAL = new int[DEF_MONTISA_SIZE][];
            int PASCAL_ROW_SIZE = DEF_MONTISA_SIZE + 1;
            for (int j = 0; j < PASCAL.Length; j++)
            {
                int termCountRow = j + 2;
                PASCAL[j] = new int[termCountRow];
                for (int k = 0; k < termCountRow; k++)
                {
                    if (k == 0 || k == termCountRow - 1)
                    {
                        PASCAL[j][k] = 1;
                    }
                    else
                    {
                        PASCAL[j][k] = PASCAL[j - 1][k] + PASCAL[j - 1][k - 1];
                    }
                }
            }
        }

        //the computational concept is still off - needs work
        public bool IRR(int[] a, int p) //irreducible is a MONIC (leading coefficient of 1) polynomial
        {
            int[] expTable = new int[a.Length - 1];
            int[] b = new int[a.Length];
            for (int i = 1; i < expTable.Length + 1; i++)
            {
                expTable[expTable.Length - i] = Print.pow(p, i);
            }
            if (a[0] == 1)
            {
                for (int i = 1; i < nb.sum(a, p); i++) //loop through all possible divideable polynomials
                {
                    //incriment
                    //incriemnt to begin with because you cannot divide by 0 xD

                    //Method 1
                    b[b.Length - 1]++;
                    if (i != 1)
                    {
                        for (int j = 0; j < expTable.Length; j++)
                        {
                            if ((i % expTable[j]) == 0)
                            {
                                b[j]++;
                            }
                        }
                        //reduce incriment
                        for (int j = 0; j < b.Length; j++)
                        {
                            b[j] %= p;
                        }
                    }

                    //Method 2 (17ms slower)
                    /*
                    b = nb.rep(i, p); 
                    */

                    //div test
                    if (i > p - 1)
                    {
                        //MessageBox.Show("before divide = " + Print.ARR_TO_STR(b) + "\ni = " + i + "\nof " + nb.sum(a, p));
                        if (REM_ZERO(DIV_REM(a, b)))
                        {
                            //MessageBox.Show("Reduced " + Print.ARR_TO_STR(a) + "\nwith: " + Print.ARR_TO_STR(b) + "\ncycle " + (i - 1) + "\nexp table " + Print.ARR_TO_STR(expTable));
                            return false;
                        }
                    }
                    //MessageBox.Show("after divide = " + Print.ARR_TO_STR(b) + "\ni = " + i + "\nof " + nb.sum(a, p));
                }
                return true;
            }
            else return false;
        }

        private bool isNullArray(int[] a)
        {
            //MessageBox.Show(Print.ARR_TO_STR(a));
            bool ret = true;
            for (int i = 0; i < a.Length; i++)
            {
                ret &= a[i] == 0;
            }
            return ret;

        }

        public int[] DIV(int[] a, int[] b_)
        {
            CLEAR();
            int[] b = new int[b_.Length];
            if (b_[0] == 0) //leading coef of divisor cannot be 0, if so, truncate
            {
                b = TRUNC(b_);
            }
            else
            {
                for (int i = 0; i < b_.Length; i++) b[i] = b_[i];
            }
            int h = b.Length - 1;
            int l = a.Length - 1;
            int QUO_LEN = (a.Length - b.Length) + 1;
            int div = b[0];
            for (int i = 1; i < b.Length; i++) //divisor negate term n-1 coef through 0th
            {
                b[i] *= -1;
            }
            for (int i = 0; i < a.Length; i++) //numerator poly coefs at top
            {
                MON[0][i] = a[i];
            }
            for (int i = 0; i < QUO_LEN; i++) //solve quotient
            {
                for (int j = 0; j < MON.Length; j++) //sum column
                {
                    RES[i] += MON[j][i];
                }
                RES[i] /= div; //div by leading coef of divisor
                for (int j = 0; j < h; j++) //implant criss-cross
                {
                    MON[MON.Length - 1 - j][j + i + 1] = RES[i] * b[j + 1];
                }
                //dbDiv();
            }
            for (int i = QUO_LEN; i < a.Length; i++)//solve remainder
            {
                for (int j = 0; j < MON.Length; j++)
                {
                    RES[i] += MON[j][i];
                }
            }
            return RES;
        }

        public int[] TRUNC(int[] a)
        {
            int c = 0;
            for (int i = 0; i < a.Length; i++)
            {
                if (a[i] != 0)
                {
                    c = i;
                    break;
                }
            }
            int[] ret = new int[a.Length - c];
            for (int i = c; i < a.Length; i++)
            {
                ret[i - c] = a[i];
            }
            return ret;
        }


        void dbDiv()
        {
            MessageBox.Show(Print.ARR_TO_STR(MON) + "\n" + Print.ARR_TO_STR(RES));
        }


        public int[] DIV_REM(int[] a, int[] b)
        {
            int[] RES = new int[b.Length - 1];
            int[] QUO = DIV(a, b);
            int j = 0;
            for (int i = (a.Length - b.Length) + 1; i < a.Length; i++)
            {
                RES[j] = QUO[i];
                j++;
            }
            return RES;
        }
        public int[] DIV_REM(string a, string b)
        {
            return DIV_REM(Print.strToIntArr(a), Print.strToIntArr(b));
        }
        public int[] DIV_REM(int a, int b, int p)
        {
            return DIV_REM(nb.rep(a, p), nb.rep(b, p));
        }

        public bool REM_ZERO(int[] a)
        {
            Console.WriteLine(a.Length + "");
            for (int i = 0; i < a.Length; i++)
            {
                if (a[i] != 0) return false;
            }
            return true;
        }

        public int[] DIV_QUO(int[] a, int[] b)
        {
            int QUO_LEN = (a.Length - b.Length) + 1;
            int[] QUO = DIV(a, b);
            RES = new int[QUO_LEN];
            for (int i = 0; i < QUO_LEN; i++)
            {
                RES[i] = QUO[i];
            }
            return RES;

        }
        public int[] DIV_QUO(string a, string b)
        {
            return DIV_QUO(Print.strToIntArr(a), Print.strToIntArr(b));
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
            RES = new int[32];
        }
        /*
                string debug = "";
                for (int j = 0; j < MON.Length; j++)
                {
                    for (int k = 0; k < MON[i].Length; k++)
                    {
                        debug += MON[j][k] + " ";
                    }
                    debug += "\n";
                }
                for (int j = 0; j < MON.Length; j++)
                {
                    debug += "=";
                }
                debug += "\n";
                for (int j = 0; j < RES.Length; j++)
                {
                    debug += RES[j] + " ";
                }
                MessageBox.Show(debug);
                */
    }
}
