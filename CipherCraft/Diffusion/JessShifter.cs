using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CipherCraft
{
    class JessShifter
    {
        int[] p = new int[] { 2, 3, 5, 7, 11, 13, 17, 19, 23, 29, 31, 37, 39, 41, 43, 47 };

        public JessShifter()
        {

        }
        public string j(string s, bool ed)
        {
            char[] ss = s.ToCharArray();
            if(ed)
            {
                for (int i = 0; i < ss.Length; i++)
                {
                    swap(ref ss, i, (i + p[i % p.Length]) % ss.Length);
                }
            }
            else
            {
                for (int i = ss.Length - 1; i >= 0; i--)
                {
                    swap(ref ss, i, (i + p[i % p.Length]) % ss.Length);
                }
            }

            return new string(ss);
        }
        private void swap(ref char[] ss, int a, int b)
        {
            char aa = ss[a];
            ss[a] = ss[b];
            ss[b] = aa;
        }
        private int maxI(int i)
        {
            int j = 1;
            while (p[j] < i >> 1)
            {
                j++;
            }
            return j;
        }
    }
}
