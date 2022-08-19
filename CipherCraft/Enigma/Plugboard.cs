using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CipherCraft
{
    public class Plugboard
    {
        //0 1 2 3 4 5 6 7 8 9
        //A
        string[] sub;
        public Plugboard()
        {

        }
        public Plugboard(string[] sub)
        {
            inst(sub);
        }
        public void inst(string[] sub)
        {
            if (valid(sub)) this.sub = sub;
            else throw new Exception("The plugboard is invalid because decryption will have conflict");
        }
        public bool valid(string[] board)
        {
            string u = "";
            for (int i = 0; i < board.Length; i++)
            {
                string toAdd = "";
                for (int j = 0; j < board[i].Length; j++)
                {
                    if (u.Contains(board[i][j])) return false;
                    toAdd += board[i][j];
                }
                u += toAdd;
            }
            return true;
        }
        public string apply(char[] a) //0-size
        {
            string ret = "";
            for (int i = 0; i < a.Length; i++)
            {
                ret += sub[a[i]];
            }
            return ret;
        }
        public string apply(char a) //0-size
        {
            return sub[a];
        }
        public char[] unapply(string a)
        {
            List<char> found = new List<char>();
            string buffer = "";
            for (int i = 0; i < a.Length; i++)
            {
                buffer += a[i];
                if (Print.search(sub, buffer))
                {
                    found.Add((char)indexOfElement(buffer));
                    buffer = "";
                }
            }
            return Print.charListToCharARR(found);
        }
        public char[] unapplyWithSpace(string a)
        {
            List<char> found = new List<char>();
            string buffer = "";
            for (int i = 0; i < a.Length; i++)
            {
                if (a[i] == ' ')
                {
                    found.Add(' ');
                }
                else
                {
                    buffer += a[i];
                    if (Print.search(sub, buffer))
                    {
                        found.Add((char)indexOfElement(buffer));
                        buffer = "";
                    }
                }
            }
            return Print.charListToCharARR(found);
        }
        public int indexOfElement(string e)
        {
            for (int i = 0; i < sub.Length; i++)
            {
                if (e.Equals(sub[i])) return i;
            }
            throw new Exception("element '" + e + "' not found");
        }
    }
}
