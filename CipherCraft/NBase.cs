using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace CipherCraft
{
    class NBase
    {
        Debug d = new Debug();
        public NBase()
        {

        }
        public string strRep(int a, int n)
        {
            return Print.ARR_TO_STR(rep(a, n));
        }
        public int[] rep(int a, int b)
        {
            if (b < 2) b = 2;
            int[] ret = new int[log(b, a) + 1];
            for (int i = 0; i < ret.Length; i++)
            {
                ret[i] = b - 1;
                while(sum(ret, b) > a)
                {
                    ret[i]--;
                }
            }
            return ret;
        }
        public int[] rep(long a, int b)
        {
            if (b < 2) b = 2;
            int[] ret = new int[log(b, a) + 1];
            for (int i = 0; i < ret.Length; i++)
            {
                ret[i] = b - 1;
                while (sumlong(ret, b) > a)
                {
                    ret[i]--;
                }
            }
            return ret;
        }
        public int sum(int[] a, int b)
        {
            int ret = 0;
            for (int i = 0; i < a.Length; i++)
            {
                ret += a[a.Length - 1 - i] * pow(b, i);
            }
            return ret;
        }
        public long sumlong(int[] a, int b)
        {
            long ret = 0;
            for (int i = 0; i < a.Length; i++)
            {
                ret += a[a.Length - 1 - i] * (long)Math.Pow(b, i);
            }
            return ret;
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
        public int log(int b, int a)
        {
            if (a == 0 || b == 0)
            {
                return 0;
            }
            return (int)Math.Log(a, b);
        }
        public int log(int b, long a)
        {
            if (a == 0 || b == 0)
            {
                return 0;
            }
            return (int)Math.Log(a, b);
        }
        public int add(int a, int b, int n)
        {
            if (n == 0 || n == 1) n = 2;
            int[] aa = rep(a, n);
            int[] bb = rep(b, n);
            sym(ref aa, ref bb);
            for (int i = 0; i < aa.Length; i++)
            {
                aa[i] = (aa[i] + bb[i]) % n;
            }
            return sum(aa, n);
        }
        public int sub(int a, int b, int n)
        {
            if (n == 0 || n == 1) n = 2;
            int[] aa = rep(a, n);
            int[] bb = rep(b, n);
            sym(ref aa, ref bb);
            for (int i = 0; i < aa.Length; i++)
            {
                aa[i] = ((aa[i] - bb[i]) + n) % n;
            }
            return sum(aa, n);
        }
        public bool equal(int[] a, int[] b)
        {
            if (a.Length != b.Length) return false;
            for (int i = 0; i < a.Length; i++)
            {
                if (a[i] != b[i]) return false;
            }
            return true;
        }

        public int mul(int a, int b, int n, int m, int FIELD_SIZE, int PRIMITIVE)
        {
            
            return 0;
        }
        public int[] shorten(int[] a, int s)
        {
            int j = 0;
            int[] ret = new int[s];
            for (int i = a.Length - s; i < a.Length; i++)
            {
                ret[j] = a[i];
                j++;
            }
            return ret;
        }
        public int dif(int a, int b)
        {
            return Math.Abs(a - b);
        }
        public void sym(ref int[] a, ref int[] b)
        {
            if(a.Length > b.Length)
            {
                int[] temp = b;
                b = new int[a.Length];
                temp.CopyTo(b, dif(a.Length, temp.Length));
            }
            else if(a.Length < b.Length)
            {
                int[] temp = a;
                a = new int[b.Length];
                temp.CopyTo(a, dif(b.Length, temp.Length));
            }
        }

        /// <summary>
        /// REDUNDANT. ONLY USED FOR DEBUGGING
        /// </summary>
        /// <param name="a"></param> NUMBER TO CHECK
        /// <param name="b"></param> BASE
        /// <returns></returns>
        public int check(int a, int b)
        {
            return sum(rep(a, b), b);
        }
    }
}
