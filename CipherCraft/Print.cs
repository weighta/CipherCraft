using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;


namespace CipherCraft
{
    class Print
    {
        public Print()
        {

        }
        public static byte[] IntARRToByteARR(int[] a)
        {
            byte[] r = new byte[a.Length << 2];
            for (int i = 0; i < a.Length; i++)
            {
                r[(i << 2)] = (byte)(a[i] >> 24);
                r[(i << 2) + 1] = (byte)(a[i] >> 16);
                r[(i << 2) + 2] = (byte)(a[i] >> 8);
                r[(i << 2) + 3] = (byte)(a[i]);
            }
            return r;
        }
        public static int[] ByteARRToIntARR(byte[] a)
        {
            int[] r = new int[a.Length >> 2];
            for (int i = 0; i < r.Length; i++)
            {
                r[i] |= a[(i << 2)] >> 24;
                r[i] |= a[(i << 2) + 1] >> 16;
                r[i] |= a[(i << 2) + 2] >> 8;
                r[i] |= a[(i << 2) + 3];
            }
            return r;
        }
        public static string ARR_TO_STR(char[] a)
        {
            string ret = "";
            for (int i = 0; i < a.Length; i++)
            {
                ret += a[i] + " ";
            }
            return ret;
        }
        public static string ARR_TO_STR(char[] a, int phase)
        {
            string ret = "";
            for (int i = 0; i < a.Length; i++)
            {
                ret += (char)(a[i] + phase) + " ";
            }
            return ret;
        }
        public static string ARR_TO_STR(byte[] a)
        {
            string ret = "";
            for (int i = 0; i < a.Length; i++)
            {
                ret += a[i].ToString("X2") + " ";
            }
            return ret;
        }
        public static string ARR_TO_STR(int[] a)
        {
            string ret = "";
            for (int i = 0; i < a.Length; i++)
            {
                ret += a[i];
                if (i < a.Length - 1) ret += " ";
            }
            return ret;
        }
        public static string ARR_TO_STR_HEX(int[] a)
        {
            string ret = "";
            for (int i = 0; i < a.Length; i++)
            {
                ret += a[i].ToString("X2");
                if (i < a.Length - 1) ret += " ";
            }
            return ret;
        }
        public static string ARR_TO_STR(int[][] a)
        {
            string ret = "";
            for (int i = 0; i < a.Length; i++)
            {
                for (int j = 0; j < a[i].Length; j++)
                {
                    ret += a[i][j] + " ";
                }
                ret += "\n";
            }
            return ret;
        }
        public static string hashToString(byte[] a)
        {
            string ret = "";
            for (int i = 0; i < a.Length; i++)
            {
                ret += a[i].ToString("X2");
            }
            return ret;
        }
        public static string intARRtoHexStr(int[][] a)
        {
            string ret = "";
            for (int i = 0; i < a.Length; i++)
            {
                for (int j = 0; j < a[i].Length; j++)
                {
                    ret += a[i][j].ToString("X2") + " ";
                }
                ret += "\n";
            }
            return ret;
        }
        public static string intARRtoHexStr(int[] a)
        {
            string ret = "";
            for (int j = 0; j < a.Length; j++)
            {
                ret += a[j].ToString("X2") + " ";
            }
            return ret;
        }
        public static string[] intARRARRtoStrARR(int[][] a)
        {
            string[] ret = new string[a.Length];
            for (int i = 0; i < a.Length; i++)
            {
                ret[i] = ARR_TO_STR(a[i]);
            }
            return ret;
        }
        public static string ARR_TO_STR(long[] a)
        {
            string ret = "";
            for (int i = 0; i < a.Length; i++)
            {
                ret += a[i] + " ";
            }
            return ret;
        }
        public static string ARR_TO_STR_FANCY(int[] a)
        {
            string ret = "{ ";
            for (int i = 0; i < a.Length; i++)
            {
                ret += a[i];
                if (i < a.Length - 1) ret += ", ";
                else ret += " }";
            }
            return ret;
        }
        public static string ARR_TO_STR_FANCY(long[] a)
        {
            string ret = "{ ";
            for (int i = 0; i < a.Length; i++)
            {
                ret += a[i];
                if (i < a.Length - 1) ret += ", ";
                else ret += " }";
            }
            return ret;
        }
        public static string IntARRtoSTR(int[] a)
        {
            string ret = "";
            for (int i = 0; i < a.Length; i++)
            {
                ret += Convert.ToChar(a[i]);
            }
            return ret;
        }
        public static int[] strToIntArr(string a)
        {
            string[] b = a.Split(' ');
            int[] ret = new int[b.Length];
            for (int i = 0; i < b.Length; i++)
            {
                ret[i] = Convert.ToInt32(b[i]);
            }
            return ret;
        }
        public static int[] hexStrToIntArr(string a)
        {
            string[] b = a.Split(' ');
            int[] ret = new int[b.Length];
            for (int i = 0; i < b.Length; i++)
            {
                ret[i] = Convert.ToInt32(b[i], 16);
            }
            return ret;
        }
        public static int[] strToNumArray(string a)
        {
            int[] ret = new int[a.Length];
            for (int i = 0; i < ret.Length; i++)
            {
                ret[i] = a[i];
            }
            return ret;
        }
        public static int[][] strToNumArray(string[] a)
        {
            int[][] ret = new int[a.Length][];
            for (int i = 0; i < a.Length; i++)
            {
                string[] b = a[i].Split(' ');
                ret[i] = new int[b.Length];
                for (int j = 0; j < ret[i].Length; j++) ret[i][j] = Convert.ToInt32(b[j]);
            }
            return ret;
        }
        public static int[][] strToNumArray(string[] a, int format)
        {
            int[][] ret = new int[a.Length][];
                if (format == 16)
                {
                    for (int i = 0; i < a.Length; i++)
                    {
                        string[] b = a[i].Split(' ');
                        ret[i] = new int[b.Length];
                        for (int j = 0; j < ret[i].Length; j++) ret[i][j] = Convert.ToInt32(b[j], format);
                    }
                }
            return ret;
        }
        public static string byteArrayToString(byte[][] a)
        {
            string ret = "";
            for (int i = 0; i < a.Length; i++)
            {
                if (i != 0) ret += '\n';
                for (int j = 0; j < a[i].Length; j++)
                {
                    if (j != 0) ret += ' ';
                    ret += a[i][j].ToString("2X");
                }
            }
            return ret;    
        }
        public static string intArrayToHexadecimalString(int[][] a)
        {
            string ret = "";
            for (int i = 0; i < a.Length; i++)
            {
                if (i != 0) ret += '\n';
                for (int j = 0; j < a[i].Length; j++)
                {
                    if (j != 0) ret += ' ';
                    ret += ((byte)a[i][j]).ToString("X2");
                }
            }
            return ret;
        }
        public static string intArrayToHexadecimalStringTrunc(int[][] a)
        {
            string ret = "";
            for (int i = 0; i < a.Length; i++)
            {
                if (i != 0) ret += '\n';
                for (int j = a[i].Length >> 1; j < a[i].Length; j++)
                {
                    ret += ((byte)a[i][j]).ToString("X2");
                    if (j != a[i].Length - 1) ret += ' ';
                }
            }
            return ret;
        }
        public static byte[] strToByteArray(string a)
        {
            string[] b = a.Split('\n');
            byte[] ret = new byte[(int)Math.Pow(b.Length, 2)];
            for (int i = 0; i < b.Length; i++)
            {
                string[] row = b[i].Split(' ');
                for (int j = 0; j < row.Length; j++)
                {
                    ret[(i * b.Length) + j] = (byte)Convert.ToInt32(row[j], 16);
                }
            }
            return ret;
        }
        public static string byteArrayToStringMAT(byte[] a, int b)
        {
            string ret = "";
            for (int i = 0; i < b; i++)
            {
                if (i != 0) ret += "\n";
                for (int j = 0; j < b; j++)
                {
                    if (j != 0) ret += ' ';
                    ret += a[(i * b) + j].ToString("X2");
                }
            }
            return ret;
        }
        public static void phase(ref int[] a, int phase, bool add)
        {
            if (add) for (int i = 0; i < a.Length; i++) a[i] += phase;
            else for (int i = 0; i < a.Length; i++) a[i] -= phase;
        }
        public static int pow(int a, int b)
        {
            int aa = a;
            if (b == 0) a = 1;
            for (int i = 0; i < b - 1; i++)
            {
                a *= aa;
            }
            return a;
        }
        public static int[] SentToNum(string a)
        {
            int[] ret = new int[a.Length];
            for (int i = 0; i < ret.Length; i++)
            {
                ret[i] = a[i] - 65;
            }
            return ret;
        }
        public static string NumToSent(int[] a)
        {
            string ret = "";
            for (int i = 0; i < a.Length; i++)
            {
                ret += Convert.ToChar(97 + a[i]);
            }
            return ret;
        }
        public static int stringArrLen(string[] a)
        {
            int l = 0;
            for (int i = 0; i < a.Length; i++) l += a[i].Length;
            return l;
        }
        public static List<int> join(List<int> a, List<int> b)
        {
            for (int i = 0; i < b.Count; i++)
            {
                a.Add(b[i]);
            }
            return a;
        }
        public static void insertReplace(ref int[] a, int[] b, int i)
        {
            for (int j = a.Length - 1; j > i; j--)
            {
                a[j] = a[j - 1];
            }
            b.CopyTo(a, i);
        }
        public static void insertReplace(ref long[] a, long[] b, int i)
        {
            for (int j = a.Length - 1; j > i; j--)
            {
                a[j] = a[j - 1];
            }
            b.CopyTo(a, i);
        }
        public static int[] truncLeft(int[] a)
        {
            int b = 0;
            for (int i = 0; i < a.Length; i++)
            {
                if (a[i] == 0) break;
                b++;
            }
            int[] c = new int[b];
            for (int i = 0; i < b; i++)
            {
                c[i] = a[i];
            }
            return c;
        }
        public static long[] truncLeft(long[] a)
        {
            int b = 0;
            for (int i = 0; i < a.Length; i++)
            {
                if (a[i] == 0) break;
                b++;
            }
            long[] c = new long[b];
            for (int i = 0; i < b; i++)
            {
                c[i] = a[i];
            }
            return c;
        }
        public static bool contains(int[] a, int b, int l)
        {
            for (int i = 0; i < l; i++)
            {
                if (a[i] == b) return true;
            }
            return false;
        }
        public static bool contains(char[] a, int b, int l)
        {
            //MessageBox.Show("l: " + l);
            for (int i = 0; i < l; i++)
            {
                if (a[i] == b) return true;
            }
            return false;
        }
        public static void say(string a)
        {
            MessageBox.Show(a);
        }
        public static void say(string[] a)
        {
            string say = "";
            for (int i = 0; i < a.Length; i++) say += a[i] + "\n";
            MessageBox.Show(say);
        }
        public static void say(int a)
        {
            MessageBox.Show(a + "");
        }
        public static void say(int[] a)
        {
            say(ARR_TO_STR(a));
        }
        public static void say(int[][] a)
        {
            say(ARR_TO_STR(a));
        }
        public static void say(long[] a)
        {
            say(ARR_TO_STR(a));
        }
        public static void print(string a)
        {
            Console.WriteLine(a);
        }
        public static void print(int[][] a)
        {
            Console.WriteLine(ARR_TO_STR(a));
        }
        public static void printhex(int[][] a)
        {
            Console.WriteLine(intARRtoHexStr(a));
        }
        public static void print(int a)
        {
            print(a + "");
        }
        public static bool search(string a, string tosearch)
        {
            for (int i = 0; i < (a.Length - tosearch.Length) + 1; i++)
            {
                int c = 1;
                for (int j = 0; j < tosearch.Length; j++)
                {
                    c &= Convert.ToInt32(a[i + j] == tosearch[j]);
                }
                if (c == 1) return true;
            }
            return false;
        }
        public static bool search(string[] a, string tosearch)
        {
            for (int i = 0; i < a.Length; i++)
            {
                if (a[i].Equals(tosearch)) return true;
            }
            return false;
        }
        public static char[] charListToCharARR(List<char> a)
        {
            char[] ret = new char[a.Count];
            for (int i = 0; i < ret.Length; i++) ret[i] = a[i];
            return ret;
        }

        public static int[] ListToARR(List<int> a)
        {
            int[] ret = new int[a.Count];
            for (int i = 0; i < ret.Length; i++) ret[i] = a[i];
            return ret;
        }
        public static int[][] ListToARR(List<int[]> a)
        {
            int[][] ret = new int[a.Count][];
            for (int i = 0; i < a.Count; i++)
            {
                ret[i] = a[i];
            }
            return ret;
        }
        public static long[] ListToARR(List<long> a)
        {
            long[] ret = new long[a.Count];
            for (int i = 0; i < ret.Length; i++) ret[i] = a[i];
            return ret;
        }
        public static string charARRtoSTR(char[] a)
        {
            string ret = "";
            for (int i = 0; i < a.Length; i++)
            {
                ret += a[i];
            }
            return ret;
        }
        public static string strARRtoSTR(string[] a)
        {
            string ret = "";
            for (int i = 0; i < a.Length; i++)
            {
                ret += a[i] + " ";
            }
            return ret;
        }
        public static string[] intARRtoStrARR(int[][] a, int phase)
        {
            string[] ret = new string[a.Length];
            string add;
            for (int i = 0; i < a.Length; i++)
            {
                add = "";
                for (int j = 0; j < a[i].Length; j++)
                {
                    add += Convert.ToChar(a[i][j] + phase);
                }
                ret[i] = add;
            }
            return ret;
        }
    }
}
