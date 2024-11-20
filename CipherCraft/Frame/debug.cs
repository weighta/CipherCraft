using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CipherCraft
{
    class Debug
    {
        GF_2_8 gf = new GF_2_8();

        public Debug()
        {
            gf.getMulTable();
        }


        //Consider removing
        public string getMatMulList(int[] B, int i)
        {
            gf.setMat(i);
            string ret = "";
            for (int j = 0; j < gf.primitive.Length; j++)
            {
                ret += intArrToString(gf.matMul(gf.A, B, j)) + "\n";
            }
            return ret;

        }

        public string getPrimitives()
        {
            string ret = "";
            for (int i = 0; i < gf.primitive.Length; i++)
            {
                ret += gf.primitive[i].ToString("X2") + " ";
            }
            return ret;
        }

        public int[] solveInv(int[] A)
        {
            return gf.solveInv(gf.makeMat(A));
        }

        public string intArrToHexString(int[] a)
        {
            string ret = "";
            for (int i = 0; i < a.Length; i++)
            {
                ret += a[i].ToString("X2") + " ";
            }
            return ret;
        }

        public string intArrToString(int[] a)
        {
            string ret = "";
            for (int i = 0; i < a.Length; i++)
            {
                ret += a[i] + " ";
            }
            return ret;
        }

        public string intArrToChar(int[] a)
        {
            string ret = "";
            for (int i = 0; i < a.Length; i++)
            {
                ret += Convert.ToChar(a[i]) + " ";
            }
            return ret;
        }

        public byte[] intArrToByte(int[] a)
        {
            byte[] ret = new byte[a.Length];
            for (int i = 0; i < ret.Length; i++)
            {
                ret[i] = (byte)a[i];
            }
            return ret;
        }

        public double d(string a)
        {
            return Convert.ToDouble(a);
        }

        public int i(string a)
        {
            try
            {
                return Convert.ToInt32(a);
            }
            catch
            {
                return 2;
            }
        }
    }
}
