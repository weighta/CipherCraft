using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CipherCraft
{
    public struct Word
    {
        public string word;
        public int numOcc;
    }
    public class Paragraph_Analysis
    {
        public Paragraph_Analysis()
        {

        }
        void inst()
        {

        }
        public string histDecode(string para)
        {
            string ret = "";

            string buffer = "";
            for (int i = 0; i < para.Length; i++)
            {
                if (para[i] >= 65 && para[i] <= 90)
                {
                    buffer += (char)(para[i] + 32);
                }
                else if (para[i] >= 97 && para[i] <= 122)
                {
                    buffer += para[i];
                }
                else if (para[i] == 32)
                {
                    buffer += ' ';
                }
                else if (para[i] == '\n')
                {
                    buffer += ' ';
                }
            }
            string[] words = buffer.Split(' ');
            List<Word> uniqueWords = new List<Word>();
            Word w;
            for (int i = 0; i < words.Length; i++)
            {
                if (!wordContains(uniqueWords, words[i]))
                {
                    w = new Word();
                    w.word = words[i];
                    w.numOcc++;
                    for (int j = i + 1; j < words.Length; j++)
                    {
                        if (words[j].Equals(words[i])) w.numOcc++;
                    }
                    uniqueWords.Add(w);
                }
            }
            //Print.say(listWords(uniqueWords));
            Word[] wordsSorted = sortByOcc(uniqueWords);
            ret += listWords(wordsSorted);
            //Print.say(ret);
            //Print.say(Print.strARRtoSTR(words));
            return ret;
        }
        bool wordContains(List<Word> w, string search)
        {
            for (int i = 0; i < w.Count; i++)
            {
                if (w[i].word.Equals(search)) return true;
            }
            return false;
        }
        bool wordContains(Word[] w, int searchLen, string search)
        {
            for (int i = 0; i < searchLen; i++)
            {
                if (w[i].word.Equals(search)) return true;
            }
            return false;
        }
        int numOccurances(string[] a, string search)
        {
            int ret = 0;
            for (int i = 0; i < a.Length; i++)
            {
                if (a[i].Equals(search))
                {
                    ret++;
                }
            }
            return ret;
        }
        string listWords(List<Word> w)
        {
            string tosay = "";
            //Print.say(w.Count + " is the number of unique words!");
            for (int i = 0; i < w.Count; i++)
            {
                tosay += w[i].word + " (" + w[i].numOcc + ")\n";
            }
            return tosay;
        }
        string listWords(Word[] w)
        {
            string tosay = "";
            //Print.say(w.Count + " is the number of unique words!");
            for (int i = 0; i < w.Length; i++)
            {
                tosay += w[i].word + " (" + w[i].numOcc + ")\n";
            }
            return tosay;
        }
        Word[] sortByOcc(List<Word> w)
        {
            Word[] ret = new Word[w.Count];
            for (int i = 0; i < ret.Length; i++)
            {
                int index = i;
                int highestFound = 0;
                for (int j = 0; j < ret.Length; j++)
                {
                    //Print.say("iteration " + i +"\nif " + w[j].numOcc + " is greater than " + w[index].numOcc + "\n and ret with\n" + listWords(ret) + "\n contains " + w[j].word + "? " + wordContains(ret, i, w[j].word));
                    if (w[j].numOcc > highestFound && !wordContains(ret, i, w[j].word))
                    {
                        index = j;
                        highestFound = w[j].numOcc;
                    }
                }
                ret[i] = w[index];
            }
            return ret;
        }
    }
}
