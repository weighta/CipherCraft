using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.IO;

namespace CipherCraft
{
    class IRR
    {
        int[] S_ = new int[2];
        long[] S = new long[2];
        public int[][][] irr;
        NBase nbase = new NBase();
        public IRR()
        {
            loadIRRs();
        }
        public void loadIRRs()
        {
            string[] paths = Directory.GetFiles("GF\\IRR");
            irr = new int[paths.Length][][];
            for (int i = 0; i < irr.Length; i++)
            {
                try
                {
                    string[] buffer = File.ReadAllLines(paths[i]);
                    irr[i] = new int[buffer.Length][];
                    for (int j = 0; j < buffer.Length; j++)
                    {
                        irr[i][j] = Print.strToIntArr(buffer[j]);
                    }
                }
                catch
                {
                    MessageBox.Show("There was a problem with loading degree " + (i + 1) + " please check its formatting");
                }
            }
        }
        public int numAvIRRbyDeg(int a)
        {
            return irr[a - 1].Length;
        }
        public bool prime(int a)
        {
            if ((a & 1) == 1)
            {
                for (int i = 3; i < a >> 1; i += 2)
                {
                    if (((double)a / i) == (a / i)) return false;
                }
                return true;
            }
            else if (a == 2) return true;

            else return false;
        }

        public int[] pFact(int a)
        {
            int[] ret = new int[32];
            int[] s = spline(a);
            s.CopyTo(ret, 0);
            int b = 0;
            if (ret[0] != 0)
            {
                bool isFactors = true;
                while (isFactors)
                {
                    if (ret[b] == 0)
                    {
                        isFactors = false;
                    }
                    else
                    {
                        s = spline(ret[b]);
                        if (s[0] != 0)
                        {
                            Print.insertReplace(ref ret, s, b);
                            b++;
                        }
                        else
                        {
                            b++;
                        }
                    }
                }
                return Print.truncLeft(ret);
            }
            else return ret; //means it's already prime
        }
        public long[] pFact(long a)
        {
            long[] ret = new long[32];
            long[] s = spline(a);
            s.CopyTo(ret, 0);
            int b = 0;
            if (ret[0] != 0)
            {
                bool isFactors = true;
                while (isFactors)
                {
                    if (ret[b] == 0)
                    {
                        isFactors = false;
                    }
                    else
                    {
                        s = spline(ret[b]);
                        if (s[0] != 0)
                        {
                            Print.insertReplace(ref ret, s, b);
                            b++;
                        }
                        else
                        {
                            b++;
                        }
                    }
                }
                return Print.truncLeft(ret);
            }
            else return ret; //means it's already prime
        }
        public int[] spline(int a)
        {

            S_[0] = 0;
            if (a == 2)
            {
                return S_;
            }
            else if ((a & 1) == 0)
            {
                S_[0] = 2;
                S_[1] = a >> 1;
                return S_;
            }
            else
            {
                for (int i = 3; i < a >> 1; i += 2)
                {
                    if (a % i == 0)
                    {
                        S[0] = i;
                        S[1] = a / i;
                        return S_;
                    }
                }
            }
            S[1] = 0;
            return S_; //prime
        }
        public long[] spline(long a)
        {
            long fourth = (a >> 3) | 1;
            S[0] = 0;
            if (a == 2)
            {
                return S;
            }
            else if ((a & 1) == 0)
            {
                S[0] = 2;
                S[1] = a >> 1;
                return S;
            }
            else
            {
                for (long i = 3; i < a >> 1; i += 2)
                {
                    if (i == fourth) Print.print("pfact 1/8 of the way");
                    if (a % i == 0)
                    {
                        S[0] = i;
                        S[1] = a / i;
                        return S;
                    }
                }
            }
            S[1] = 0;
            return S; //prime
        }

        public int[] getPrimes(int a, int b)
        {
            int j = 0;
            for (int i = a; i < b; i++)
            {
                if (prime(i)) j++;
            }
            int[] p = new int[j];
            j = 0;
            for (int i = a; i < b; i++)
            {
                if (prime(i))
                {
                    p[j] = i;
                    j++;
                }
            }
            return p;
        }
    }
}
