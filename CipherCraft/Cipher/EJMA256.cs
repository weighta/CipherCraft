using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace CipherCraft
{
    public class EJMA256
    {
        private JessShifter js;
        private GF_P_N gfp_n;
        private Enigma en;
        private RDA_cipher rda;

        private const int NUM_IRREDUCIBLEPOLY = 30;
        private int[] IRREDUCIBLEPOLY = new int[30] { 0x11B, 0x11D, 0x12B, 0x12D, 0x139, 0x13F, 0x14D, 0x15F, 0x163, 0x165, 0x169, 0x171, 0x177, 0x17B, 0x187, 0x18B, 0x18D, 0x19F, 0x1A3, 0x1A9, 0x1B1, 0x1BD, 0x1C3, 0x1CF, 0x1D7, 0x1DD, 0x1E7, 0x1F3, 0x1F5, 0x1F9 };
        private int[] PRIMES = new int[256];

        private int[] JMAP = new int[0x780]; //Number of irreducible polynomials * 64 bytes
        private int[] INV_JMAP = new int[0x780];
        byte[] J_IMAGE = new byte[64];

        private byte[] GFMUL_LOOKUP = new byte[0x1E0000]; //Number of irreducible polynomials * 256^2
        private byte[] GFMULINV_LOOKUP = new byte[0x1E00]; // Number of Irreducible polynomials * 256
        private byte[] GFM_AUG = new byte[64];
        private byte[] GFM_TMP = new byte[64];
        private byte[] GFM_NULL = new byte[64];
        private int[] GFM_IRRPOLYINDEX = new int[30];
        private byte[] RHASH = new byte[64];
        private byte[] KHASH = new byte[64]; //Number of irreducible polynomials * 64 bytes;
        private byte[] EXP = new byte[0x780];
        private string PWD;

        public EJMA256()
        {
            gfp_n = new GF_P_N();
            rda = new RDA_cipher();
            BuildPrimes();
            BuildGfMulLookupTable();
            BuildGfMulInvLookupTable();
        }
        private void BuildPrimes()
        {
            PRIMES[0] = 2;
            PRIMES[1] = 3;
            int n = 5;
            for (int i = 2; i < 256;)
            {
                bool prime = true;
                for (int j = 3; j < n >> 1; j += 2)
                {
                    if (n % j == 0)
                    {
                        prime = false;
                        break;
                    }
                }
                if (prime) PRIMES[i++] = n;
                n += 2;
            }
        }
        private void BuildGfMulLookupTable()
        {
            for (int i = 0; i < NUM_IRREDUCIBLEPOLY; i++)
            {
                int IRR_MULL_OFFSET = i << 16;
                for (int a = 0; a <= 255; a++)
                {
                    for (int b = 0; b <= 255; b++)
                    {
                        //GFMUL_LOOKUP[(i << 16) + (a << 8) + b] = (byte)gfp_n.mul(2, 8, a, b, IRREDUCIBLEPOLY[i]);
                        GFMUL_LOOKUP[IRR_MULL_OFFSET + (a << 8) + b] = GMul((byte)a, (byte)b, i);
                    }
                }
            }
        }
        private void BuildGfMulInvLookupTable()
        {
            for (int i = 0; i < NUM_IRREDUCIBLEPOLY; i++)
            {
                int IRR_MULL_OFFSET = i << 16;
                int IRR_MULLINV_OFFSET = (i << 8);
                for (int a = 1; a <= 255; a++)
                {
                    for (int b = 1; b <= 255; b++)
                    {
                        if (GFmul(a, b, IRR_MULL_OFFSET) == 1)
                        {
                            GFMULINV_LOOKUP[IRR_MULLINV_OFFSET + a] = (byte)b;
                        }
                    }
                }
            }
        }
        private void BuildJShiftMaps()
        {
            int[] preIndex = new int[64]
            {
                00, 01, 02, 03, 04, 05, 06, 07,
                08, 09, 10, 11, 12, 13, 14, 15,
                16, 17, 18, 19, 20, 21, 22, 23,
                24, 25, 26, 27, 28, 29, 30, 31,
                32, 33, 34, 35, 36, 37, 38, 39,
                40, 41, 42, 43, 44, 45, 46, 47,
                48, 49, 50, 51, 52, 53, 54, 55,
                56, 57, 58, 59, 60, 61, 62, 63
            };
            int[] index = new int[64];
            for (int i = 0; i < NUM_IRREDUCIBLEPOLY; i++)
            {
                preIndex.CopyTo(index, 0);
                for (int j = 0; j < index.Length; j++) //round map
                {
                    SWAP(ref index, j, PRIMES[EXP[(i << 6) + j]] & 63);
                }
                index.CopyTo(JMAP, i << 6);
                for (int j = 0; j < index.Length; j++) //round inverse map
                {
                    INV_JMAP[(i << 6) + index[j]] = j;
                }
            }
        }
        private void BuildGFMIrrPolys()
        {
            for (int i = 0; i < GFM_IRRPOLYINDEX.Length; i++)
            {
                GFM_IRRPOLYINDEX[i] = RHASH[i] % NUM_IRREDUCIBLEPOLY;
            }
        }
        /// <summary>
        /// SOURCE CODE FROM WIKI, Modified to recieve all irreducible polynomials in GF(256)
        /// </summary>
        /// <param name="a"></param>
        /// <param name="b"></param>
        /// <param name="IRR"></param>
        /// <returns></returns>
        private byte GMul(byte a, byte b, int IRR_INDEX) //FROM WIKI
        { // Galois Field (256) Multiplication of two Bytes
            byte p = 0;

            for (int counter = 0; counter < 8; counter++)
            {
                if ((b & 1) != 0)
                {
                    p ^= a;
                }

                bool hi_bit_set = (a & 0x80) != 0;
                a <<= 1;
                if (hi_bit_set)
                {
                    a ^= (byte)IRREDUCIBLEPOLY[IRR_INDEX];
                }
                b >>= 1;
            }

            return p;
        }
        private byte GFmul(int a, int b, int IRR_OFFSET)
        {
            return GFMUL_LOOKUP[IRR_OFFSET + (a << 8) + b];
        }
        private byte GFmulInv(int a, int IRR_OFFSET)
        {
            return GFMULINV_LOOKUP[IRR_OFFSET + a];
        }

        /// <summary>
        /// Calculates the inverse matrix 8x8 over GF(2^8)
        /// </summary>
        /// <param name="MAT8X8">byte[64] represented as 8x8 matrix</param>
        /// <param name="IRR_INDEX">RANGE FROM 0-30 Inclusive</param>
        public void R_GFM(ref byte[] MAT8X8, int IRR_INDEX)
        {
            //Obtain Augmented Matrix
            MAT8X8.CopyTo(GFM_TMP, 0);
            GFM_NULL.CopyTo(GFM_AUG, 0); //memset augmented matrix to 0
            int IRR_MUL_OFFSET = IRR_INDEX << 16;
            int IRR_INVMUL_OFFSET = IRR_INDEX << 8;

            if (GFM_TMP[0] != 0)
            {
                GFM_AUG[0] = GFmulInv(GFM_TMP[0], IRR_INVMUL_OFFSET);
                GFM_TMP[1] = GFmul(GFM_TMP[1], GFM_AUG[0], IRR_MUL_OFFSET);
                GFM_TMP[2] = GFmul(GFM_TMP[2], GFM_AUG[0], IRR_MUL_OFFSET);
                GFM_TMP[3] = GFmul(GFM_TMP[3], GFM_AUG[0], IRR_MUL_OFFSET);
                GFM_TMP[4] = GFmul(GFM_TMP[4], GFM_AUG[0], IRR_MUL_OFFSET);
                GFM_TMP[5] = GFmul(GFM_TMP[5], GFM_AUG[0], IRR_MUL_OFFSET);
                GFM_TMP[6] = GFmul(GFM_TMP[6], GFM_AUG[0], IRR_MUL_OFFSET);
                GFM_TMP[7] = GFmul(GFM_TMP[7], GFM_AUG[0], IRR_MUL_OFFSET);

                GFM_AUG[8] = GFmul(GFM_AUG[0], GFM_TMP[8], IRR_MUL_OFFSET);
                GFM_TMP[9] = (byte)(GFM_TMP[9] ^ GFmul(GFM_TMP[1], GFM_TMP[8], IRR_MUL_OFFSET));
                GFM_TMP[10] = (byte)(GFM_TMP[10] ^ GFmul(GFM_TMP[2], GFM_TMP[8], IRR_MUL_OFFSET));
                GFM_TMP[11] = (byte)(GFM_TMP[11] ^ GFmul(GFM_TMP[3], GFM_TMP[8], IRR_MUL_OFFSET));
                GFM_TMP[12] = (byte)(GFM_TMP[12] ^ GFmul(GFM_TMP[4], GFM_TMP[8], IRR_MUL_OFFSET));
                GFM_TMP[13] = (byte)(GFM_TMP[13] ^ GFmul(GFM_TMP[5], GFM_TMP[8], IRR_MUL_OFFSET));
                GFM_TMP[14] = (byte)(GFM_TMP[14] ^ GFmul(GFM_TMP[6], GFM_TMP[8], IRR_MUL_OFFSET));
                GFM_TMP[15] = (byte)(GFM_TMP[15] ^ GFmul(GFM_TMP[7], GFM_TMP[8], IRR_MUL_OFFSET));

                GFM_AUG[16] = GFmul(GFM_AUG[0], GFM_TMP[16], IRR_MUL_OFFSET);
                GFM_TMP[17] = (byte)(GFM_TMP[17] ^ GFmul(GFM_TMP[1], GFM_TMP[16], IRR_MUL_OFFSET));
                GFM_TMP[18] = (byte)(GFM_TMP[18] ^ GFmul(GFM_TMP[2], GFM_TMP[16], IRR_MUL_OFFSET));
                GFM_TMP[19] = (byte)(GFM_TMP[19] ^ GFmul(GFM_TMP[3], GFM_TMP[16], IRR_MUL_OFFSET));
                GFM_TMP[20] = (byte)(GFM_TMP[20] ^ GFmul(GFM_TMP[4], GFM_TMP[16], IRR_MUL_OFFSET));
                GFM_TMP[21] = (byte)(GFM_TMP[21] ^ GFmul(GFM_TMP[5], GFM_TMP[16], IRR_MUL_OFFSET));
                GFM_TMP[22] = (byte)(GFM_TMP[22] ^ GFmul(GFM_TMP[6], GFM_TMP[16], IRR_MUL_OFFSET));
                GFM_TMP[23] = (byte)(GFM_TMP[23] ^ GFmul(GFM_TMP[7], GFM_TMP[16], IRR_MUL_OFFSET));

                GFM_AUG[24] = GFmul(GFM_AUG[0], GFM_TMP[24], IRR_MUL_OFFSET);
                GFM_TMP[25] = (byte)(GFM_TMP[25] ^ GFmul(GFM_TMP[1], GFM_TMP[24], IRR_MUL_OFFSET));
                GFM_TMP[26] = (byte)(GFM_TMP[26] ^ GFmul(GFM_TMP[2], GFM_TMP[24], IRR_MUL_OFFSET));
                GFM_TMP[27] = (byte)(GFM_TMP[27] ^ GFmul(GFM_TMP[3], GFM_TMP[24], IRR_MUL_OFFSET));
                GFM_TMP[28] = (byte)(GFM_TMP[28] ^ GFmul(GFM_TMP[4], GFM_TMP[24], IRR_MUL_OFFSET));
                GFM_TMP[29] = (byte)(GFM_TMP[29] ^ GFmul(GFM_TMP[5], GFM_TMP[24], IRR_MUL_OFFSET));
                GFM_TMP[30] = (byte)(GFM_TMP[30] ^ GFmul(GFM_TMP[6], GFM_TMP[24], IRR_MUL_OFFSET));
                GFM_TMP[31] = (byte)(GFM_TMP[31] ^ GFmul(GFM_TMP[7], GFM_TMP[24], IRR_MUL_OFFSET));

                GFM_AUG[32] = GFmul(GFM_AUG[0], GFM_TMP[32], IRR_MUL_OFFSET);
                GFM_TMP[33] = (byte)(GFM_TMP[33] ^ GFmul(GFM_TMP[1], GFM_TMP[32], IRR_MUL_OFFSET));
                GFM_TMP[34] = (byte)(GFM_TMP[34] ^ GFmul(GFM_TMP[2], GFM_TMP[32], IRR_MUL_OFFSET));
                GFM_TMP[35] = (byte)(GFM_TMP[35] ^ GFmul(GFM_TMP[3], GFM_TMP[32], IRR_MUL_OFFSET));
                GFM_TMP[36] = (byte)(GFM_TMP[36] ^ GFmul(GFM_TMP[4], GFM_TMP[32], IRR_MUL_OFFSET));
                GFM_TMP[37] = (byte)(GFM_TMP[37] ^ GFmul(GFM_TMP[5], GFM_TMP[32], IRR_MUL_OFFSET));
                GFM_TMP[38] = (byte)(GFM_TMP[38] ^ GFmul(GFM_TMP[6], GFM_TMP[32], IRR_MUL_OFFSET));
                GFM_TMP[39] = (byte)(GFM_TMP[39] ^ GFmul(GFM_TMP[7], GFM_TMP[32], IRR_MUL_OFFSET));

                GFM_AUG[40] = GFmul(GFM_AUG[0], GFM_TMP[40], IRR_MUL_OFFSET);
                GFM_TMP[41] = (byte)(GFM_TMP[41] ^ GFmul(GFM_TMP[1], GFM_TMP[40], IRR_MUL_OFFSET));
                GFM_TMP[42] = (byte)(GFM_TMP[42] ^ GFmul(GFM_TMP[2], GFM_TMP[40], IRR_MUL_OFFSET));
                GFM_TMP[43] = (byte)(GFM_TMP[43] ^ GFmul(GFM_TMP[3], GFM_TMP[40], IRR_MUL_OFFSET));
                GFM_TMP[44] = (byte)(GFM_TMP[44] ^ GFmul(GFM_TMP[4], GFM_TMP[40], IRR_MUL_OFFSET));
                GFM_TMP[45] = (byte)(GFM_TMP[45] ^ GFmul(GFM_TMP[5], GFM_TMP[40], IRR_MUL_OFFSET));
                GFM_TMP[46] = (byte)(GFM_TMP[46] ^ GFmul(GFM_TMP[6], GFM_TMP[40], IRR_MUL_OFFSET));
                GFM_TMP[47] = (byte)(GFM_TMP[47] ^ GFmul(GFM_TMP[7], GFM_TMP[40], IRR_MUL_OFFSET));

                GFM_AUG[48] = GFmul(GFM_AUG[0], GFM_TMP[48], IRR_MUL_OFFSET);
                GFM_TMP[49] = (byte)(GFM_TMP[49] ^ GFmul(GFM_TMP[1], GFM_TMP[48], IRR_MUL_OFFSET));
                GFM_TMP[50] = (byte)(GFM_TMP[50] ^ GFmul(GFM_TMP[2], GFM_TMP[48], IRR_MUL_OFFSET));
                GFM_TMP[51] = (byte)(GFM_TMP[51] ^ GFmul(GFM_TMP[3], GFM_TMP[48], IRR_MUL_OFFSET));
                GFM_TMP[52] = (byte)(GFM_TMP[52] ^ GFmul(GFM_TMP[4], GFM_TMP[48], IRR_MUL_OFFSET));
                GFM_TMP[53] = (byte)(GFM_TMP[53] ^ GFmul(GFM_TMP[5], GFM_TMP[48], IRR_MUL_OFFSET));
                GFM_TMP[54] = (byte)(GFM_TMP[54] ^ GFmul(GFM_TMP[6], GFM_TMP[48], IRR_MUL_OFFSET));
                GFM_TMP[55] = (byte)(GFM_TMP[55] ^ GFmul(GFM_TMP[7], GFM_TMP[48], IRR_MUL_OFFSET));

                GFM_AUG[56] = GFmul(GFM_AUG[0], GFM_TMP[56], IRR_MUL_OFFSET);
                GFM_TMP[57] = (byte)(GFM_TMP[57] ^ GFmul(GFM_TMP[1], GFM_TMP[56], IRR_MUL_OFFSET));
                GFM_TMP[58] = (byte)(GFM_TMP[58] ^ GFmul(GFM_TMP[2], GFM_TMP[56], IRR_MUL_OFFSET));
                GFM_TMP[59] = (byte)(GFM_TMP[59] ^ GFmul(GFM_TMP[3], GFM_TMP[56], IRR_MUL_OFFSET));
                GFM_TMP[60] = (byte)(GFM_TMP[60] ^ GFmul(GFM_TMP[4], GFM_TMP[56], IRR_MUL_OFFSET));
                GFM_TMP[61] = (byte)(GFM_TMP[61] ^ GFmul(GFM_TMP[5], GFM_TMP[56], IRR_MUL_OFFSET));
                GFM_TMP[62] = (byte)(GFM_TMP[62] ^ GFmul(GFM_TMP[6], GFM_TMP[56], IRR_MUL_OFFSET));
                GFM_TMP[63] = (byte)(GFM_TMP[63] ^ GFmul(GFM_TMP[7], GFM_TMP[56], IRR_MUL_OFFSET));

                if (GFM_TMP[9] != 0)
                {
                    GFM_AUG[9] = GFmulInv(GFM_TMP[9], IRR_INVMUL_OFFSET);
                    GFM_TMP[10] = GFmul(GFM_TMP[10], GFM_AUG[9], IRR_MUL_OFFSET);
                    GFM_TMP[11] = GFmul(GFM_TMP[11], GFM_AUG[9], IRR_MUL_OFFSET);
                    GFM_TMP[12] = GFmul(GFM_TMP[12], GFM_AUG[9], IRR_MUL_OFFSET);
                    GFM_TMP[13] = GFmul(GFM_TMP[13], GFM_AUG[9], IRR_MUL_OFFSET);
                    GFM_TMP[14] = GFmul(GFM_TMP[14], GFM_AUG[9], IRR_MUL_OFFSET);
                    GFM_TMP[15] = GFmul(GFM_TMP[15], GFM_AUG[9], IRR_MUL_OFFSET);
                    GFM_AUG[8] = GFmul(GFM_AUG[8], GFM_AUG[9], IRR_MUL_OFFSET);

                    GFM_AUG[1] = GFmul(GFM_AUG[9], GFM_TMP[1], IRR_MUL_OFFSET);
                    GFM_AUG[0] = (byte)(GFM_AUG[0] ^ GFmul(GFM_AUG[8], GFM_TMP[1], IRR_MUL_OFFSET));
                    GFM_TMP[2] = (byte)(GFM_TMP[2] ^ GFmul(GFM_TMP[10], GFM_TMP[1], IRR_MUL_OFFSET));
                    GFM_TMP[3] = (byte)(GFM_TMP[3] ^ GFmul(GFM_TMP[11], GFM_TMP[1], IRR_MUL_OFFSET));
                    GFM_TMP[4] = (byte)(GFM_TMP[4] ^ GFmul(GFM_TMP[12], GFM_TMP[1], IRR_MUL_OFFSET));
                    GFM_TMP[5] = (byte)(GFM_TMP[5] ^ GFmul(GFM_TMP[13], GFM_TMP[1], IRR_MUL_OFFSET));
                    GFM_TMP[6] = (byte)(GFM_TMP[6] ^ GFmul(GFM_TMP[14], GFM_TMP[1], IRR_MUL_OFFSET));
                    GFM_TMP[7] = (byte)(GFM_TMP[7] ^ GFmul(GFM_TMP[15], GFM_TMP[1], IRR_MUL_OFFSET));

                    GFM_AUG[17] = GFmul(GFM_AUG[9], GFM_TMP[17], IRR_MUL_OFFSET);
                    GFM_AUG[16] = (byte)(GFM_AUG[16] ^ GFmul(GFM_AUG[8], GFM_TMP[17], IRR_MUL_OFFSET));
                    GFM_TMP[18] = (byte)(GFM_TMP[18] ^ GFmul(GFM_TMP[10], GFM_TMP[17], IRR_MUL_OFFSET));
                    GFM_TMP[19] = (byte)(GFM_TMP[19] ^ GFmul(GFM_TMP[11], GFM_TMP[17], IRR_MUL_OFFSET));
                    GFM_TMP[20] = (byte)(GFM_TMP[20] ^ GFmul(GFM_TMP[12], GFM_TMP[17], IRR_MUL_OFFSET));
                    GFM_TMP[21] = (byte)(GFM_TMP[21] ^ GFmul(GFM_TMP[13], GFM_TMP[17], IRR_MUL_OFFSET));
                    GFM_TMP[22] = (byte)(GFM_TMP[22] ^ GFmul(GFM_TMP[14], GFM_TMP[17], IRR_MUL_OFFSET));
                    GFM_TMP[23] = (byte)(GFM_TMP[23] ^ GFmul(GFM_TMP[15], GFM_TMP[17], IRR_MUL_OFFSET));

                    GFM_AUG[25] = GFmul(GFM_AUG[9], GFM_TMP[25], IRR_MUL_OFFSET);
                    GFM_AUG[24] = (byte)(GFM_AUG[24] ^ GFmul(GFM_AUG[8], GFM_TMP[25], IRR_MUL_OFFSET));
                    GFM_TMP[26] = (byte)(GFM_TMP[26] ^ GFmul(GFM_TMP[10], GFM_TMP[25], IRR_MUL_OFFSET));
                    GFM_TMP[27] = (byte)(GFM_TMP[27] ^ GFmul(GFM_TMP[11], GFM_TMP[25], IRR_MUL_OFFSET));
                    GFM_TMP[28] = (byte)(GFM_TMP[28] ^ GFmul(GFM_TMP[12], GFM_TMP[25], IRR_MUL_OFFSET));
                    GFM_TMP[29] = (byte)(GFM_TMP[29] ^ GFmul(GFM_TMP[13], GFM_TMP[25], IRR_MUL_OFFSET));
                    GFM_TMP[30] = (byte)(GFM_TMP[30] ^ GFmul(GFM_TMP[14], GFM_TMP[25], IRR_MUL_OFFSET));
                    GFM_TMP[31] = (byte)(GFM_TMP[31] ^ GFmul(GFM_TMP[15], GFM_TMP[25], IRR_MUL_OFFSET));

                    GFM_AUG[33] = GFmul(GFM_AUG[9], GFM_TMP[33], IRR_MUL_OFFSET);
                    GFM_AUG[32] = (byte)(GFM_AUG[32] ^ GFmul(GFM_AUG[8], GFM_TMP[33], IRR_MUL_OFFSET));
                    GFM_TMP[34] = (byte)(GFM_TMP[34] ^ GFmul(GFM_TMP[10], GFM_TMP[33], IRR_MUL_OFFSET));
                    GFM_TMP[35] = (byte)(GFM_TMP[35] ^ GFmul(GFM_TMP[11], GFM_TMP[33], IRR_MUL_OFFSET));
                    GFM_TMP[36] = (byte)(GFM_TMP[36] ^ GFmul(GFM_TMP[12], GFM_TMP[33], IRR_MUL_OFFSET));
                    GFM_TMP[37] = (byte)(GFM_TMP[37] ^ GFmul(GFM_TMP[13], GFM_TMP[33], IRR_MUL_OFFSET));
                    GFM_TMP[38] = (byte)(GFM_TMP[38] ^ GFmul(GFM_TMP[14], GFM_TMP[33], IRR_MUL_OFFSET));
                    GFM_TMP[39] = (byte)(GFM_TMP[39] ^ GFmul(GFM_TMP[15], GFM_TMP[33], IRR_MUL_OFFSET));

                    GFM_AUG[41] = GFmul(GFM_AUG[9], GFM_TMP[41], IRR_MUL_OFFSET);
                    GFM_AUG[40] = (byte)(GFM_AUG[40] ^ GFmul(GFM_AUG[8], GFM_TMP[41], IRR_MUL_OFFSET));
                    GFM_TMP[42] = (byte)(GFM_TMP[42] ^ GFmul(GFM_TMP[10], GFM_TMP[41], IRR_MUL_OFFSET));
                    GFM_TMP[43] = (byte)(GFM_TMP[43] ^ GFmul(GFM_TMP[11], GFM_TMP[41], IRR_MUL_OFFSET));
                    GFM_TMP[44] = (byte)(GFM_TMP[44] ^ GFmul(GFM_TMP[12], GFM_TMP[41], IRR_MUL_OFFSET));
                    GFM_TMP[45] = (byte)(GFM_TMP[45] ^ GFmul(GFM_TMP[13], GFM_TMP[41], IRR_MUL_OFFSET));
                    GFM_TMP[46] = (byte)(GFM_TMP[46] ^ GFmul(GFM_TMP[14], GFM_TMP[41], IRR_MUL_OFFSET));
                    GFM_TMP[47] = (byte)(GFM_TMP[47] ^ GFmul(GFM_TMP[15], GFM_TMP[41], IRR_MUL_OFFSET));

                    GFM_AUG[49] = GFmul(GFM_AUG[9], GFM_TMP[49], IRR_MUL_OFFSET);
                    GFM_AUG[48] = (byte)(GFM_AUG[48] ^ GFmul(GFM_AUG[8], GFM_TMP[49], IRR_MUL_OFFSET));
                    GFM_TMP[50] = (byte)(GFM_TMP[50] ^ GFmul(GFM_TMP[10], GFM_TMP[49], IRR_MUL_OFFSET));
                    GFM_TMP[51] = (byte)(GFM_TMP[51] ^ GFmul(GFM_TMP[11], GFM_TMP[49], IRR_MUL_OFFSET));
                    GFM_TMP[52] = (byte)(GFM_TMP[52] ^ GFmul(GFM_TMP[12], GFM_TMP[49], IRR_MUL_OFFSET));
                    GFM_TMP[53] = (byte)(GFM_TMP[53] ^ GFmul(GFM_TMP[13], GFM_TMP[49], IRR_MUL_OFFSET));
                    GFM_TMP[54] = (byte)(GFM_TMP[54] ^ GFmul(GFM_TMP[14], GFM_TMP[49], IRR_MUL_OFFSET));
                    GFM_TMP[55] = (byte)(GFM_TMP[55] ^ GFmul(GFM_TMP[15], GFM_TMP[49], IRR_MUL_OFFSET));

                    GFM_AUG[57] = GFmul(GFM_AUG[9], GFM_TMP[57], IRR_MUL_OFFSET);
                    GFM_AUG[56] = (byte)(GFM_AUG[56] ^ GFmul(GFM_AUG[8], GFM_TMP[57], IRR_MUL_OFFSET));
                    GFM_TMP[58] = (byte)(GFM_TMP[58] ^ GFmul(GFM_TMP[10], GFM_TMP[57], IRR_MUL_OFFSET));
                    GFM_TMP[59] = (byte)(GFM_TMP[59] ^ GFmul(GFM_TMP[11], GFM_TMP[57], IRR_MUL_OFFSET));
                    GFM_TMP[60] = (byte)(GFM_TMP[60] ^ GFmul(GFM_TMP[12], GFM_TMP[57], IRR_MUL_OFFSET));
                    GFM_TMP[61] = (byte)(GFM_TMP[61] ^ GFmul(GFM_TMP[13], GFM_TMP[57], IRR_MUL_OFFSET));
                    GFM_TMP[62] = (byte)(GFM_TMP[62] ^ GFmul(GFM_TMP[14], GFM_TMP[57], IRR_MUL_OFFSET));
                    GFM_TMP[63] = (byte)(GFM_TMP[63] ^ GFmul(GFM_TMP[15], GFM_TMP[57], IRR_MUL_OFFSET));

                    if (GFM_TMP[18] != 0)
                    {
                        GFM_AUG[18] = GFmulInv(GFM_TMP[18], IRR_INVMUL_OFFSET);
                        GFM_TMP[19] = GFmul(GFM_TMP[19], GFM_AUG[18], IRR_MUL_OFFSET);
                        GFM_TMP[20] = GFmul(GFM_TMP[20], GFM_AUG[18], IRR_MUL_OFFSET);
                        GFM_TMP[21] = GFmul(GFM_TMP[21], GFM_AUG[18], IRR_MUL_OFFSET);
                        GFM_TMP[22] = GFmul(GFM_TMP[22], GFM_AUG[18], IRR_MUL_OFFSET);
                        GFM_TMP[23] = GFmul(GFM_TMP[23], GFM_AUG[18], IRR_MUL_OFFSET);
                        GFM_AUG[17] = GFmul(GFM_AUG[17], GFM_AUG[18], IRR_MUL_OFFSET);
                        GFM_AUG[16] = GFmul(GFM_AUG[16], GFM_AUG[18], IRR_MUL_OFFSET);

                        GFM_AUG[2] = GFmul(GFM_AUG[18], GFM_TMP[2], IRR_MUL_OFFSET);
                        GFM_AUG[1] = (byte)(GFM_AUG[1] ^ GFmul(GFM_AUG[17], GFM_TMP[2], IRR_MUL_OFFSET));
                        GFM_AUG[0] = (byte)(GFM_AUG[0] ^ GFmul(GFM_AUG[16], GFM_TMP[2], IRR_MUL_OFFSET));
                        GFM_TMP[3] = (byte)(GFM_TMP[3] ^ GFmul(GFM_TMP[19], GFM_TMP[2], IRR_MUL_OFFSET));
                        GFM_TMP[4] = (byte)(GFM_TMP[4] ^ GFmul(GFM_TMP[20], GFM_TMP[2], IRR_MUL_OFFSET));
                        GFM_TMP[5] = (byte)(GFM_TMP[5] ^ GFmul(GFM_TMP[21], GFM_TMP[2], IRR_MUL_OFFSET));
                        GFM_TMP[6] = (byte)(GFM_TMP[6] ^ GFmul(GFM_TMP[22], GFM_TMP[2], IRR_MUL_OFFSET));
                        GFM_TMP[7] = (byte)(GFM_TMP[7] ^ GFmul(GFM_TMP[23], GFM_TMP[2], IRR_MUL_OFFSET));

                        GFM_AUG[10] = GFmul(GFM_AUG[18], GFM_TMP[10], IRR_MUL_OFFSET);
                        GFM_AUG[9] = (byte)(GFM_AUG[9] ^ GFmul(GFM_AUG[17], GFM_TMP[10], IRR_MUL_OFFSET));
                        GFM_AUG[8] = (byte)(GFM_AUG[8] ^ GFmul(GFM_AUG[16], GFM_TMP[10], IRR_MUL_OFFSET));
                        GFM_TMP[11] = (byte)(GFM_TMP[11] ^ GFmul(GFM_TMP[19], GFM_TMP[10], IRR_MUL_OFFSET));
                        GFM_TMP[12] = (byte)(GFM_TMP[12] ^ GFmul(GFM_TMP[20], GFM_TMP[10], IRR_MUL_OFFSET));
                        GFM_TMP[13] = (byte)(GFM_TMP[13] ^ GFmul(GFM_TMP[21], GFM_TMP[10], IRR_MUL_OFFSET));
                        GFM_TMP[14] = (byte)(GFM_TMP[14] ^ GFmul(GFM_TMP[22], GFM_TMP[10], IRR_MUL_OFFSET));
                        GFM_TMP[15] = (byte)(GFM_TMP[15] ^ GFmul(GFM_TMP[23], GFM_TMP[10], IRR_MUL_OFFSET));

                        GFM_AUG[26] = GFmul(GFM_AUG[18], GFM_TMP[26], IRR_MUL_OFFSET);
                        GFM_AUG[25] = (byte)(GFM_AUG[25] ^ GFmul(GFM_AUG[17], GFM_TMP[26], IRR_MUL_OFFSET));
                        GFM_AUG[24] = (byte)(GFM_AUG[24] ^ GFmul(GFM_AUG[16], GFM_TMP[26], IRR_MUL_OFFSET));
                        GFM_TMP[27] = (byte)(GFM_TMP[27] ^ GFmul(GFM_TMP[19], GFM_TMP[26], IRR_MUL_OFFSET));
                        GFM_TMP[28] = (byte)(GFM_TMP[28] ^ GFmul(GFM_TMP[20], GFM_TMP[26], IRR_MUL_OFFSET));
                        GFM_TMP[29] = (byte)(GFM_TMP[29] ^ GFmul(GFM_TMP[21], GFM_TMP[26], IRR_MUL_OFFSET));
                        GFM_TMP[30] = (byte)(GFM_TMP[30] ^ GFmul(GFM_TMP[22], GFM_TMP[26], IRR_MUL_OFFSET));
                        GFM_TMP[31] = (byte)(GFM_TMP[31] ^ GFmul(GFM_TMP[23], GFM_TMP[26], IRR_MUL_OFFSET));

                        GFM_AUG[34] = GFmul(GFM_AUG[18], GFM_TMP[34], IRR_MUL_OFFSET);
                        GFM_AUG[33] = (byte)(GFM_AUG[33] ^ GFmul(GFM_AUG[17], GFM_TMP[34], IRR_MUL_OFFSET));
                        GFM_AUG[32] = (byte)(GFM_AUG[32] ^ GFmul(GFM_AUG[16], GFM_TMP[34], IRR_MUL_OFFSET));
                        GFM_TMP[35] = (byte)(GFM_TMP[35] ^ GFmul(GFM_TMP[19], GFM_TMP[34], IRR_MUL_OFFSET));
                        GFM_TMP[36] = (byte)(GFM_TMP[36] ^ GFmul(GFM_TMP[20], GFM_TMP[34], IRR_MUL_OFFSET));
                        GFM_TMP[37] = (byte)(GFM_TMP[37] ^ GFmul(GFM_TMP[21], GFM_TMP[34], IRR_MUL_OFFSET));
                        GFM_TMP[38] = (byte)(GFM_TMP[38] ^ GFmul(GFM_TMP[22], GFM_TMP[34], IRR_MUL_OFFSET));
                        GFM_TMP[39] = (byte)(GFM_TMP[39] ^ GFmul(GFM_TMP[23], GFM_TMP[34], IRR_MUL_OFFSET));

                        GFM_AUG[42] = GFmul(GFM_AUG[18], GFM_TMP[42], IRR_MUL_OFFSET);
                        GFM_AUG[41] = (byte)(GFM_AUG[41] ^ GFmul(GFM_AUG[17], GFM_TMP[42], IRR_MUL_OFFSET));
                        GFM_AUG[40] = (byte)(GFM_AUG[40] ^ GFmul(GFM_AUG[16], GFM_TMP[42], IRR_MUL_OFFSET));
                        GFM_TMP[43] = (byte)(GFM_TMP[43] ^ GFmul(GFM_TMP[19], GFM_TMP[42], IRR_MUL_OFFSET));
                        GFM_TMP[44] = (byte)(GFM_TMP[44] ^ GFmul(GFM_TMP[20], GFM_TMP[42], IRR_MUL_OFFSET));
                        GFM_TMP[45] = (byte)(GFM_TMP[45] ^ GFmul(GFM_TMP[21], GFM_TMP[42], IRR_MUL_OFFSET));
                        GFM_TMP[46] = (byte)(GFM_TMP[46] ^ GFmul(GFM_TMP[22], GFM_TMP[42], IRR_MUL_OFFSET));
                        GFM_TMP[47] = (byte)(GFM_TMP[47] ^ GFmul(GFM_TMP[23], GFM_TMP[42], IRR_MUL_OFFSET));

                        GFM_AUG[50] = GFmul(GFM_AUG[18], GFM_TMP[50], IRR_MUL_OFFSET);
                        GFM_AUG[49] = (byte)(GFM_AUG[49] ^ GFmul(GFM_AUG[17], GFM_TMP[50], IRR_MUL_OFFSET));
                        GFM_AUG[48] = (byte)(GFM_AUG[48] ^ GFmul(GFM_AUG[16], GFM_TMP[50], IRR_MUL_OFFSET));
                        GFM_TMP[51] = (byte)(GFM_TMP[51] ^ GFmul(GFM_TMP[19], GFM_TMP[50], IRR_MUL_OFFSET));
                        GFM_TMP[52] = (byte)(GFM_TMP[52] ^ GFmul(GFM_TMP[20], GFM_TMP[50], IRR_MUL_OFFSET));
                        GFM_TMP[53] = (byte)(GFM_TMP[53] ^ GFmul(GFM_TMP[21], GFM_TMP[50], IRR_MUL_OFFSET));
                        GFM_TMP[54] = (byte)(GFM_TMP[54] ^ GFmul(GFM_TMP[22], GFM_TMP[50], IRR_MUL_OFFSET));
                        GFM_TMP[55] = (byte)(GFM_TMP[55] ^ GFmul(GFM_TMP[23], GFM_TMP[50], IRR_MUL_OFFSET));

                        GFM_AUG[58] = GFmul(GFM_AUG[18], GFM_TMP[58], IRR_MUL_OFFSET);
                        GFM_AUG[57] = (byte)(GFM_AUG[57] ^ GFmul(GFM_AUG[17], GFM_TMP[58], IRR_MUL_OFFSET));
                        GFM_AUG[56] = (byte)(GFM_AUG[56] ^ GFmul(GFM_AUG[16], GFM_TMP[58], IRR_MUL_OFFSET));
                        GFM_TMP[59] = (byte)(GFM_TMP[59] ^ GFmul(GFM_TMP[19], GFM_TMP[58], IRR_MUL_OFFSET));
                        GFM_TMP[60] = (byte)(GFM_TMP[60] ^ GFmul(GFM_TMP[20], GFM_TMP[58], IRR_MUL_OFFSET));
                        GFM_TMP[61] = (byte)(GFM_TMP[61] ^ GFmul(GFM_TMP[21], GFM_TMP[58], IRR_MUL_OFFSET));
                        GFM_TMP[62] = (byte)(GFM_TMP[62] ^ GFmul(GFM_TMP[22], GFM_TMP[58], IRR_MUL_OFFSET));
                        GFM_TMP[63] = (byte)(GFM_TMP[63] ^ GFmul(GFM_TMP[23], GFM_TMP[58], IRR_MUL_OFFSET));

                        if (GFM_TMP[27] != 0)
                        {
                            GFM_AUG[27] = GFmulInv(GFM_TMP[27], IRR_INVMUL_OFFSET);
                            GFM_TMP[28] = GFmul(GFM_TMP[28], GFM_AUG[27], IRR_MUL_OFFSET);
                            GFM_TMP[29] = GFmul(GFM_TMP[29], GFM_AUG[27], IRR_MUL_OFFSET);
                            GFM_TMP[30] = GFmul(GFM_TMP[30], GFM_AUG[27], IRR_MUL_OFFSET);
                            GFM_TMP[31] = GFmul(GFM_TMP[31], GFM_AUG[27], IRR_MUL_OFFSET);
                            GFM_AUG[26] = GFmul(GFM_AUG[26], GFM_AUG[27], IRR_MUL_OFFSET);
                            GFM_AUG[25] = GFmul(GFM_AUG[25], GFM_AUG[27], IRR_MUL_OFFSET);
                            GFM_AUG[24] = GFmul(GFM_AUG[24], GFM_AUG[27], IRR_MUL_OFFSET);

                            GFM_AUG[3] = GFmul(GFM_AUG[27], GFM_TMP[3], IRR_MUL_OFFSET);
                            GFM_AUG[2] = (byte)(GFM_AUG[2] ^ GFmul(GFM_AUG[26], GFM_TMP[3], IRR_MUL_OFFSET));
                            GFM_AUG[1] = (byte)(GFM_AUG[1] ^ GFmul(GFM_AUG[25], GFM_TMP[3], IRR_MUL_OFFSET));
                            GFM_AUG[0] = (byte)(GFM_AUG[0] ^ GFmul(GFM_AUG[24], GFM_TMP[3], IRR_MUL_OFFSET));
                            GFM_TMP[4] = (byte)(GFM_TMP[4] ^ GFmul(GFM_TMP[28], GFM_TMP[3], IRR_MUL_OFFSET));
                            GFM_TMP[5] = (byte)(GFM_TMP[5] ^ GFmul(GFM_TMP[29], GFM_TMP[3], IRR_MUL_OFFSET));
                            GFM_TMP[6] = (byte)(GFM_TMP[6] ^ GFmul(GFM_TMP[30], GFM_TMP[3], IRR_MUL_OFFSET));
                            GFM_TMP[7] = (byte)(GFM_TMP[7] ^ GFmul(GFM_TMP[31], GFM_TMP[3], IRR_MUL_OFFSET));

                            GFM_AUG[11] = GFmul(GFM_AUG[27], GFM_TMP[11], IRR_MUL_OFFSET);
                            GFM_AUG[10] = (byte)(GFM_AUG[10] ^ GFmul(GFM_AUG[26], GFM_TMP[11], IRR_MUL_OFFSET));
                            GFM_AUG[9] = (byte)(GFM_AUG[9] ^ GFmul(GFM_AUG[25], GFM_TMP[11], IRR_MUL_OFFSET));
                            GFM_AUG[8] = (byte)(GFM_AUG[8] ^ GFmul(GFM_AUG[24], GFM_TMP[11], IRR_MUL_OFFSET));
                            GFM_TMP[12] = (byte)(GFM_TMP[12] ^ GFmul(GFM_TMP[28], GFM_TMP[11], IRR_MUL_OFFSET));
                            GFM_TMP[13] = (byte)(GFM_TMP[13] ^ GFmul(GFM_TMP[29], GFM_TMP[11], IRR_MUL_OFFSET));
                            GFM_TMP[14] = (byte)(GFM_TMP[14] ^ GFmul(GFM_TMP[30], GFM_TMP[11], IRR_MUL_OFFSET));
                            GFM_TMP[15] = (byte)(GFM_TMP[15] ^ GFmul(GFM_TMP[31], GFM_TMP[11], IRR_MUL_OFFSET));

                            GFM_AUG[19] = GFmul(GFM_AUG[27], GFM_TMP[19], IRR_MUL_OFFSET);
                            GFM_AUG[18] = (byte)(GFM_AUG[18] ^ GFmul(GFM_AUG[26], GFM_TMP[19], IRR_MUL_OFFSET));
                            GFM_AUG[17] = (byte)(GFM_AUG[17] ^ GFmul(GFM_AUG[25], GFM_TMP[19], IRR_MUL_OFFSET));
                            GFM_AUG[16] = (byte)(GFM_AUG[16] ^ GFmul(GFM_AUG[24], GFM_TMP[19], IRR_MUL_OFFSET));
                            GFM_TMP[20] = (byte)(GFM_TMP[20] ^ GFmul(GFM_TMP[28], GFM_TMP[19], IRR_MUL_OFFSET));
                            GFM_TMP[21] = (byte)(GFM_TMP[21] ^ GFmul(GFM_TMP[29], GFM_TMP[19], IRR_MUL_OFFSET));
                            GFM_TMP[22] = (byte)(GFM_TMP[22] ^ GFmul(GFM_TMP[30], GFM_TMP[19], IRR_MUL_OFFSET));
                            GFM_TMP[23] = (byte)(GFM_TMP[23] ^ GFmul(GFM_TMP[31], GFM_TMP[19], IRR_MUL_OFFSET));

                            GFM_AUG[35] = GFmul(GFM_AUG[27], GFM_TMP[35], IRR_MUL_OFFSET);
                            GFM_AUG[34] = (byte)(GFM_AUG[34] ^ GFmul(GFM_AUG[26], GFM_TMP[35], IRR_MUL_OFFSET));
                            GFM_AUG[33] = (byte)(GFM_AUG[33] ^ GFmul(GFM_AUG[25], GFM_TMP[35], IRR_MUL_OFFSET));
                            GFM_AUG[32] = (byte)(GFM_AUG[32] ^ GFmul(GFM_AUG[24], GFM_TMP[35], IRR_MUL_OFFSET));
                            GFM_TMP[36] = (byte)(GFM_TMP[36] ^ GFmul(GFM_TMP[28], GFM_TMP[35], IRR_MUL_OFFSET));
                            GFM_TMP[37] = (byte)(GFM_TMP[37] ^ GFmul(GFM_TMP[29], GFM_TMP[35], IRR_MUL_OFFSET));
                            GFM_TMP[38] = (byte)(GFM_TMP[38] ^ GFmul(GFM_TMP[30], GFM_TMP[35], IRR_MUL_OFFSET));
                            GFM_TMP[39] = (byte)(GFM_TMP[39] ^ GFmul(GFM_TMP[31], GFM_TMP[35], IRR_MUL_OFFSET));

                            GFM_AUG[43] = GFmul(GFM_AUG[27], GFM_TMP[43], IRR_MUL_OFFSET);
                            GFM_AUG[42] = (byte)(GFM_AUG[42] ^ GFmul(GFM_AUG[26], GFM_TMP[43], IRR_MUL_OFFSET));
                            GFM_AUG[41] = (byte)(GFM_AUG[41] ^ GFmul(GFM_AUG[25], GFM_TMP[43], IRR_MUL_OFFSET));
                            GFM_AUG[40] = (byte)(GFM_AUG[40] ^ GFmul(GFM_AUG[24], GFM_TMP[43], IRR_MUL_OFFSET));
                            GFM_TMP[44] = (byte)(GFM_TMP[44] ^ GFmul(GFM_TMP[28], GFM_TMP[43], IRR_MUL_OFFSET));
                            GFM_TMP[45] = (byte)(GFM_TMP[45] ^ GFmul(GFM_TMP[29], GFM_TMP[43], IRR_MUL_OFFSET));
                            GFM_TMP[46] = (byte)(GFM_TMP[46] ^ GFmul(GFM_TMP[30], GFM_TMP[43], IRR_MUL_OFFSET));
                            GFM_TMP[47] = (byte)(GFM_TMP[47] ^ GFmul(GFM_TMP[31], GFM_TMP[43], IRR_MUL_OFFSET));

                            GFM_AUG[51] = GFmul(GFM_AUG[27], GFM_TMP[51], IRR_MUL_OFFSET);
                            GFM_AUG[50] = (byte)(GFM_AUG[50] ^ GFmul(GFM_AUG[26], GFM_TMP[51], IRR_MUL_OFFSET));
                            GFM_AUG[49] = (byte)(GFM_AUG[49] ^ GFmul(GFM_AUG[25], GFM_TMP[51], IRR_MUL_OFFSET));
                            GFM_AUG[48] = (byte)(GFM_AUG[48] ^ GFmul(GFM_AUG[24], GFM_TMP[51], IRR_MUL_OFFSET));
                            GFM_TMP[52] = (byte)(GFM_TMP[52] ^ GFmul(GFM_TMP[28], GFM_TMP[51], IRR_MUL_OFFSET));
                            GFM_TMP[53] = (byte)(GFM_TMP[53] ^ GFmul(GFM_TMP[29], GFM_TMP[51], IRR_MUL_OFFSET));
                            GFM_TMP[54] = (byte)(GFM_TMP[54] ^ GFmul(GFM_TMP[30], GFM_TMP[51], IRR_MUL_OFFSET));
                            GFM_TMP[55] = (byte)(GFM_TMP[55] ^ GFmul(GFM_TMP[31], GFM_TMP[51], IRR_MUL_OFFSET));

                            GFM_AUG[59] = GFmul(GFM_AUG[27], GFM_TMP[59], IRR_MUL_OFFSET);
                            GFM_AUG[58] = (byte)(GFM_AUG[58] ^ GFmul(GFM_AUG[26], GFM_TMP[59], IRR_MUL_OFFSET));
                            GFM_AUG[57] = (byte)(GFM_AUG[57] ^ GFmul(GFM_AUG[25], GFM_TMP[59], IRR_MUL_OFFSET));
                            GFM_AUG[56] = (byte)(GFM_AUG[56] ^ GFmul(GFM_AUG[24], GFM_TMP[59], IRR_MUL_OFFSET));
                            GFM_TMP[60] = (byte)(GFM_TMP[60] ^ GFmul(GFM_TMP[28], GFM_TMP[59], IRR_MUL_OFFSET));
                            GFM_TMP[61] = (byte)(GFM_TMP[61] ^ GFmul(GFM_TMP[29], GFM_TMP[59], IRR_MUL_OFFSET));
                            GFM_TMP[62] = (byte)(GFM_TMP[62] ^ GFmul(GFM_TMP[30], GFM_TMP[59], IRR_MUL_OFFSET));
                            GFM_TMP[63] = (byte)(GFM_TMP[63] ^ GFmul(GFM_TMP[31], GFM_TMP[59], IRR_MUL_OFFSET));

                            if (GFM_TMP[36] != 0)
                            {
                                GFM_AUG[36] = GFmulInv(GFM_TMP[36], IRR_INVMUL_OFFSET);
                                GFM_TMP[37] = GFmul(GFM_TMP[37], GFM_AUG[36], IRR_MUL_OFFSET);
                                GFM_TMP[38] = GFmul(GFM_TMP[38], GFM_AUG[36], IRR_MUL_OFFSET);
                                GFM_TMP[39] = GFmul(GFM_TMP[39], GFM_AUG[36], IRR_MUL_OFFSET);
                                GFM_AUG[35] = GFmul(GFM_AUG[35], GFM_AUG[36], IRR_MUL_OFFSET);
                                GFM_AUG[34] = GFmul(GFM_AUG[34], GFM_AUG[36], IRR_MUL_OFFSET);
                                GFM_AUG[33] = GFmul(GFM_AUG[33], GFM_AUG[36], IRR_MUL_OFFSET);
                                GFM_AUG[32] = GFmul(GFM_AUG[32], GFM_AUG[36], IRR_MUL_OFFSET);

                                GFM_AUG[4] = GFmul(GFM_AUG[36], GFM_TMP[4], IRR_MUL_OFFSET);
                                GFM_AUG[3] = (byte)(GFM_AUG[3] ^ GFmul(GFM_AUG[35], GFM_TMP[4], IRR_MUL_OFFSET));
                                GFM_AUG[2] = (byte)(GFM_AUG[2] ^ GFmul(GFM_AUG[34], GFM_TMP[4], IRR_MUL_OFFSET));
                                GFM_AUG[1] = (byte)(GFM_AUG[1] ^ GFmul(GFM_AUG[33], GFM_TMP[4], IRR_MUL_OFFSET));
                                GFM_AUG[0] = (byte)(GFM_AUG[0] ^ GFmul(GFM_AUG[32], GFM_TMP[4], IRR_MUL_OFFSET));
                                GFM_TMP[5] = (byte)(GFM_TMP[5] ^ GFmul(GFM_TMP[37], GFM_TMP[4], IRR_MUL_OFFSET));
                                GFM_TMP[6] = (byte)(GFM_TMP[6] ^ GFmul(GFM_TMP[38], GFM_TMP[4], IRR_MUL_OFFSET));
                                GFM_TMP[7] = (byte)(GFM_TMP[7] ^ GFmul(GFM_TMP[39], GFM_TMP[4], IRR_MUL_OFFSET));

                                GFM_AUG[12] = GFmul(GFM_AUG[36], GFM_TMP[12], IRR_MUL_OFFSET);
                                GFM_AUG[11] = (byte)(GFM_AUG[11] ^ GFmul(GFM_AUG[35], GFM_TMP[12], IRR_MUL_OFFSET));
                                GFM_AUG[10] = (byte)(GFM_AUG[10] ^ GFmul(GFM_AUG[34], GFM_TMP[12], IRR_MUL_OFFSET));
                                GFM_AUG[9] = (byte)(GFM_AUG[9] ^ GFmul(GFM_AUG[33], GFM_TMP[12], IRR_MUL_OFFSET));
                                GFM_AUG[8] = (byte)(GFM_AUG[8] ^ GFmul(GFM_AUG[32], GFM_TMP[12], IRR_MUL_OFFSET));
                                GFM_TMP[13] = (byte)(GFM_TMP[13] ^ GFmul(GFM_TMP[37], GFM_TMP[12], IRR_MUL_OFFSET));
                                GFM_TMP[14] = (byte)(GFM_TMP[14] ^ GFmul(GFM_TMP[38], GFM_TMP[12], IRR_MUL_OFFSET));
                                GFM_TMP[15] = (byte)(GFM_TMP[15] ^ GFmul(GFM_TMP[39], GFM_TMP[12], IRR_MUL_OFFSET));

                                GFM_AUG[20] = GFmul(GFM_AUG[36], GFM_TMP[20], IRR_MUL_OFFSET);
                                GFM_AUG[19] = (byte)(GFM_AUG[19] ^ GFmul(GFM_AUG[35], GFM_TMP[20], IRR_MUL_OFFSET));
                                GFM_AUG[18] = (byte)(GFM_AUG[18] ^ GFmul(GFM_AUG[34], GFM_TMP[20], IRR_MUL_OFFSET));
                                GFM_AUG[17] = (byte)(GFM_AUG[17] ^ GFmul(GFM_AUG[33], GFM_TMP[20], IRR_MUL_OFFSET));
                                GFM_AUG[16] = (byte)(GFM_AUG[16] ^ GFmul(GFM_AUG[32], GFM_TMP[20], IRR_MUL_OFFSET));
                                GFM_TMP[21] = (byte)(GFM_TMP[21] ^ GFmul(GFM_TMP[37], GFM_TMP[20], IRR_MUL_OFFSET));
                                GFM_TMP[22] = (byte)(GFM_TMP[22] ^ GFmul(GFM_TMP[38], GFM_TMP[20], IRR_MUL_OFFSET));
                                GFM_TMP[23] = (byte)(GFM_TMP[23] ^ GFmul(GFM_TMP[39], GFM_TMP[20], IRR_MUL_OFFSET));

                                GFM_AUG[28] = GFmul(GFM_AUG[36], GFM_TMP[28], IRR_MUL_OFFSET);
                                GFM_AUG[27] = (byte)(GFM_AUG[27] ^ GFmul(GFM_AUG[35], GFM_TMP[28], IRR_MUL_OFFSET));
                                GFM_AUG[26] = (byte)(GFM_AUG[26] ^ GFmul(GFM_AUG[34], GFM_TMP[28], IRR_MUL_OFFSET));
                                GFM_AUG[25] = (byte)(GFM_AUG[25] ^ GFmul(GFM_AUG[33], GFM_TMP[28], IRR_MUL_OFFSET));
                                GFM_AUG[24] = (byte)(GFM_AUG[24] ^ GFmul(GFM_AUG[32], GFM_TMP[28], IRR_MUL_OFFSET));
                                GFM_TMP[29] = (byte)(GFM_TMP[29] ^ GFmul(GFM_TMP[37], GFM_TMP[28], IRR_MUL_OFFSET));
                                GFM_TMP[30] = (byte)(GFM_TMP[30] ^ GFmul(GFM_TMP[38], GFM_TMP[28], IRR_MUL_OFFSET));
                                GFM_TMP[31] = (byte)(GFM_TMP[31] ^ GFmul(GFM_TMP[39], GFM_TMP[28], IRR_MUL_OFFSET));

                                GFM_AUG[44] = GFmul(GFM_AUG[36], GFM_TMP[44], IRR_MUL_OFFSET);
                                GFM_AUG[43] = (byte)(GFM_AUG[43] ^ GFmul(GFM_AUG[35], GFM_TMP[44], IRR_MUL_OFFSET));
                                GFM_AUG[42] = (byte)(GFM_AUG[42] ^ GFmul(GFM_AUG[34], GFM_TMP[44], IRR_MUL_OFFSET));
                                GFM_AUG[41] = (byte)(GFM_AUG[41] ^ GFmul(GFM_AUG[33], GFM_TMP[44], IRR_MUL_OFFSET));
                                GFM_AUG[40] = (byte)(GFM_AUG[40] ^ GFmul(GFM_AUG[32], GFM_TMP[44], IRR_MUL_OFFSET));
                                GFM_TMP[45] = (byte)(GFM_TMP[45] ^ GFmul(GFM_TMP[37], GFM_TMP[44], IRR_MUL_OFFSET));
                                GFM_TMP[46] = (byte)(GFM_TMP[46] ^ GFmul(GFM_TMP[38], GFM_TMP[44], IRR_MUL_OFFSET));
                                GFM_TMP[47] = (byte)(GFM_TMP[47] ^ GFmul(GFM_TMP[39], GFM_TMP[44], IRR_MUL_OFFSET));

                                GFM_AUG[52] = GFmul(GFM_AUG[36], GFM_TMP[52], IRR_MUL_OFFSET);
                                GFM_AUG[51] = (byte)(GFM_AUG[51] ^ GFmul(GFM_AUG[35], GFM_TMP[52], IRR_MUL_OFFSET));
                                GFM_AUG[50] = (byte)(GFM_AUG[50] ^ GFmul(GFM_AUG[34], GFM_TMP[52], IRR_MUL_OFFSET));
                                GFM_AUG[49] = (byte)(GFM_AUG[49] ^ GFmul(GFM_AUG[33], GFM_TMP[52], IRR_MUL_OFFSET));
                                GFM_AUG[48] = (byte)(GFM_AUG[48] ^ GFmul(GFM_AUG[32], GFM_TMP[52], IRR_MUL_OFFSET));
                                GFM_TMP[53] = (byte)(GFM_TMP[53] ^ GFmul(GFM_TMP[37], GFM_TMP[52], IRR_MUL_OFFSET));
                                GFM_TMP[54] = (byte)(GFM_TMP[54] ^ GFmul(GFM_TMP[38], GFM_TMP[52], IRR_MUL_OFFSET));
                                GFM_TMP[55] = (byte)(GFM_TMP[55] ^ GFmul(GFM_TMP[39], GFM_TMP[52], IRR_MUL_OFFSET));

                                GFM_AUG[60] = GFmul(GFM_AUG[36], GFM_TMP[60], IRR_MUL_OFFSET);
                                GFM_AUG[59] = (byte)(GFM_AUG[59] ^ GFmul(GFM_AUG[35], GFM_TMP[60], IRR_MUL_OFFSET));
                                GFM_AUG[58] = (byte)(GFM_AUG[58] ^ GFmul(GFM_AUG[34], GFM_TMP[60], IRR_MUL_OFFSET));
                                GFM_AUG[57] = (byte)(GFM_AUG[57] ^ GFmul(GFM_AUG[33], GFM_TMP[60], IRR_MUL_OFFSET));
                                GFM_AUG[56] = (byte)(GFM_AUG[56] ^ GFmul(GFM_AUG[32], GFM_TMP[60], IRR_MUL_OFFSET));
                                GFM_TMP[61] = (byte)(GFM_TMP[61] ^ GFmul(GFM_TMP[37], GFM_TMP[60], IRR_MUL_OFFSET));
                                GFM_TMP[62] = (byte)(GFM_TMP[62] ^ GFmul(GFM_TMP[38], GFM_TMP[60], IRR_MUL_OFFSET));
                                GFM_TMP[63] = (byte)(GFM_TMP[63] ^ GFmul(GFM_TMP[39], GFM_TMP[60], IRR_MUL_OFFSET));

                                if (GFM_TMP[45] != 0)
                                {
                                    GFM_AUG[45] = GFmulInv(GFM_TMP[45], IRR_INVMUL_OFFSET);
                                    GFM_TMP[46] = GFmul(GFM_TMP[46], GFM_AUG[45], IRR_MUL_OFFSET);
                                    GFM_TMP[47] = GFmul(GFM_TMP[47], GFM_AUG[45], IRR_MUL_OFFSET);
                                    GFM_AUG[44] = GFmul(GFM_AUG[44], GFM_AUG[45], IRR_MUL_OFFSET);
                                    GFM_AUG[43] = GFmul(GFM_AUG[43], GFM_AUG[45], IRR_MUL_OFFSET);
                                    GFM_AUG[42] = GFmul(GFM_AUG[42], GFM_AUG[45], IRR_MUL_OFFSET);
                                    GFM_AUG[41] = GFmul(GFM_AUG[41], GFM_AUG[45], IRR_MUL_OFFSET);
                                    GFM_AUG[40] = GFmul(GFM_AUG[40], GFM_AUG[45], IRR_MUL_OFFSET);

                                    GFM_AUG[5] = GFmul(GFM_AUG[45], GFM_TMP[5], IRR_MUL_OFFSET);
                                    GFM_AUG[4] = (byte)(GFM_AUG[4] ^ GFmul(GFM_AUG[44], GFM_TMP[5], IRR_MUL_OFFSET));
                                    GFM_AUG[3] = (byte)(GFM_AUG[3] ^ GFmul(GFM_AUG[43], GFM_TMP[5], IRR_MUL_OFFSET));
                                    GFM_AUG[2] = (byte)(GFM_AUG[2] ^ GFmul(GFM_AUG[42], GFM_TMP[5], IRR_MUL_OFFSET));
                                    GFM_AUG[1] = (byte)(GFM_AUG[1] ^ GFmul(GFM_AUG[41], GFM_TMP[5], IRR_MUL_OFFSET));
                                    GFM_AUG[0] = (byte)(GFM_AUG[0] ^ GFmul(GFM_AUG[40], GFM_TMP[5], IRR_MUL_OFFSET));
                                    GFM_TMP[6] = (byte)(GFM_TMP[6] ^ GFmul(GFM_TMP[46], GFM_TMP[5], IRR_MUL_OFFSET));
                                    GFM_TMP[7] = (byte)(GFM_TMP[7] ^ GFmul(GFM_TMP[47], GFM_TMP[5], IRR_MUL_OFFSET));

                                    GFM_AUG[13] = GFmul(GFM_AUG[45], GFM_TMP[13], IRR_MUL_OFFSET);
                                    GFM_AUG[12] = (byte)(GFM_AUG[12] ^ GFmul(GFM_AUG[44], GFM_TMP[13], IRR_MUL_OFFSET));
                                    GFM_AUG[11] = (byte)(GFM_AUG[11] ^ GFmul(GFM_AUG[43], GFM_TMP[13], IRR_MUL_OFFSET));
                                    GFM_AUG[10] = (byte)(GFM_AUG[10] ^ GFmul(GFM_AUG[42], GFM_TMP[13], IRR_MUL_OFFSET));
                                    GFM_AUG[9] = (byte)(GFM_AUG[9] ^ GFmul(GFM_AUG[41], GFM_TMP[13], IRR_MUL_OFFSET));
                                    GFM_AUG[8] = (byte)(GFM_AUG[8] ^ GFmul(GFM_AUG[40], GFM_TMP[13], IRR_MUL_OFFSET));
                                    GFM_TMP[14] = (byte)(GFM_TMP[14] ^ GFmul(GFM_TMP[46], GFM_TMP[13], IRR_MUL_OFFSET));
                                    GFM_TMP[15] = (byte)(GFM_TMP[15] ^ GFmul(GFM_TMP[47], GFM_TMP[13], IRR_MUL_OFFSET));

                                    GFM_AUG[21] = GFmul(GFM_AUG[45], GFM_TMP[21], IRR_MUL_OFFSET);
                                    GFM_AUG[20] = (byte)(GFM_AUG[20] ^ GFmul(GFM_AUG[44], GFM_TMP[21], IRR_MUL_OFFSET));
                                    GFM_AUG[19] = (byte)(GFM_AUG[19] ^ GFmul(GFM_AUG[43], GFM_TMP[21], IRR_MUL_OFFSET));
                                    GFM_AUG[18] = (byte)(GFM_AUG[18] ^ GFmul(GFM_AUG[42], GFM_TMP[21], IRR_MUL_OFFSET));
                                    GFM_AUG[17] = (byte)(GFM_AUG[17] ^ GFmul(GFM_AUG[41], GFM_TMP[21], IRR_MUL_OFFSET));
                                    GFM_AUG[16] = (byte)(GFM_AUG[16] ^ GFmul(GFM_AUG[40], GFM_TMP[21], IRR_MUL_OFFSET));
                                    GFM_TMP[22] = (byte)(GFM_TMP[22] ^ GFmul(GFM_TMP[46], GFM_TMP[21], IRR_MUL_OFFSET));
                                    GFM_TMP[23] = (byte)(GFM_TMP[23] ^ GFmul(GFM_TMP[47], GFM_TMP[21], IRR_MUL_OFFSET));

                                    GFM_AUG[29] = GFmul(GFM_AUG[45], GFM_TMP[29], IRR_MUL_OFFSET);
                                    GFM_AUG[28] = (byte)(GFM_AUG[28] ^ GFmul(GFM_AUG[44], GFM_TMP[29], IRR_MUL_OFFSET));
                                    GFM_AUG[27] = (byte)(GFM_AUG[27] ^ GFmul(GFM_AUG[43], GFM_TMP[29], IRR_MUL_OFFSET));
                                    GFM_AUG[26] = (byte)(GFM_AUG[26] ^ GFmul(GFM_AUG[42], GFM_TMP[29], IRR_MUL_OFFSET));
                                    GFM_AUG[25] = (byte)(GFM_AUG[25] ^ GFmul(GFM_AUG[41], GFM_TMP[29], IRR_MUL_OFFSET));
                                    GFM_AUG[24] = (byte)(GFM_AUG[24] ^ GFmul(GFM_AUG[40], GFM_TMP[29], IRR_MUL_OFFSET));
                                    GFM_TMP[30] = (byte)(GFM_TMP[30] ^ GFmul(GFM_TMP[46], GFM_TMP[29], IRR_MUL_OFFSET));
                                    GFM_TMP[31] = (byte)(GFM_TMP[31] ^ GFmul(GFM_TMP[47], GFM_TMP[29], IRR_MUL_OFFSET));

                                    GFM_AUG[37] = GFmul(GFM_AUG[45], GFM_TMP[37], IRR_MUL_OFFSET);
                                    GFM_AUG[36] = (byte)(GFM_AUG[36] ^ GFmul(GFM_AUG[44], GFM_TMP[37], IRR_MUL_OFFSET));
                                    GFM_AUG[35] = (byte)(GFM_AUG[35] ^ GFmul(GFM_AUG[43], GFM_TMP[37], IRR_MUL_OFFSET));
                                    GFM_AUG[34] = (byte)(GFM_AUG[34] ^ GFmul(GFM_AUG[42], GFM_TMP[37], IRR_MUL_OFFSET));
                                    GFM_AUG[33] = (byte)(GFM_AUG[33] ^ GFmul(GFM_AUG[41], GFM_TMP[37], IRR_MUL_OFFSET));
                                    GFM_AUG[32] = (byte)(GFM_AUG[32] ^ GFmul(GFM_AUG[40], GFM_TMP[37], IRR_MUL_OFFSET));
                                    GFM_TMP[38] = (byte)(GFM_TMP[38] ^ GFmul(GFM_TMP[46], GFM_TMP[37], IRR_MUL_OFFSET));
                                    GFM_TMP[39] = (byte)(GFM_TMP[39] ^ GFmul(GFM_TMP[47], GFM_TMP[37], IRR_MUL_OFFSET));

                                    GFM_AUG[53] = GFmul(GFM_AUG[45], GFM_TMP[53], IRR_MUL_OFFSET);
                                    GFM_AUG[52] = (byte)(GFM_AUG[52] ^ GFmul(GFM_AUG[44], GFM_TMP[53], IRR_MUL_OFFSET));
                                    GFM_AUG[51] = (byte)(GFM_AUG[51] ^ GFmul(GFM_AUG[43], GFM_TMP[53], IRR_MUL_OFFSET));
                                    GFM_AUG[50] = (byte)(GFM_AUG[50] ^ GFmul(GFM_AUG[42], GFM_TMP[53], IRR_MUL_OFFSET));
                                    GFM_AUG[49] = (byte)(GFM_AUG[49] ^ GFmul(GFM_AUG[41], GFM_TMP[53], IRR_MUL_OFFSET));
                                    GFM_AUG[48] = (byte)(GFM_AUG[48] ^ GFmul(GFM_AUG[40], GFM_TMP[53], IRR_MUL_OFFSET));
                                    GFM_TMP[54] = (byte)(GFM_TMP[54] ^ GFmul(GFM_TMP[46], GFM_TMP[53], IRR_MUL_OFFSET));
                                    GFM_TMP[55] = (byte)(GFM_TMP[55] ^ GFmul(GFM_TMP[47], GFM_TMP[53], IRR_MUL_OFFSET));

                                    GFM_AUG[61] = GFmul(GFM_AUG[45], GFM_TMP[61], IRR_MUL_OFFSET);
                                    GFM_AUG[60] = (byte)(GFM_AUG[60] ^ GFmul(GFM_AUG[44], GFM_TMP[61], IRR_MUL_OFFSET));
                                    GFM_AUG[59] = (byte)(GFM_AUG[59] ^ GFmul(GFM_AUG[43], GFM_TMP[61], IRR_MUL_OFFSET));
                                    GFM_AUG[58] = (byte)(GFM_AUG[58] ^ GFmul(GFM_AUG[42], GFM_TMP[61], IRR_MUL_OFFSET));
                                    GFM_AUG[57] = (byte)(GFM_AUG[57] ^ GFmul(GFM_AUG[41], GFM_TMP[61], IRR_MUL_OFFSET));
                                    GFM_AUG[56] = (byte)(GFM_AUG[56] ^ GFmul(GFM_AUG[40], GFM_TMP[61], IRR_MUL_OFFSET));
                                    GFM_TMP[62] = (byte)(GFM_TMP[62] ^ GFmul(GFM_TMP[46], GFM_TMP[61], IRR_MUL_OFFSET));
                                    GFM_TMP[63] = (byte)(GFM_TMP[63] ^ GFmul(GFM_TMP[47], GFM_TMP[61], IRR_MUL_OFFSET));

                                    if (GFM_TMP[54] != 0)
                                    {
                                        GFM_AUG[54] = GFmulInv(GFM_TMP[54], IRR_INVMUL_OFFSET);
                                        GFM_TMP[55] = GFmul(GFM_TMP[55], GFM_AUG[54], IRR_MUL_OFFSET);
                                        GFM_AUG[53] = GFmul(GFM_AUG[53], GFM_AUG[54], IRR_MUL_OFFSET);
                                        GFM_AUG[52] = GFmul(GFM_AUG[52], GFM_AUG[54], IRR_MUL_OFFSET);
                                        GFM_AUG[51] = GFmul(GFM_AUG[51], GFM_AUG[54], IRR_MUL_OFFSET);
                                        GFM_AUG[50] = GFmul(GFM_AUG[50], GFM_AUG[54], IRR_MUL_OFFSET);
                                        GFM_AUG[49] = GFmul(GFM_AUG[49], GFM_AUG[54], IRR_MUL_OFFSET);
                                        GFM_AUG[48] = GFmul(GFM_AUG[48], GFM_AUG[54], IRR_MUL_OFFSET);

                                        GFM_AUG[6] = GFmul(GFM_AUG[54], GFM_TMP[6], IRR_MUL_OFFSET);
                                        GFM_AUG[5] = (byte)(GFM_AUG[5] ^ GFmul(GFM_AUG[53], GFM_TMP[6], IRR_MUL_OFFSET));
                                        GFM_AUG[4] = (byte)(GFM_AUG[4] ^ GFmul(GFM_AUG[52], GFM_TMP[6], IRR_MUL_OFFSET));
                                        GFM_AUG[3] = (byte)(GFM_AUG[3] ^ GFmul(GFM_AUG[51], GFM_TMP[6], IRR_MUL_OFFSET));
                                        GFM_AUG[2] = (byte)(GFM_AUG[2] ^ GFmul(GFM_AUG[50], GFM_TMP[6], IRR_MUL_OFFSET));
                                        GFM_AUG[1] = (byte)(GFM_AUG[1] ^ GFmul(GFM_AUG[49], GFM_TMP[6], IRR_MUL_OFFSET));
                                        GFM_AUG[0] = (byte)(GFM_AUG[0] ^ GFmul(GFM_AUG[48], GFM_TMP[6], IRR_MUL_OFFSET));
                                        GFM_TMP[7] = (byte)(GFM_TMP[7] ^ GFmul(GFM_TMP[55], GFM_TMP[6], IRR_MUL_OFFSET));

                                        GFM_AUG[14] = GFmul(GFM_AUG[54], GFM_TMP[14], IRR_MUL_OFFSET);
                                        GFM_AUG[13] = (byte)(GFM_AUG[13] ^ GFmul(GFM_AUG[53], GFM_TMP[14], IRR_MUL_OFFSET));
                                        GFM_AUG[12] = (byte)(GFM_AUG[12] ^ GFmul(GFM_AUG[52], GFM_TMP[14], IRR_MUL_OFFSET));
                                        GFM_AUG[11] = (byte)(GFM_AUG[11] ^ GFmul(GFM_AUG[51], GFM_TMP[14], IRR_MUL_OFFSET));
                                        GFM_AUG[10] = (byte)(GFM_AUG[10] ^ GFmul(GFM_AUG[50], GFM_TMP[14], IRR_MUL_OFFSET));
                                        GFM_AUG[9] = (byte)(GFM_AUG[9] ^ GFmul(GFM_AUG[49], GFM_TMP[14], IRR_MUL_OFFSET));
                                        GFM_AUG[8] = (byte)(GFM_AUG[8] ^ GFmul(GFM_AUG[48], GFM_TMP[14], IRR_MUL_OFFSET));
                                        GFM_TMP[15] = (byte)(GFM_TMP[15] ^ GFmul(GFM_TMP[55], GFM_TMP[14], IRR_MUL_OFFSET));

                                        GFM_AUG[22] = GFmul(GFM_AUG[54], GFM_TMP[22], IRR_MUL_OFFSET);
                                        GFM_AUG[21] = (byte)(GFM_AUG[21] ^ GFmul(GFM_AUG[53], GFM_TMP[22], IRR_MUL_OFFSET));
                                        GFM_AUG[20] = (byte)(GFM_AUG[20] ^ GFmul(GFM_AUG[52], GFM_TMP[22], IRR_MUL_OFFSET));
                                        GFM_AUG[19] = (byte)(GFM_AUG[19] ^ GFmul(GFM_AUG[51], GFM_TMP[22], IRR_MUL_OFFSET));
                                        GFM_AUG[18] = (byte)(GFM_AUG[18] ^ GFmul(GFM_AUG[50], GFM_TMP[22], IRR_MUL_OFFSET));
                                        GFM_AUG[17] = (byte)(GFM_AUG[17] ^ GFmul(GFM_AUG[49], GFM_TMP[22], IRR_MUL_OFFSET));
                                        GFM_AUG[16] = (byte)(GFM_AUG[16] ^ GFmul(GFM_AUG[48], GFM_TMP[22], IRR_MUL_OFFSET));
                                        GFM_TMP[23] = (byte)(GFM_TMP[23] ^ GFmul(GFM_TMP[55], GFM_TMP[22], IRR_MUL_OFFSET));

                                        GFM_AUG[30] = GFmul(GFM_AUG[54], GFM_TMP[30], IRR_MUL_OFFSET);
                                        GFM_AUG[29] = (byte)(GFM_AUG[29] ^ GFmul(GFM_AUG[53], GFM_TMP[30], IRR_MUL_OFFSET));
                                        GFM_AUG[28] = (byte)(GFM_AUG[28] ^ GFmul(GFM_AUG[52], GFM_TMP[30], IRR_MUL_OFFSET));
                                        GFM_AUG[27] = (byte)(GFM_AUG[27] ^ GFmul(GFM_AUG[51], GFM_TMP[30], IRR_MUL_OFFSET));
                                        GFM_AUG[26] = (byte)(GFM_AUG[26] ^ GFmul(GFM_AUG[50], GFM_TMP[30], IRR_MUL_OFFSET));
                                        GFM_AUG[25] = (byte)(GFM_AUG[25] ^ GFmul(GFM_AUG[49], GFM_TMP[30], IRR_MUL_OFFSET));
                                        GFM_AUG[24] = (byte)(GFM_AUG[24] ^ GFmul(GFM_AUG[48], GFM_TMP[30], IRR_MUL_OFFSET));
                                        GFM_TMP[31] = (byte)(GFM_TMP[31] ^ GFmul(GFM_TMP[55], GFM_TMP[30], IRR_MUL_OFFSET));

                                        GFM_AUG[38] = GFmul(GFM_AUG[54], GFM_TMP[38], IRR_MUL_OFFSET);
                                        GFM_AUG[37] = (byte)(GFM_AUG[37] ^ GFmul(GFM_AUG[53], GFM_TMP[38], IRR_MUL_OFFSET));
                                        GFM_AUG[36] = (byte)(GFM_AUG[36] ^ GFmul(GFM_AUG[52], GFM_TMP[38], IRR_MUL_OFFSET));
                                        GFM_AUG[35] = (byte)(GFM_AUG[35] ^ GFmul(GFM_AUG[51], GFM_TMP[38], IRR_MUL_OFFSET));
                                        GFM_AUG[34] = (byte)(GFM_AUG[34] ^ GFmul(GFM_AUG[50], GFM_TMP[38], IRR_MUL_OFFSET));
                                        GFM_AUG[33] = (byte)(GFM_AUG[33] ^ GFmul(GFM_AUG[49], GFM_TMP[38], IRR_MUL_OFFSET));
                                        GFM_AUG[32] = (byte)(GFM_AUG[32] ^ GFmul(GFM_AUG[48], GFM_TMP[38], IRR_MUL_OFFSET));
                                        GFM_TMP[39] = (byte)(GFM_TMP[39] ^ GFmul(GFM_TMP[55], GFM_TMP[38], IRR_MUL_OFFSET));

                                        GFM_AUG[46] = GFmul(GFM_AUG[54], GFM_TMP[46], IRR_MUL_OFFSET);
                                        GFM_AUG[45] = (byte)(GFM_AUG[45] ^ GFmul(GFM_AUG[53], GFM_TMP[46], IRR_MUL_OFFSET));
                                        GFM_AUG[44] = (byte)(GFM_AUG[44] ^ GFmul(GFM_AUG[52], GFM_TMP[46], IRR_MUL_OFFSET));
                                        GFM_AUG[43] = (byte)(GFM_AUG[43] ^ GFmul(GFM_AUG[51], GFM_TMP[46], IRR_MUL_OFFSET));
                                        GFM_AUG[42] = (byte)(GFM_AUG[42] ^ GFmul(GFM_AUG[50], GFM_TMP[46], IRR_MUL_OFFSET));
                                        GFM_AUG[41] = (byte)(GFM_AUG[41] ^ GFmul(GFM_AUG[49], GFM_TMP[46], IRR_MUL_OFFSET));
                                        GFM_AUG[40] = (byte)(GFM_AUG[40] ^ GFmul(GFM_AUG[48], GFM_TMP[46], IRR_MUL_OFFSET));
                                        GFM_TMP[47] = (byte)(GFM_TMP[47] ^ GFmul(GFM_TMP[55], GFM_TMP[46], IRR_MUL_OFFSET));

                                        GFM_AUG[62] = GFmul(GFM_AUG[54], GFM_TMP[62], IRR_MUL_OFFSET);
                                        GFM_AUG[61] = (byte)(GFM_AUG[61] ^ GFmul(GFM_AUG[53], GFM_TMP[62], IRR_MUL_OFFSET));
                                        GFM_AUG[60] = (byte)(GFM_AUG[60] ^ GFmul(GFM_AUG[52], GFM_TMP[62], IRR_MUL_OFFSET));
                                        GFM_AUG[59] = (byte)(GFM_AUG[59] ^ GFmul(GFM_AUG[51], GFM_TMP[62], IRR_MUL_OFFSET));
                                        GFM_AUG[58] = (byte)(GFM_AUG[58] ^ GFmul(GFM_AUG[50], GFM_TMP[62], IRR_MUL_OFFSET));
                                        GFM_AUG[57] = (byte)(GFM_AUG[57] ^ GFmul(GFM_AUG[49], GFM_TMP[62], IRR_MUL_OFFSET));
                                        GFM_AUG[56] = (byte)(GFM_AUG[56] ^ GFmul(GFM_AUG[48], GFM_TMP[62], IRR_MUL_OFFSET));
                                        GFM_TMP[63] = (byte)(GFM_TMP[63] ^ GFmul(GFM_TMP[55], GFM_TMP[62], IRR_MUL_OFFSET));

                                        if (GFM_TMP[63] != 0)
                                        {
                                            GFM_AUG[63] = GFmulInv(GFM_TMP[63], IRR_INVMUL_OFFSET);
                                            GFM_AUG[62] = GFmul(GFM_AUG[62], GFM_AUG[63], IRR_MUL_OFFSET);
                                            GFM_AUG[61] = GFmul(GFM_AUG[61], GFM_AUG[63], IRR_MUL_OFFSET);
                                            GFM_AUG[60] = GFmul(GFM_AUG[60], GFM_AUG[63], IRR_MUL_OFFSET);
                                            GFM_AUG[59] = GFmul(GFM_AUG[59], GFM_AUG[63], IRR_MUL_OFFSET);
                                            GFM_AUG[58] = GFmul(GFM_AUG[58], GFM_AUG[63], IRR_MUL_OFFSET);
                                            GFM_AUG[57] = GFmul(GFM_AUG[57], GFM_AUG[63], IRR_MUL_OFFSET);
                                            GFM_AUG[56] = GFmul(GFM_AUG[56], GFM_AUG[63], IRR_MUL_OFFSET);

                                            GFM_AUG[7] = GFmul(GFM_AUG[63], GFM_TMP[7], IRR_MUL_OFFSET);
                                            GFM_AUG[6] = (byte)(GFM_AUG[6] ^ GFmul(GFM_AUG[62], GFM_TMP[7], IRR_MUL_OFFSET));
                                            GFM_AUG[5] = (byte)(GFM_AUG[5] ^ GFmul(GFM_AUG[61], GFM_TMP[7], IRR_MUL_OFFSET));
                                            GFM_AUG[4] = (byte)(GFM_AUG[4] ^ GFmul(GFM_AUG[60], GFM_TMP[7], IRR_MUL_OFFSET));
                                            GFM_AUG[3] = (byte)(GFM_AUG[3] ^ GFmul(GFM_AUG[59], GFM_TMP[7], IRR_MUL_OFFSET));
                                            GFM_AUG[2] = (byte)(GFM_AUG[2] ^ GFmul(GFM_AUG[58], GFM_TMP[7], IRR_MUL_OFFSET));
                                            GFM_AUG[1] = (byte)(GFM_AUG[1] ^ GFmul(GFM_AUG[57], GFM_TMP[7], IRR_MUL_OFFSET));
                                            GFM_AUG[0] = (byte)(GFM_AUG[0] ^ GFmul(GFM_AUG[56], GFM_TMP[7], IRR_MUL_OFFSET));

                                            GFM_AUG[15] = GFmul(GFM_AUG[63], GFM_TMP[15], IRR_MUL_OFFSET);
                                            GFM_AUG[14] = (byte)(GFM_AUG[14] ^ GFmul(GFM_AUG[62], GFM_TMP[15], IRR_MUL_OFFSET));
                                            GFM_AUG[13] = (byte)(GFM_AUG[13] ^ GFmul(GFM_AUG[61], GFM_TMP[15], IRR_MUL_OFFSET));
                                            GFM_AUG[12] = (byte)(GFM_AUG[12] ^ GFmul(GFM_AUG[60], GFM_TMP[15], IRR_MUL_OFFSET));
                                            GFM_AUG[11] = (byte)(GFM_AUG[11] ^ GFmul(GFM_AUG[59], GFM_TMP[15], IRR_MUL_OFFSET));
                                            GFM_AUG[10] = (byte)(GFM_AUG[10] ^ GFmul(GFM_AUG[58], GFM_TMP[15], IRR_MUL_OFFSET));
                                            GFM_AUG[9] = (byte)(GFM_AUG[9] ^ GFmul(GFM_AUG[57], GFM_TMP[15], IRR_MUL_OFFSET));
                                            GFM_AUG[8] = (byte)(GFM_AUG[8] ^ GFmul(GFM_AUG[56], GFM_TMP[15], IRR_MUL_OFFSET));

                                            GFM_AUG[23] = GFmul(GFM_AUG[63], GFM_TMP[23], IRR_MUL_OFFSET);
                                            GFM_AUG[22] = (byte)(GFM_AUG[22] ^ GFmul(GFM_AUG[62], GFM_TMP[23], IRR_MUL_OFFSET));
                                            GFM_AUG[21] = (byte)(GFM_AUG[21] ^ GFmul(GFM_AUG[61], GFM_TMP[23], IRR_MUL_OFFSET));
                                            GFM_AUG[20] = (byte)(GFM_AUG[20] ^ GFmul(GFM_AUG[60], GFM_TMP[23], IRR_MUL_OFFSET));
                                            GFM_AUG[19] = (byte)(GFM_AUG[19] ^ GFmul(GFM_AUG[59], GFM_TMP[23], IRR_MUL_OFFSET));
                                            GFM_AUG[18] = (byte)(GFM_AUG[18] ^ GFmul(GFM_AUG[58], GFM_TMP[23], IRR_MUL_OFFSET));
                                            GFM_AUG[17] = (byte)(GFM_AUG[17] ^ GFmul(GFM_AUG[57], GFM_TMP[23], IRR_MUL_OFFSET));
                                            GFM_AUG[16] = (byte)(GFM_AUG[16] ^ GFmul(GFM_AUG[56], GFM_TMP[23], IRR_MUL_OFFSET));

                                            GFM_AUG[31] = GFmul(GFM_AUG[63], GFM_TMP[31], IRR_MUL_OFFSET);
                                            GFM_AUG[30] = (byte)(GFM_AUG[30] ^ GFmul(GFM_AUG[62], GFM_TMP[31], IRR_MUL_OFFSET));
                                            GFM_AUG[29] = (byte)(GFM_AUG[29] ^ GFmul(GFM_AUG[61], GFM_TMP[31], IRR_MUL_OFFSET));
                                            GFM_AUG[28] = (byte)(GFM_AUG[28] ^ GFmul(GFM_AUG[60], GFM_TMP[31], IRR_MUL_OFFSET));
                                            GFM_AUG[27] = (byte)(GFM_AUG[27] ^ GFmul(GFM_AUG[59], GFM_TMP[31], IRR_MUL_OFFSET));
                                            GFM_AUG[26] = (byte)(GFM_AUG[26] ^ GFmul(GFM_AUG[58], GFM_TMP[31], IRR_MUL_OFFSET));
                                            GFM_AUG[25] = (byte)(GFM_AUG[25] ^ GFmul(GFM_AUG[57], GFM_TMP[31], IRR_MUL_OFFSET));
                                            GFM_AUG[24] = (byte)(GFM_AUG[24] ^ GFmul(GFM_AUG[56], GFM_TMP[31], IRR_MUL_OFFSET));

                                            GFM_AUG[39] = GFmul(GFM_AUG[63], GFM_TMP[39], IRR_MUL_OFFSET);
                                            GFM_AUG[38] = (byte)(GFM_AUG[38] ^ GFmul(GFM_AUG[62], GFM_TMP[39], IRR_MUL_OFFSET));
                                            GFM_AUG[37] = (byte)(GFM_AUG[37] ^ GFmul(GFM_AUG[61], GFM_TMP[39], IRR_MUL_OFFSET));
                                            GFM_AUG[36] = (byte)(GFM_AUG[36] ^ GFmul(GFM_AUG[60], GFM_TMP[39], IRR_MUL_OFFSET));
                                            GFM_AUG[35] = (byte)(GFM_AUG[35] ^ GFmul(GFM_AUG[59], GFM_TMP[39], IRR_MUL_OFFSET));
                                            GFM_AUG[34] = (byte)(GFM_AUG[34] ^ GFmul(GFM_AUG[58], GFM_TMP[39], IRR_MUL_OFFSET));
                                            GFM_AUG[33] = (byte)(GFM_AUG[33] ^ GFmul(GFM_AUG[57], GFM_TMP[39], IRR_MUL_OFFSET));
                                            GFM_AUG[32] = (byte)(GFM_AUG[32] ^ GFmul(GFM_AUG[56], GFM_TMP[39], IRR_MUL_OFFSET));

                                            GFM_AUG[47] = GFmul(GFM_AUG[63], GFM_TMP[47], IRR_MUL_OFFSET);
                                            GFM_AUG[46] = (byte)(GFM_AUG[46] ^ GFmul(GFM_AUG[62], GFM_TMP[47], IRR_MUL_OFFSET));
                                            GFM_AUG[45] = (byte)(GFM_AUG[45] ^ GFmul(GFM_AUG[61], GFM_TMP[47], IRR_MUL_OFFSET));
                                            GFM_AUG[44] = (byte)(GFM_AUG[44] ^ GFmul(GFM_AUG[60], GFM_TMP[47], IRR_MUL_OFFSET));
                                            GFM_AUG[43] = (byte)(GFM_AUG[43] ^ GFmul(GFM_AUG[59], GFM_TMP[47], IRR_MUL_OFFSET));
                                            GFM_AUG[42] = (byte)(GFM_AUG[42] ^ GFmul(GFM_AUG[58], GFM_TMP[47], IRR_MUL_OFFSET));
                                            GFM_AUG[41] = (byte)(GFM_AUG[41] ^ GFmul(GFM_AUG[57], GFM_TMP[47], IRR_MUL_OFFSET));
                                            GFM_AUG[40] = (byte)(GFM_AUG[40] ^ GFmul(GFM_AUG[56], GFM_TMP[47], IRR_MUL_OFFSET));

                                            GFM_AUG[55] = GFmul(GFM_AUG[63], GFM_TMP[55], IRR_MUL_OFFSET);
                                            GFM_AUG[54] = (byte)(GFM_AUG[54] ^ GFmul(GFM_AUG[62], GFM_TMP[55], IRR_MUL_OFFSET));
                                            GFM_AUG[53] = (byte)(GFM_AUG[53] ^ GFmul(GFM_AUG[61], GFM_TMP[55], IRR_MUL_OFFSET));
                                            GFM_AUG[52] = (byte)(GFM_AUG[52] ^ GFmul(GFM_AUG[60], GFM_TMP[55], IRR_MUL_OFFSET));
                                            GFM_AUG[51] = (byte)(GFM_AUG[51] ^ GFmul(GFM_AUG[59], GFM_TMP[55], IRR_MUL_OFFSET));
                                            GFM_AUG[50] = (byte)(GFM_AUG[50] ^ GFmul(GFM_AUG[58], GFM_TMP[55], IRR_MUL_OFFSET));
                                            GFM_AUG[49] = (byte)(GFM_AUG[49] ^ GFmul(GFM_AUG[57], GFM_TMP[55], IRR_MUL_OFFSET));
                                            GFM_AUG[48] = (byte)(GFM_AUG[48] ^ GFmul(GFM_AUG[56], GFM_TMP[55], IRR_MUL_OFFSET));

                                            GFM_AUG.CopyTo(MAT8X8, 0); //if completes all ifs, write this
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            } //Calculate inverse matrix over GF(2^8) with p(x)

        }

        public void JSHIFT(ref byte[] MAT8X8, int ROUND)
        {
            int shift = ROUND << 6;
            for (int i = 0; i < MAT8X8.Length; i++)
            {
                J_IMAGE[i] = MAT8X8[JMAP[i + shift]];
            }
            J_IMAGE.CopyTo(MAT8X8, 0);
        }

        public void INV_JSHIFT(ref byte[] MAT8X8, int ROUND)
        {
            int shift = ROUND << 6;
            for (int i = 0; i < MAT8X8.Length; i++)
            {
                J_IMAGE[i] = MAT8X8[INV_JMAP[i + shift]];
            }
            J_IMAGE.CopyTo(MAT8X8, 0);
        }

        public void SWAP(ref int[] MAT8X8, int a, int b)
        {
            int tmp = MAT8X8[b];
            MAT8X8[b] = MAT8X8[a];
            MAT8X8[a] = tmp;
        }

        public void Encrypt(ref byte[] buffer, int rounds, string key)
        {
            PWD = key;

            KHASH = rda.kHashingAlgorithm(PWD);
            RHASH = rda.rHashingAlgorithm(KHASH, PWD);
            rda.keyExpansion(KHASH, 30).CopyTo(EXP, 0);

            BuildJShiftMaps();
            BuildGFMIrrPolys();
        }



        public string GFM_ToString()
        {
            string FULL_AUG = "";
            for (int i = 0; i < 8; i++)
            {
                if (i != 0) FULL_AUG += '\n';
                for (int j = 0; j < 16; j++)
                {
                    if (j != 0) FULL_AUG += ' ';
                    if (j >= 8)
                    {
                        FULL_AUG += GFM_AUG[(i << 3) + (j - 8)].ToString("X2");
                    }
                    else
                    {
                        FULL_AUG += GFM_TMP[(i << 3) + j].ToString("X2");
                    }
                }
            }
            return FULL_AUG;
        }
    }
}
