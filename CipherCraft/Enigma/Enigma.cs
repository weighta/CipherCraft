using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace CipherCraft
{
    class Enigma
    {
        RDA_cipher rda = new RDA_cipher();

        Plugboard pb = new Plugboard();
        Caesar caes = new Caesar();
        Rotor[] rot;
        public char[] inv;

        public int size;
        public int[] curr_comb;
        public int[] start_comb;
        public char[] latest_seq;
        public Enigma()
        {
            buildMachine(26, new int[6]);
        }
        public Enigma(string key)
        {
            buildMachine(26, new int[6]);
            buildRots(key);
        }
        public void buildMachine(int perm_size, int[] comb)
        {
            rot = new Rotor[comb.Length + 1];
            size = perm_size;

            curr_comb = new int[comb.Length];
            start_comb = new int[comb.Length];
            latest_seq = new char[(rot.Length << 1)];

            comb.CopyTo(curr_comb, 0);
            comb.CopyTo(start_comb, 0);

        }
        public void buildRots(string key)
        {
            int a = curr_comb.Length << 1;
            int j = 0;
            byte[] khash = rda.kHashingAlgorithm(key);
            byte[] s = rda.keyExpansion(khash, a);
            for (int i = 0; i < rot.Length; i++)
            {
                char[] p = new char[size];
                int f = 0;
                while (f < size)
                {
                    int c = s[j] % size;
                    if (!Print.contains(p, c, f))
                    {
                        p[f] = (char)c;
                        f++;
                    }
                    j++;
                    if (j >= s.Length)
                    {
                        a++;
                        s = rda.keyExpansion(khash, a);
                    }
                }
                rot[i] = new Rotor(p);
                if (i < start_comb.Length)//align rots
                {
                    for (int k = 0; k < start_comb[start_comb.Length - i - 1]; k++)
                    {
                        rot[i].rot(true);
                    }
                    rot[i].save();
                }

            }
            inv = new char[size];
            //sayRots();
        }

        public string enc(string a) //CAPS traditional with spaces
        {
            reset();
            //Print.say(Print.ARR_TO_STR(curr_comb));
            string ret = "";
            for (int i = 0; i < a.Length; i++)
            {
                turn();
                if (a[i] == 0x20)
                {
                    ret += ' ';
                }
                else
                {
                    try
                    {
                        ret += (char)(sub((char)(a[i] - 65)) + 65);
                    }
                    catch
                    {
                        Console.WriteLine("Don't throw junk into the enigma machine such as '" + ret[i] + "'");
                    }
                }
            }
            return ret;
        }
        public string enc(string a, string[] board) //CAPS traditional with spaces, and plugboard
        {
            reset();
            pb.inst(board);
            //Print.say(Print.ARR_TO_STR(curr_comb));
            string ret = "";
            for (int i = 0; i < a.Length; i++)
            {
                turn();
                if (a[i] == 0x20)
                {
                    ret += ' ';
                }
                else
                {
                    try
                    {
                        ret += pb.apply(sub((char)(a[i] - 65)));
                    }
                    catch
                    {
                        Console.WriteLine("Don't throw junk into the enigma machine such as '" + ret[i] + "'");
                    }
                }
            }
            return ret;
        }
        public string enc(string a, int phase) //Phase included, NO SPACES ALLOWED!!
        {
            reset();
            //Print.say(Print.ARR_TO_STR(curr_comb));
            string ret = "";
            for (int i = 0; i < a.Length; i++)
            {
                turn();
                if (a[i] - phase < 0)
                {
                    throw new Exception("input domain was less than phase size");
                }
                ret += (char)(sub((char)(a[i] - phase)) + phase);
            }
            return ret;
        }
        public string enc(string a, int phase, string[] board) //Phase and plugboard included, NO SPACES ALLOWED!!
        {
            reset();
            pb.inst(board);
            //Print.say(Print.ARR_TO_STR(curr_comb));
            string ret = "";
            for (int i = 0; i < a.Length; i++)
            {
                turn();
                ret += sub((char)(a[i] - phase));
            }
            return pb.apply(ret.ToCharArray());
        }

        #region FOR EXTRA ENCRYPTION PURPOSES SUCH AS GALOIS FIELD

        public void enc(ref char[] a) //straight up 0-25, no phase bs
        {
            reset();
            for (int i = 0; i < a.Length; i++)
            {
                turn();
                a[i] = sub(a[i]);
            }
        }
        public void enc(ref char[] a, string[] plugboard)
        {
            reset();
            pb.inst(plugboard);
            for (int i = 0; i < a.Length; i++)
            {
                turn();
                a[i] = sub(a[i]);
            }
            pb.apply(a).ToCharArray().CopyTo(a, 0);
        }
        public void dec(ref char[] a)
        {
            reset();
            for (int i = 0; i < a.Length; i++)
            {
                turn();
                a[i] = invSub(a[i]);
            }
        }
        public void dec(ref char[] a, string[] plugboard)
        {
            reset();
            pb.inst(plugboard);
            pb.unapply(Print.charARRtoSTR(a)).CopyTo(a, 0);
            for (int i = 0; i < a.Length; i++)
            {
                turn();
                a[i] = invSub(a[i]);
            }
        }
        #endregion

        public string dec(string a) //traditional
        {
            reset();
            string ret = "";
            for (int i = 0; i < a.Length; i++)
            {
                turn();
                if (a[i] == 0x20)
                {
                    ret += ' ';
                }
                else
                {
                    try
                    {
                        ret += (char)(invSub((char)(a[i] - 65)) + 65);
                    }
                    catch
                    {
                        Console.WriteLine("Don't throw junk into the enigma machine such as '" + ret[i] + "'");
                    }
                }
            }
            return ret;
        }
        public string dec(string a, string[] board)//traditional with spaces and plugboard
        {
            reset();
            pb.inst(board);
            a = Print.charARRtoSTR(pb.unapplyWithSpace(a));
            string ret = "";
            for (int i = 0; i < a.Length; i++)
            {
                turn();
                if (a[i] == 0x20)
                {
                    ret += ' ';
                }
                else
                {
                    try
                    {
                        ret += (char)(invSub((char)((a[i])))+65);
                    }
                    catch
                    {
                        Console.WriteLine("Don't throw junk into the enigma machine such as '" + ret[i] + "'");
                    }
                }
            }
            return ret;
        }
        public string dec(string a, int phase) //Phase included, NO SPACES ALLOWED!!
        {
            reset();
            //Print.say(Print.ARR_TO_STR(curr_comb));
            string ret = "";
            for (int i = 0; i < a.Length; i++)
            {
                turn();
                if (a[i] - phase < 0)
                {
                    throw new Exception("input domain was less than phase size");
                }
                ret += (char)(invSub((char)(a[i] - phase)) + phase);
            }
            return ret;
        }
        public string dec(string a, int phase, string[] board) //Phase and plugboard included, NO SPACES ALLOWED!!
        {
            reset();
            pb.inst(board);
            a = Print.charARRtoSTR(pb.unapply(a));
            //Print.say(Print.ARR_TO_STR(curr_comb));
            string ret = "";
            for (int i = 0; i < a.Length; i++)
            {
                turn();
                ret += (char)(invSub((char)(a[i])) + phase);
            }
            return ret;
        }

        char sub(char a)
        {
            latest_seq[0] = a;
            for (int i = 0; i < (rot.Length << 1) - 1; i++)
            {
                int index = -Math.Abs(i - curr_comb.Length) + curr_comb.Length;
                //MessageBox.Show("state: " + (a+0) + "\nrotor: " + index + " rot pick: " + index + " current rot: " + rot[index]);
                a = rot[index].chars[a];
                latest_seq[i + 1] = a;
            }
            return a;
        }
        char invSub(char a)
        {
            for (int i = 0; i < size; i++)
            {
                inv[sub((char)i)] = (char)i;
            }
            return inv[a];
        }
        void turn()
        {
            int i = 1;
            bool go = true;
            while (go)
            {
                if (curr_comb[curr_comb.Length - i] >= size - 1)
                {
                    curr_comb[curr_comb.Length - i - 1]++;
                    curr_comb[curr_comb.Length - i] = 0;
                    go = curr_comb[curr_comb.Length - i - 1] == size;
                    rot[i - 1].rot(true);
                    rot[i].rot(true);
                }
                else
                {
                    curr_comb[curr_comb.Length - i]++;
                    rot[i - 1].rot(true);
                    go = false;
                }
                i++;
            }
        }
        void reset()
        {
            start_comb.CopyTo(curr_comb, 0); //reset combination
            for (int i = 0; i < rot.Length; i++) rot[i].reset();
            //Print.say(Print.ARR_TO_STR(curr_comb) + "\n" + Print.ARR_TO_STR(rot[0].chars));
        }
        void sayRots()
        {
            string ret = "";
            for (int i = 0; i < rot.Length; i++)
            {
                ret += Print.ARR_TO_STR(rot[i].chars) + "\n";
            }
            MessageBox.Show(ret);
        }
        public string getCurrComb()
        {
            string ret = "";
            for (int i = 0; i < curr_comb.Length; i++)
            {
                ret += curr_comb[i] + " ";
            }
            return ret;
        }
        public string getLatestSeq(int phase)
        {
            string ret = "";
            for (int i = 0; i < latest_seq.Length; i++)
            {
                ret += (char)(latest_seq[i] + phase);
                if (i < latest_seq.Length - 1) ret += " ";
            }
            return ret;
        }
        public string ToString(int phase)
        {
            string ret = "";
            for (int i = 0; i < rot.Length; i++)
            {
                ret += Print.ARR_TO_STR(rot[i].chars, phase) + "\n";
            }
            return ret;
        }
        public string getElements(int phase)
        {
            string ret = "";
            for (int i = 0; i < size; i++)
            {
                ret += (char)(i + phase);
                if (i < size - 1) ret += " ";
            }
            return ret;
        }
    }
}
