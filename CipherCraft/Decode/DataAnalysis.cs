using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace CipherCraft
{
    /// <summary>
    /// Take bytes and multiply each pair of 4, then represent that product into base 26 to find hidden ascii characters
    /// </summary>
    public class nBaseAscii
    {
        char[] c = new char[] { 'a', 'b', 'c', 'd', 'e', 'f', 'g', 'h', 'i', 'j', 'k', 'l', 'm', 'n', 'o', 'p', 'q', 'r', 's', 't', 'u', 'v', 'w', 'x', 'y', 'z' };
        NBase nb = new NBase();
        public Dictionary dict = new Dictionary();
        byte[] data;
        public nBaseAscii(byte[] data)
        {
            this.data = data;
        }
        public string[] searchEverything() //Every stride, every language lookup
        {
            int startStride = 1;
            string[] dictionaryRes = new string[dict.language.Length];
            string[] search = new string[9 - startStride];
            for (int i = startStride; i <= 8; i++)
            {
                search[i - startStride] = Solve(i, 26);
            }
            for (int i = 0; i < dictionaryRes.Length; i++)
            {
                //MessageBox.Show(Print.stringArrLen(search) + "");
                dictionaryRes[i] = dict.dictionaryCheck(search, 4, i);
            }
            return dictionaryRes;
        }

        public string Solve(int stride, int n)
        {
            string decode = "";
            for (int i = 0; i < data.Length; i+=stride)
            {
                long prod = 1;
                if (i + stride > data.Length)
                {
                    for (int j = i; j < i + ((i + stride) - data.Length); j++)
                    {
                        if (data[j] != 0) prod *= data[j];
                    }
                }
                else
                {
                    for (int j = i; j < i + stride; j++)
                    {
                        if (data[j] != 0) prod *= data[j];
                        //MessageBox.Show(prod + "");
                    }
                }
                int[] ascii = nb.rep(prod, n);
                //MessageBox.Show("rep: " + Print.ARR_TO_STR(ascii));
                for (int j = 0; j < ascii.Length; j++)
                {
                    decode += c[ascii[j]];
                }
                //MessageBox.Show("findings: " + decode + "\ni=" + i);
            }
            return decode;
        }
    }
}
