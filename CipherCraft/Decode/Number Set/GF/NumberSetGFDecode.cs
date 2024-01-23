using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CipherCraft
{
    public struct GF
    {
        public int field;
        public int offs;
    }
    public struct INV_MAT
    {
        public int[] A;
        public int irr_used;
        public int[] Z_b;
    }
    public struct ReductionField
    {
        public int n;
        public int p;
        public int k;
        public int irr_index;
    }
    public class NumberSetGFDecode : NumberSetDecoder
    {
        GF_P_N gfp_n = new GF_P_N();
        IRR irr = new IRR();
        Dictionary dict = new Dictionary();
        ReductionField[] RF;

        List<GF> gf;
        List<INV_MAT> invs;
        int[] Z_b = new int[2] { 2, 8 };
        int n;

        public NumberSetGFDecode()
        {

        }

        public NumberSetGFDecode(int[] a, int Z_b) : base(a)
        {
            SetZ_b(Z_b);
            design();
        }
        public void Set_a_Z_b(int[] a, int Z_b)
        {
            Set(a);
            SetZ_b(Z_b);
            design();
        }
        public void SetZ_b(int Z_b)
        {
            n = Z_b;
            gfp_n.decField(Z_b, ref this.Z_b);
        }
        public override void design()
        {
            checkGFs();

            invMATS();
            dictInvMATS();
        }
        void checkGFs()
        {
            gf = new List<GF>();
            for (int i = 0; i < a.Length; i++)
            {
                if (gfp_n.isValidField(a[i]))
                {
                    GF g = new GF();
                    g.field = a[i];
                    g.offs = i;
                    gf.Add(g);
                }
            }
            if (gf.Count > 0)
            {
                string add = "";
                for (int i = 0; i < gf.Count(); i++)
                {
                    add += gfp_n.fF(gf[i].field) + " at 0x" + gf[i].offs.ToString("X2");
                    if (i < gf.Count() - 1)
                    {
                        add += ", ";
                    }
                }
                logadd("Fields discovered: " + add + "\n");
            }
            else
            {
                logadd("No galois fields found with 8bit stride" + "\n");
            }
        }
        void invMATS()
        {
            gfp_n.IRRSet(n);
            int len = gfp_n.irr_.Length;
            invs = new List<INV_MAT>();
            INV_MAT tmp;
            for (int i = 0; i < len; i++)
            {
                //Print.say( Z_b[0] + " " + Z_b[1] + " " + i);
                int[] inv = gfp_n.GaloisFieldMatINVFAST(a, Z_b[0], Z_b[1], gfp_n.getIRRbyIndex(Z_b[0], Z_b[1], i));
                tmp = new INV_MAT();
                tmp.A = inv;
                tmp.irr_used = i;
                tmp.Z_b = Z_b;
                if (!Matrix.isNull(inv)) invs.Add(tmp);
            }
            if (invs.Count > 0)
            {
                string add = "";
                for (int i = 0; i < invs.Count; i++)
                {
                    add += invs[i].irr_used + ": " + Print.intARRtoHexStr(invs[i].A) + " with p(x) = " + gfp_n.getIRRbyIndex(Z_b[0], Z_b[1], invs[i].irr_used) + "\n";
                }
                logadd("GF Inverse Matricies Found Over Z_" + invs[0].Z_b[0] + "[" + invs[0].Z_b[1] + "]:\n\n" + add + invs.Count + "/" + gfp_n.irr_.Length + "\n");
            }
        }
        void dictInvMATS()
        {
            RF = new ReductionField[5];
            for (int i = 0; i < RF.Length; i++) RF[i] = new ReductionField();
            RF[0].n = 26;
            RF[0].p = 0;
            RF[0].k = 0;
            RF[0].irr_index = 0;

            RF[1].n = 27;
            RF[1].p = 3;
            RF[1].k = 3;
            RF[1].irr_index = 0;

            RF[2].n = 29;
            RF[2].p = 29;
            RF[2].k = 1;
            RF[2].irr_index = 0;

            RF[3].n = 31;
            RF[3].p = 31;
            RF[3].k = 1;
            RF[3].irr_index = 0;

            RF[4].n = 25;
            RF[4].p = 5;
            RF[4].k = 2;
            RF[4].irr_index = 0;


            int phase = 97;
            int[][] buffer = new int[invs.Count][];
            for (int i = 0; i < buffer.Length; i++)
            {
                buffer[i] = new int[invs[i].A.Length];
                invs[i].A.CopyTo(buffer[i], 0);
            }
            int[][][] reduced = new int[RF.Length][][];

            if (buffer.Length > 0)
            {
                for (int i = 0; i < reduced.Length; i++)
                {
                    gfp_n.IRRSet((int)Math.Pow(RF[i].p, RF[i].k));
                    reduced[i] = gfp_n.Reduce(buffer, RF[i].n, RF[i].p, RF[i].k, RF[i].irr_index);
                }
                string p = "";
                for (int i = 0; i < reduced.Length; i++)
                {
                    p += RF[i].n + ": " + Print.ARR_TO_STR(reduced[i][0]) + " with p(x) = " + invs[0].irr_used + "\n";
                }
                Console.WriteLine(p);

                for (int i = 0; i < reduced.Length; i++)
                {
                    string[] search = Print.intARRtoStrARR(reduced[i], phase);
                    //Print.say(search);
                    string decode = dict.dictionaryCheck(search, 4, 0);
                    logadd("Dictionary with reduction Z_" + RF[i].p + "[" + RF[i].n + "]");
                    logadd(decode + "\n");
                }
            }
            else logadd("No inv matricies found over " + gfp_n.irr_.Length + " irreducibles");
        }
    }
}
