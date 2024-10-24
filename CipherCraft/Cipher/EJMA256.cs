using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Windows.Forms;

namespace CipherCraft
{
    public class EJMA256
    {
        public RDA_cipher rda;

        private const int NUM_IRREDUCIBLEPOLY = 30;
        private int[] IRREDUCIBLEPOLY = new int[30] { 0x11B, 0x11D, 0x12B, 0x12D, 0x139, 0x13F, 0x14D, 0x15F, 0x163, 0x165, 0x169, 0x171, 0x177, 0x17B, 0x187, 0x18B, 0x18D, 0x19F, 0x1A3, 0x1A9, 0x1B1, 0x1BD, 0x1C3, 0x1CF, 0x1D7, 0x1DD, 0x1E7, 0x1F3, 0x1F5, 0x1F9 };
        private int[] PRIMES = new int[256];

        private byte[] ENIGMA_MACHINE = new byte[0x10000000]; //1MB * 256
        private byte[] INV_ENIGMA_MACHINE = new byte[0x10000000];
        private byte[] ROTORS = new byte[0x300];

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
        private byte[] KHASH = new byte[64]; 
        private byte[] EXP; //64 bytes * Number of irreducible polynomials
        private string PWD;
        private int ROUNDS;

        public EJMA256()
        { 
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

        private void BuildEnigma()
        {
            BuildEnigmaRotors();
            BuildEnigmaMachine();
        }
        private void BuildEnigmaRotors()
        {
            int a = NUM_IRREDUCIBLEPOLY; // 30
            int ii = 0;
            for (int i = 0; i < 3; i++) //EJMA has 3 rotors totalling (256)^3 = 16,777,216 clocks, only 1,048,576 are needed for 1MB
            {
                int f = 0;
                while (f < 256)
                {
                    bool notContains = true;
                    for (int k = i << 8; k < (i << 8) + f; k++)
                    {
                        if (ROTORS[k] == EXP[ii]) notContains = false;
                    }
                    if (notContains)
                    {
                        byte value = EXP[ii];

                        ROTORS[(i << 8) + f++] = value;
                        if ((f & 255) == 0)
                        {
                            value = (byte)0x0;
                        }
                    }
                    ii++;
                    if (ii >= EXP.Length)
                    {
                        a <<= 1; // 30 * 2
                        EXP = rda.keyExpansion(KHASH, a);
                    }
                }
            }
        }
        private void BuildEnigmaMachine()
        {
            for (int i = 0; i < 0x100000; i++) //0x100000 = 1MB
            {
                int shift = i << 8;
                int onesPlace = i & 255;
                int twosPlace = (i >> 8) & 255;
                int threesPlace = (i >> 16) & 255;

                for (int j = 0; j < 256; j++) //256 see where every character maps to
                {
                    int value = j;
                    value = ROTORS[(value + onesPlace) & 255];
                    value = ROTORS[256 + ((value + twosPlace) & 255)];
                    ENIGMA_MACHINE[shift + j] = ROTORS[512 + ((value + threesPlace) & 255)];
                }
            }
        }
        private void BuildEnigmaMachineInverse()
        {
            for (int i = 0; i < 1048576; i++)
            {
                int shift = i << 8; // * 256
                for (int j = 0; j < 256; j++)
                {
                    INV_ENIGMA_MACHINE[shift + ENIGMA_MACHINE[shift + j]] = (byte)j;
                }
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
        /// Polyalphabetic substitution cipher from WWII wih security fixes.
        /// </summary>
        /// <param name="blockOffs"></param>
        public void ENIGMA(ref byte[] MAT8X8, int blockOffs)
        {
            int enigmaIndex = (blockOffs << 8) & 0xFFFFFFF; //64 * 256 mod enigma size
            for (int i = 0; i < 64; i++)
            {
                MAT8X8[i] = ENIGMA_MACHINE[MAT8X8[i] + (i << 8) + enigmaIndex];
            }
        }
        public void INV_ENIGMA(ref byte[] MAT8X8, int blockOffs)
        {
            int enigmaIndex = (blockOffs << 8) & 0xFFFFFFF; //64 * 256 mod enigma size
            for (int i = 0; i < 64; i++)
            {
                MAT8X8[i] = INV_ENIGMA_MACHINE[MAT8X8[i] + (i << 8) + enigmaIndex];
            }
        }

        /// <summary>
        /// Remaps byte positions over entire 8x8 matrix
        /// </summary>
        /// <param name="MAT8X8"></param>
        /// <param name="ROUND"></param>
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

        /// <summary>
        /// Calculates the inverse matrix 8x8 over GF(2^8) with p(x)
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

        /// <summary>
        /// Add the round key
        /// </summary>
        /// <param name="MAT8X8"></param>
        /// <param name="ROUND"></param>
        public void ADD_ROUND_KEY(ref byte[] MAT8X8, int ROUND)
        {
            int shift = ROUND << 6;
            MAT8X8[0] ^= EXP[shift];
            MAT8X8[1] ^= EXP[shift + 1];
            MAT8X8[2] ^= EXP[shift + 2];
            MAT8X8[3] ^= EXP[shift + 3];
            MAT8X8[4] ^= EXP[shift + 4];
            MAT8X8[5] ^= EXP[shift + 5];
            MAT8X8[6] ^= EXP[shift + 6];
            MAT8X8[7] ^= EXP[shift + 7];
            MAT8X8[8] ^= EXP[shift + 8];
            MAT8X8[9] ^= EXP[shift + 9];
            MAT8X8[10] ^= EXP[shift + 10];
            MAT8X8[11] ^= EXP[shift + 11];
            MAT8X8[12] ^= EXP[shift + 12];
            MAT8X8[13] ^= EXP[shift + 13];
            MAT8X8[14] ^= EXP[shift + 14];
            MAT8X8[15] ^= EXP[shift + 15];
            MAT8X8[16] ^= EXP[shift + 16];
            MAT8X8[17] ^= EXP[shift + 17];
            MAT8X8[18] ^= EXP[shift + 18];
            MAT8X8[19] ^= EXP[shift + 19];
            MAT8X8[20] ^= EXP[shift + 20];
            MAT8X8[21] ^= EXP[shift + 21];
            MAT8X8[22] ^= EXP[shift + 22];
            MAT8X8[23] ^= EXP[shift + 23];
            MAT8X8[24] ^= EXP[shift + 24];
            MAT8X8[25] ^= EXP[shift + 25];
            MAT8X8[26] ^= EXP[shift + 26];
            MAT8X8[27] ^= EXP[shift + 27];
            MAT8X8[28] ^= EXP[shift + 28];
            MAT8X8[29] ^= EXP[shift + 29];
            MAT8X8[30] ^= EXP[shift + 30];
            MAT8X8[31] ^= EXP[shift + 31];
            MAT8X8[32] ^= EXP[shift + 32];
            MAT8X8[33] ^= EXP[shift + 33];
            MAT8X8[34] ^= EXP[shift + 34];
            MAT8X8[35] ^= EXP[shift + 35];
            MAT8X8[36] ^= EXP[shift + 36];
            MAT8X8[37] ^= EXP[shift + 37];
            MAT8X8[38] ^= EXP[shift + 38];
            MAT8X8[39] ^= EXP[shift + 39];
            MAT8X8[40] ^= EXP[shift + 40];
            MAT8X8[41] ^= EXP[shift + 41];
            MAT8X8[42] ^= EXP[shift + 42];
            MAT8X8[43] ^= EXP[shift + 43];
            MAT8X8[44] ^= EXP[shift + 44];
            MAT8X8[45] ^= EXP[shift + 45];
            MAT8X8[46] ^= EXP[shift + 46];
            MAT8X8[47] ^= EXP[shift + 47];
            MAT8X8[48] ^= EXP[shift + 48];
            MAT8X8[49] ^= EXP[shift + 49];
            MAT8X8[50] ^= EXP[shift + 50];
            MAT8X8[51] ^= EXP[shift + 51];
            MAT8X8[52] ^= EXP[shift + 52];
            MAT8X8[53] ^= EXP[shift + 53];
            MAT8X8[54] ^= EXP[shift + 54];
            MAT8X8[55] ^= EXP[shift + 55];
            MAT8X8[56] ^= EXP[shift + 56];
            MAT8X8[57] ^= EXP[shift + 57];
            MAT8X8[58] ^= EXP[shift + 58];
            MAT8X8[59] ^= EXP[shift + 59];
            MAT8X8[60] ^= EXP[shift + 60];
            MAT8X8[61] ^= EXP[shift + 61];
            MAT8X8[62] ^= EXP[shift + 62];
            MAT8X8[63] ^= EXP[shift + 63];
        }

        /// <summary>
        /// Add RHASH key at beginning of encryption
        /// </summary>
        /// <param name="MAT8X8"></param>
        public void ADD_RHASH_KEY(ref byte[] MAT8X8)
        {
            MAT8X8[0] ^= RHASH[0];
            MAT8X8[1] ^= RHASH[1];
            MAT8X8[2] ^= RHASH[2];
            MAT8X8[3] ^= RHASH[3];
            MAT8X8[4] ^= RHASH[4];
            MAT8X8[5] ^= RHASH[5];
            MAT8X8[6] ^= RHASH[6];
            MAT8X8[7] ^= RHASH[7];
            MAT8X8[8] ^= RHASH[8];
            MAT8X8[9] ^= RHASH[9];
            MAT8X8[10] ^= RHASH[10];
            MAT8X8[11] ^= RHASH[11];
            MAT8X8[12] ^= RHASH[12];
            MAT8X8[13] ^= RHASH[13];
            MAT8X8[14] ^= RHASH[14];
            MAT8X8[15] ^= RHASH[15];
            MAT8X8[16] ^= RHASH[16];
            MAT8X8[17] ^= RHASH[17];
            MAT8X8[18] ^= RHASH[18];
            MAT8X8[19] ^= RHASH[19];
            MAT8X8[20] ^= RHASH[20];
            MAT8X8[21] ^= RHASH[21];
            MAT8X8[22] ^= RHASH[22];
            MAT8X8[23] ^= RHASH[23];
            MAT8X8[24] ^= RHASH[24];
            MAT8X8[25] ^= RHASH[25];
            MAT8X8[26] ^= RHASH[26];
            MAT8X8[27] ^= RHASH[27];
            MAT8X8[28] ^= RHASH[28];
            MAT8X8[29] ^= RHASH[29];
            MAT8X8[30] ^= RHASH[30];
            MAT8X8[31] ^= RHASH[31];
            MAT8X8[32] ^= RHASH[32];
            MAT8X8[33] ^= RHASH[33];
            MAT8X8[34] ^= RHASH[34];
            MAT8X8[35] ^= RHASH[35];
            MAT8X8[36] ^= RHASH[36];
            MAT8X8[37] ^= RHASH[37];
            MAT8X8[38] ^= RHASH[38];
            MAT8X8[39] ^= RHASH[39];
            MAT8X8[40] ^= RHASH[40];
            MAT8X8[41] ^= RHASH[41];
            MAT8X8[42] ^= RHASH[42];
            MAT8X8[43] ^= RHASH[43];
            MAT8X8[44] ^= RHASH[44];
            MAT8X8[45] ^= RHASH[45];
            MAT8X8[46] ^= RHASH[46];
            MAT8X8[47] ^= RHASH[47];
            MAT8X8[48] ^= RHASH[48];
            MAT8X8[49] ^= RHASH[49];
            MAT8X8[50] ^= RHASH[50];
            MAT8X8[51] ^= RHASH[51];
            MAT8X8[52] ^= RHASH[52];
            MAT8X8[53] ^= RHASH[53];
            MAT8X8[54] ^= RHASH[54];
            MAT8X8[55] ^= RHASH[55];
            MAT8X8[56] ^= RHASH[56];
            MAT8X8[57] ^= RHASH[57];
            MAT8X8[58] ^= RHASH[58];
            MAT8X8[59] ^= RHASH[59];
            MAT8X8[60] ^= RHASH[60];
            MAT8X8[61] ^= RHASH[61];
            MAT8X8[62] ^= RHASH[62];
            MAT8X8[63] ^= RHASH[63];
        }

        public byte[] Encrypt(byte[] buffer, int rounds, string key)
        {
            ROUNDS = rounds;
            PWD = key;
            KHASH = rda.kHashingAlgorithm(PWD);
            RHASH = rda.rHashingAlgorithm(KHASH, PWD);
            EXP = rda.keyExpansion(KHASH, 30);

            BuildEnigma();
            BuildJShiftMaps();
            BuildGFMIrrPolys();

            return Encrypt(buffer);
        }

        public byte[] Encrypt(byte[] buffer)
        {
            string stages = "";
            buffer = Symmetricate(buffer);
            byte[] BLOCK = new byte[64];
            for (int i = 0; i < buffer.Length; i += 64)
            {
                getBlock(ref buffer, ref BLOCK, i);
                for (int j = 0; j < ROUNDS; j++)
                {
                    if (j == 0)
                    {
                        ADD_RHASH_KEY(ref BLOCK); //salt hash
                    }
                    ENIGMA(ref BLOCK, i); //poly sub
                    JSHIFT(ref BLOCK, j); //byte remapping
                    stages += "stage " + j + ":\nBEFORE\n" + Print.byteArrayToStringMAT(BLOCK, 8) + "\n";
                    R_GFM(ref BLOCK, j);
                    stages += "AFTER\n" + Print.byteArrayToStringMAT(BLOCK, 8) + "\n";
                    if (j < ROUNDS - 1)
                    {
                         //8x8 linear diffusion
                    }//No diffusion on last round
                    ADD_ROUND_KEY(ref BLOCK, j); //round hash
                }
                setBlock(ref buffer, ref BLOCK, i);
            }
            System.Windows.Forms.Clipboard.SetText(stages);
            return buffer;
        }

        public byte[] Decrypt(byte[] buffer, int rounds, string key)
        {
            if ((buffer.Length & 63) == 0)
            {
                ROUNDS = rounds;
                PWD = key;
                KHASH = rda.kHashingAlgorithm(PWD);
                RHASH = rda.rHashingAlgorithm(KHASH, PWD);
                EXP = rda.keyExpansion(KHASH, 30);

                BuildEnigma();
                BuildEnigmaMachineInverse();
                BuildJShiftMaps();
                BuildGFMIrrPolys();
                return Decrypt(buffer);
            }
            else throw new Exception("Buffer is not 256 symmetric");
        }
        public byte[] Decrypt(byte[] buffer)
        {
            string stages = "";
            byte[] BLOCK = new byte[64];
            for (int i = 0; i < buffer.Length; i += 64)
            {
                getBlock(ref buffer, ref BLOCK, i);
                for (int j = ROUNDS - 1; j >= 0; j--)
                {
                    ADD_ROUND_KEY(ref BLOCK, j);
                    if (j < ROUNDS - 1)
                    {
                        
                    }
                    stages += "stage " + (j + 1) + ":\nBEFORE\n" + Print.byteArrayToStringMAT(BLOCK, 8) + "\n";
                    R_GFM(ref BLOCK, j);
                    stages += "AFTER\n" + Print.byteArrayToStringMAT(BLOCK, 8) + "\n";
                    INV_JSHIFT(ref BLOCK, j);
                    INV_ENIGMA(ref BLOCK, i);
                    if (j == 0)
                    {
                        ADD_RHASH_KEY(ref BLOCK);
                    }
                }
                setBlock(ref buffer, ref BLOCK, i);
            }
            System.Windows.Forms.Clipboard.SetText(stages);
            return Desymmetricate(buffer);
        }

        public void getBlock(ref byte[] buffer, ref byte[] BLOCK, int i)
        {
            BLOCK[0] = buffer[i];
            BLOCK[1] = buffer[i + 1];
            BLOCK[2] = buffer[i + 2];
            BLOCK[3] = buffer[i + 3];
            BLOCK[4] = buffer[i + 4];
            BLOCK[5] = buffer[i + 5];
            BLOCK[6] = buffer[i + 6];
            BLOCK[7] = buffer[i + 7];
            BLOCK[8] = buffer[i + 8];
            BLOCK[9] = buffer[i + 9];
            BLOCK[10] = buffer[i + 10];
            BLOCK[11] = buffer[i + 11];
            BLOCK[12] = buffer[i + 12];
            BLOCK[13] = buffer[i + 13];
            BLOCK[14] = buffer[i + 14];
            BLOCK[15] = buffer[i + 15];
            BLOCK[16] = buffer[i + 16];
            BLOCK[17] = buffer[i + 17];
            BLOCK[18] = buffer[i + 18];
            BLOCK[19] = buffer[i + 19];
            BLOCK[20] = buffer[i + 20];
            BLOCK[21] = buffer[i + 21];
            BLOCK[22] = buffer[i + 22];
            BLOCK[23] = buffer[i + 23];
            BLOCK[24] = buffer[i + 24];
            BLOCK[25] = buffer[i + 25];
            BLOCK[26] = buffer[i + 26];
            BLOCK[27] = buffer[i + 27];
            BLOCK[28] = buffer[i + 28];
            BLOCK[29] = buffer[i + 29];
            BLOCK[30] = buffer[i + 30];
            BLOCK[31] = buffer[i + 31];
            BLOCK[32] = buffer[i + 32];
            BLOCK[33] = buffer[i + 33];
            BLOCK[34] = buffer[i + 34];
            BLOCK[35] = buffer[i + 35];
            BLOCK[36] = buffer[i + 36];
            BLOCK[37] = buffer[i + 37];
            BLOCK[38] = buffer[i + 38];
            BLOCK[39] = buffer[i + 39];
            BLOCK[40] = buffer[i + 40];
            BLOCK[41] = buffer[i + 41];
            BLOCK[42] = buffer[i + 42];
            BLOCK[43] = buffer[i + 43];
            BLOCK[44] = buffer[i + 44];
            BLOCK[45] = buffer[i + 45];
            BLOCK[46] = buffer[i + 46];
            BLOCK[47] = buffer[i + 47];
            BLOCK[48] = buffer[i + 48];
            BLOCK[49] = buffer[i + 49];
            BLOCK[50] = buffer[i + 50];
            BLOCK[51] = buffer[i + 51];
            BLOCK[52] = buffer[i + 52];
            BLOCK[53] = buffer[i + 53];
            BLOCK[54] = buffer[i + 54];
            BLOCK[55] = buffer[i + 55];
            BLOCK[56] = buffer[i + 56];
            BLOCK[57] = buffer[i + 57];
            BLOCK[58] = buffer[i + 58];
            BLOCK[59] = buffer[i + 59];
            BLOCK[60] = buffer[i + 60];
            BLOCK[61] = buffer[i + 61];
            BLOCK[62] = buffer[i + 62];
            BLOCK[63] = buffer[i + 63];
        }
        public void setBlock(ref byte[] buffer, ref byte[] BLOCK, int i)
        {
            buffer[i] = BLOCK[0];
            buffer[i + 1] = BLOCK[1];
            buffer[i + 2] = BLOCK[2];
            buffer[i + 3] = BLOCK[3];
            buffer[i + 4] = BLOCK[4];
            buffer[i + 5] = BLOCK[5];
            buffer[i + 6] = BLOCK[6];
            buffer[i + 7] = BLOCK[7];
            buffer[i + 8] = BLOCK[8];
            buffer[i + 9] = BLOCK[9];
            buffer[i + 10] = BLOCK[10];
            buffer[i + 11] = BLOCK[11];
            buffer[i + 12] = BLOCK[12];
            buffer[i + 13] = BLOCK[13];
            buffer[i + 14] = BLOCK[14];
            buffer[i + 15] = BLOCK[15];
            buffer[i + 16] = BLOCK[16];
            buffer[i + 17] = BLOCK[17];
            buffer[i + 18] = BLOCK[18];
            buffer[i + 19] = BLOCK[19];
            buffer[i + 20] = BLOCK[20];
            buffer[i + 21] = BLOCK[21];
            buffer[i + 22] = BLOCK[22];
            buffer[i + 23] = BLOCK[23];
            buffer[i + 24] = BLOCK[24];
            buffer[i + 25] = BLOCK[25];
            buffer[i + 26] = BLOCK[26];
            buffer[i + 27] = BLOCK[27];
            buffer[i + 28] = BLOCK[28];
            buffer[i + 29] = BLOCK[29];
            buffer[i + 30] = BLOCK[30];
            buffer[i + 31] = BLOCK[31];
            buffer[i + 32] = BLOCK[32];
            buffer[i + 33] = BLOCK[33];
            buffer[i + 34] = BLOCK[34];
            buffer[i + 35] = BLOCK[35];
            buffer[i + 36] = BLOCK[36];
            buffer[i + 37] = BLOCK[37];
            buffer[i + 38] = BLOCK[38];
            buffer[i + 39] = BLOCK[39];
            buffer[i + 40] = BLOCK[40];
            buffer[i + 41] = BLOCK[41];
            buffer[i + 42] = BLOCK[42];
            buffer[i + 43] = BLOCK[43];
            buffer[i + 44] = BLOCK[44];
            buffer[i + 45] = BLOCK[45];
            buffer[i + 46] = BLOCK[46];
            buffer[i + 47] = BLOCK[47];
            buffer[i + 48] = BLOCK[48];
            buffer[i + 49] = BLOCK[49];
            buffer[i + 50] = BLOCK[50];
            buffer[i + 51] = BLOCK[51];
            buffer[i + 52] = BLOCK[52];
            buffer[i + 53] = BLOCK[53];
            buffer[i + 54] = BLOCK[54];
            buffer[i + 55] = BLOCK[55];
            buffer[i + 56] = BLOCK[56];
            buffer[i + 57] = BLOCK[57];
            buffer[i + 58] = BLOCK[58];
            buffer[i + 59] = BLOCK[59];
            buffer[i + 60] = BLOCK[60];
            buffer[i + 61] = BLOCK[61];
            buffer[i + 62] = BLOCK[62];
            buffer[i + 63] = BLOCK[63];
        }

        public byte[] Symmetricate_Old(byte[] a)
        {
            if ((a.Length & 255) == 0)
            {
                return a;
            }
            else
            {
                int rem = (64 - (a.Length & 63)) & 63;
                byte[] ret = new byte[a.Length + rem];
                a.CopyTo(ret, 0);
                return ret;
            }
        }

        int rem(int a)
        {
            return (64 - (a & 63)) & 63;
        }
        public byte[] Symmetricate(byte[] word)
        {
            int remainder = rem(word.Length);
            byte[] asymmetric = new byte[word.Length + 1];
            word.CopyTo(asymmetric, 1);
            asymmetric[0] = (byte)(rem(asymmetric.Length));
            byte[] symmetric = new byte[asymmetric.Length + rem(asymmetric.Length)];
            asymmetric.CopyTo(symmetric, 0);
            return symmetric;
        }
        public byte[] Desymmetricate(byte[] word)
        {
            int remainder = word[0];
            byte[] asymmetric = new byte[word.Length - 1 - remainder];
            for (int i = 1; i < asymmetric.Length + 1; i++) //fixed
            {
                asymmetric[i - 1] = word[i];
            }
            return asymmetric;
        }
        public void SWAP(ref int[] MAT8X8, int a, int b)
        {
            int tmp = MAT8X8[b];
            MAT8X8[b] = MAT8X8[a];
            MAT8X8[a] = tmp;
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
        public string GFM_ToStringTrunc()
        {
            string FULL_AUG = "";
            for (int i = 0; i < 8; i++)
            {
                if (i != 0) FULL_AUG += '\n';
                for (int j = 8; j < 16; j++)
                {
                    if (j >= 8)
                    {
                        FULL_AUG += GFM_AUG[(i << 3) + (j - 8)].ToString("X2");
                    }
                    else
                    {
                        FULL_AUG += GFM_TMP[(i << 3) + j].ToString("X2");
                    }
                    if (j != 15) FULL_AUG += ' ';
                }
            }
            return FULL_AUG;
        }
    }
}
