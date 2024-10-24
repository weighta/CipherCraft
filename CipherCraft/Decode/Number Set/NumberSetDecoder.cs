using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CipherCraft
{
    public abstract class NumberSetDecoder
    {
        public int[] a;
        public byte[] a_;
        public int Base = 256;


        public double stdd;
        public double avg;
        public int sum;
        long num;

        public string log = "";

        public NumberSetDecoder()
        {

        }
        public void Set(int[] a)
        {
            log = "";
            num = 0;
            this.a = a;
            basis();
        }
        public void Set(byte[] a_)
        {
            this.a_ = a_;
        }
        protected NumberSetDecoder(int[] a)
        {
            this.a = a;
        }
        protected NumberSetDecoder(byte[] a_)
        {
            this.a_ = a_;
        }
        void basis()
        {
            AVG();
            STDD();
            STAT();
            NUM();
        }
        void AVG()
        {
            sum = 0;
            for (int i = 0; i < a.Length; i++)
            {
                sum += a[i];
            }
            logadd("sum: " + sum);
            avg = (double)sum / a.Length;
            logadd("avg: " + avg);
        }
        void STDD()
        {
            double sum = 0.0;
            for (int i = 0; i < a.Length; i++)
            {
                sum += Math.Pow(a[i] - avg, 2);
            }
            stdd = Math.Sqrt(sum / (a.Length - 1));
            logadd("std_dev: " + stdd);
        }
        void STAT()
        {
            string add = "The polynomial p(x) = " + Print.ARR_TO_STR(a) + " of degree " + (a.Length - 1) + " ";
            if (a.Length >= 6) add += "has no general formula to solve roots.";
            else add += "has a general formula to solve roots";
            logadd(add);
        }
        void NUM()
        {
            for (int i = 0; i < a.Length; i++)
            {
                num += (long)Math.Pow(Base, i) * a[a.Length - i - 1];
            }
            logadd("num (" + Base + "): " + num);
        }

        public abstract void design();

        public void logadd(string b)
        {
            log += b + "\n";
        }
    }
}
