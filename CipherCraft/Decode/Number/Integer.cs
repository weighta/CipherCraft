using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CipherCraft
{
    public class Integer
    {
        long num;
        int digis;
        List<string> log;

        IRR irr = new IRR();
        NBase nb = new NBase();

        public Integer()
        {

        }
        public Integer(long num)
        {
            Open(num);
        }
        public void Open(long num)
        {
            this.num = num;
            log = new List<string>();
            digis = (int)Math.Log10(num) + 1;
            SolveEverything();
        }
        void SolveEverything()
        {
            FACTTRIANGLE();
            NBASE();
            BRENDECODER();
        }

        void FACTTRIANGLE()
        {
            Print.print("Computing Factorization Triangle");
            string log = "FACT TRIANGLE\n\n";
            int n = 10;
            long[] curr = new long[1];
            curr[0] = num;
            List<long> chain = new List<long>();
            for (int i = 0; i < n; i++)
            {
                Print.print("n = " + (i + 1));
                curr = irr.pFact(curr[0]);
                if (curr[0] > 1)
                {
                    log += Print.ARR_TO_STR(curr) + " n = " + (i + 1) + "\n";
                    while(curr.Length > 1)
                    {
                        curr = diff(curr);
                        log += Print.ARR_TO_STR(curr) + "\n";
                        if (curr.Length == 1) chain.Add(curr[0]);
                    }
                }
                else
                {
                    break;
                }
            }
            log += "chain: " + Print.ARR_TO_STR_FANCY(Print.ListToARR(chain));
            this.log.Add(log);
        }
        void NBASE()
        {
            Print.print("NBASE");
            string log = "NBASE\n\n";
            int n = (int)Math.Pow(digis, 2);
            for (int i = 2; i < n+2; i++)
            {
                log += "n = " + i + ": " + Print.ARR_TO_STR(nb.rep(num, i))+"\n";
                Print.print("n = " + i);
            }
            this.log.Add(log);
        }
        void BRENDECODER()
        {
            Print.print("BRENDECODER");
            string log = "BRENDECODER\n\n";
            int[] numArr = nb.rep(num, 10);
            int high = 0;
            for (int i = 0; i < numArr.Length; i++) //solve high
            {
                if (numArr[i] > high) high = numArr[i];
            }
            log += "highest integer " + high + "\n";
            high++;
            if (high > 1)
            {
                int[] sumHighBase = nb.rep(nb.sumlong(numArr, high), 10);
                log += "n = " + high + ": " + nb.sumlong(sumHighBase, 10) + "\n";
                List<int> res = new List<int>();
                for (int i = 0; i < sumHighBase.Length; i++)
                {
                    if (sumHighBase[i] > 1)
                    {
                        if (irr.prime(sumHighBase[i])) res.Add(0);
                        else res.Add(1);
                    }
                }
                int[] bin = Print.ListToARR(res);
                log += "bin: " + Print.ARR_TO_STR(bin) + "\n";
                log += "solved: " + nb.sum(bin, 2);
            }
            else
            {

            }
            this.log.Add(log);
        }

        int[] diff(int[] a)
        {
            int[] ret = new int[a.Length - 1];
            for (int i = 0; i < ret.Length; i++)
            {
                ret[i] = Math.Abs(a[i] - a[i + 1]);
            }
            return ret;
        }
        long[] diff(long[] a)
        {
            long[] ret = new long[a.Length - 1];
            for (int i = 0; i < ret.Length; i++)
            {
                ret[i] = Math.Abs(a[i] - a[i + 1]);
            }
            return ret;
        }
        int[] toArray(long a)
        {
            int[] ret = new int[(int)Math.Log10(a) + 1];
            int state = 0;
            for (int i = 0; i < ret.Length; i++)
            {
                ret[i] = (int)((num / (int)Math.Pow(10, ret.Length - i)) - (state * 10));
                state = (state * 10) + ret[i];
            }
            return ret;
        }
        public string getLog()
        {
            string ret = "";
            for (int i = 0; i < log.Count; i++)
            {
                ret += log[i] + "\n\n";
            }
            return ret;
        }
    }
}
