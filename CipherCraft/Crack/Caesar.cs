using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace CipherCraft
{
    public class Caesar
    {
        Dictionary dict = new Dictionary();
        public Caesar()
        {

        }
        public string AnalyzeCipher(string a)
        {
            MessageBox.Show(a);
            int maxIteration = findLowestChar(a);
            string res = "";
            string[] line = new string[maxIteration];
            for (int i = 0; i < maxIteration; i++)
            {
                string tmp = "";
                for (int j = 0; j < a.Length; j++)
                {
                    tmp += (char)(a[j] - i);
                }
                line[i] = tmp;
            }
            res = dict.dictionaryCheck(line, 3, 0);
            int[] solve = findLongestString(line);
            string ret = "Iteration " + solve[0] + " seems to be suspicious";
            ret += "  with " + solve[1] + "chars.";
            ret += " Containing:\n" + line[solve[0]];
            ret += "\nAnd:\n" + strARRtoSTR(line);

            return ret;
        }
        public string AnalyzeCipher(int[] a)
        {
            string b = "";
            for (int i = 0; i < a.Length; i++)
            {
                b += (char)a[i];
            }
            return AnalyzeCipher(b);
        }
        public int findLowestChar(string a)
        {
            int ret = 1 << 30;
            for (int i = 0; i < a.Length; i++)
            {
                if (a[i] < ret) ret = a[i];
            }
            return ret;
        }
        public int[] findLongestString(string[] a)
        {

            int[] ret = new int[2];
            for (int i = 0; i < a.Length; i++)
            {
                if (a[i].Length > ret[1])
                {
                    ret[1] = a[i].Length;
                    ret[0] = i;
                }
            }
            return ret;
        }
        public string strARRtoSTR(string[] a)
        {
            string ret = "";
            for (int i = 0; i < a.Length; i++) ret += a[i] + "\n";
            return ret;
        }
        public string enc(string a, int phase)
        {
            string ret = "";
            for (int i = 0; i < a.Length; i++)
            {
                ret += (char)(a[i] + phase);
            }
            return ret;
        }
        public string enc(int[][] a, int phase)
        {
            string ret = "";
            for (int i = 0; i < a.Length; i++)
            {
                for (int j = 0; j < a[i].Length; j++)
                {
                    ret += Convert.ToChar(a[i][j] + phase) + " ";
                }
                ret += "\n";
            }
            return ret;
        }
        public string dec(string a, int phase)
        {
            string ret = "";
            for (int i = 0; i < a.Length; i++)
            {
                ret += (char)(a[i] - phase);
            }
            return ret;
        }
    }
}
