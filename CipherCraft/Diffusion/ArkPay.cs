using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CipherCraft
{
    public class ArkPay
    {
        public ArkPay()
        {

        }
        public string Ark(string s, bool eord)
        {
            string ret = "";
            string[] ss = s.Split(' ');
            if (eord)
            {
                for (int i = 0; i < ss.Length; i++)
                {
                    Swap(ref ss[i], 0, ss[i].Length - 1);
                    ss[i] += "ay";
                    ret += ss[i];
                    if (i != ss.Length - 1) ret += " ";
                }
            }
            else
            {
                for (int i = 0; i < ss.Length; i++)
                {
                    ss[i] = ss[i].Substring(0, ss[i].Length);
                    Swap(ref ss[i], 0, ss[i].Length - 1);
                    ret += ss[i];
                    if (i != ss.Length - 1) ret += " ";
                }
            }
            return ret;
        }
        public void Swap(ref string s, int a, int b)
        {
            char[] aa = s.ToCharArray();
            char c = aa[a];
            aa[a] = aa[b];
            aa[b] = c;
            s = "";
            for (int i = 0; i < aa.Length; i++) s += aa[i];
        }
        public void Shift(ref string s, int a)
        {

        }
    }
}
