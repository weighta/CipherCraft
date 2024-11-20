using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace CipherCraft
{
    public class FieldHopper
    {
        Enigma enigma = new Enigma();
        GF_P_N gfp_n = new GF_P_N();

        public string db = "";

        public FieldHopper()
        {

        }
        public void matHop(string map, ref int[] text)
        {

            db = "before: " + Print.ARR_TO_STR(text) + "\n";

            string[] lines = map.Split('\n');
            for (int i = 0; i < lines.Length; i++)
            {
                int[] line = Print.strToIntArr(lines[i]);
                GFmatMulData(line[0], line[1], getMat(line), ref text);
            }
        }
        public void matUnHop(string map, ref int[] text)
        {

            db = "before: " + Print.ARR_TO_STR(text) + "\n";

            string[] lines = map.Split('\n');
            for (int i = lines.Length - 1; i >= 0; i--)
            {
                int[] line = Print.strToIntArr(lines[i]);
                GFmatMulData(line[0], line[1], getIMat(line), ref text);
            }
        }
        public int[] getMat(int[] line)
        {
            int[] ret = new int[(line.Length - 2) >> 1];
            for (int i = 0; i < ret.Length; i++) ret[i] = line[2 + i];
            return ret;
        }
        public int[] getIMat(int[] line)
        {
            int[] ret = new int[(line.Length - 2) >> 1];
            for (int i = 0; i < ret.Length; i++) ret[i] = line[2 + ret.Length + i];
            return ret;
        }
        int[] fieldDec = new int[2];

        public void GFmatMulData(int GF, int irr_index, int[] a, ref int[] text)
        {
            gfp_n.decField(GF, ref fieldDec);
            db += "GF(" + fieldDec[0] + "^" + fieldDec[1] + ") p(x) = " + gfp_n.getIRRbyIndex(fieldDec[0], irr_index) + " mat: " + Print.ARR_TO_STR(a) +  "\n";
            int bNumColumns = text.Length / a.Length;
            int[][] b = new int[a.Length][];
            for (int i = 0; i < b.Length; i++){
                b[i] = new int[bNumColumns];
            }

            gfp_n.GaloisMatMulFastFastSet(fieldDec[0], fieldDec[1], gfp_n.getIRRbyIndex(fieldDec[0], irr_index), a.Length, bNumColumns);
            for (int i = 0; i < bNumColumns; i++)
            {
                for (int j = 0; j < a.Length; j++)
                {
                    b[j][i] = text[(i * a.Length) + j];
                }
            }

            b = gfp_n.GaloisMatMulFastFast(Matrix.squareMat(a), b);
            for (int i = 0; i < b[0].Length; i++)
            {
                for (int j = 0; j < b.Length; j++)
                {
                    text[(i * b.Length) + j] = b[j][i];
                }
            }
            db += "res: " + Print.ARR_TO_STR(text) + "\n";
        }
    }
}
