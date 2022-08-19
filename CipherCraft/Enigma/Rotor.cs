using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CipherCraft
{

    public class Rotor
    {
        char[] tmp;
        public char[] chars_tmp;
        public char[] chars;
        public Rotor(char[] a)
        {
            chars = a;

            chars_tmp = new char[a.Length];
            a.CopyTo(chars_tmp, 0);
            tmp = new char[chars.Length];
        }
        public void rot(bool b)
        {
            if (b) //left
            {
                tmp[tmp.Length - 1] = chars[0];
                for (int i = 0; i < tmp.Length - 1; i++)
                {
                    tmp[i] = chars[i + 1];
                }
            }
            else //right
            {
                tmp[0] = chars[chars.Length];
                for (int i = 1; i < tmp.Length; i++)
                {
                    tmp[i] = chars[i - 1];
                }
            }
            tmp.CopyTo(chars, 0);
        }
        public void save()
        {
            chars.CopyTo(chars_tmp, 0);
        }
        public void reset()
        {
            chars_tmp.CopyTo(chars, 0);
        }
    }
}
