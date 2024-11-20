using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Drawing;
using System.Threading;
using System.Runtime.InteropServices;
using System.Diagnostics;
using System.Globalization;
using System.Windows.Forms;

namespace CipherCraft
{
    public struct WordMatch
    {
        public string Word;
        public Point loc;
    }
    public struct WordMatchGF
    {
        public string Word;
        public int loc;
    }
    public class Dictionary : GraphicalBase
    {
        public string[] language = new string[] { "EN", "GE" };
        string[] dict;
        public int wordsFound = 0;
        public int requiredChecks = 0;
        public int totalChecks = 0;
        public int threadsFinished = 0;
        public Dictionary()
        {

        }
        public Dictionary(PictureBox pictureBox1) : base(pictureBox1)
        {
            dict = File.ReadAllLines(language[0] + ".txt"); //english
            requiredChecks = dict.Length;
        }
        public override void bitmapFunction(int x)
        {
            throw new NotImplementedException();
        }

        ThreadInfo1[] threadInfo1;
        public List<WordMatchGF>[][][] dictionaryCheckGF_threaded(int[] fields, char[][][][] chars, int threadCount, GF_P_N gfp_n)
        {
            setProgress2(0);
            setProgress3(0);
            wordsFound=0;
            List<WordMatchGF>[][][] words = new List<WordMatchGF>[fields.Length][][]; //field, reduction, poly, words
            int numReductions = chars[0].Length; //CHANGE THIS LATER IF NUM REDUCTIONS PER FIELD CHANGES
            for (int i = 0; i < words.Length; i++)
            {
                words[i] = new List<WordMatchGF>[numReductions][];
                for (int j = 0; j < words[i].Length; j++)
                {
                    words[i][j] = new List<WordMatchGF>[chars[i][j].Length];
                    for (int k = 0; k < words[i][j].Length; k++) words[i][j][k] = new List<WordMatchGF>();
                }
            }
            threadInfo1 = new ThreadInfo1[threadCount];
            for (int i = 0; i < threadInfo1.Length; i++) threadInfo1[i] = new ThreadInfo1() { threadCount = threadCount, minlength =3 };
            Thread[] threads = new Thread[threadCount];
            for (int i = 0; i < fields.Length;)
            {
                totalChecks = 0;
                for (int j = 0; j < threads.Length; j++)
                {
                    threads[j] = new Thread(new ParameterizedThreadStart(dictionaryCheckGF_threaded));
                    threadInfo1[j].chars = chars[i];
                    threadInfo1[j].threadID = j;
                    threads[j].Start(threadInfo1[j]);
                }
                ThreadsWait(threads);
                for (int j = 0; j < threads.Length; j++)
                {
                    for (int k = 0; k < words[i].Length; k++) //Reductions
                    {
                        for (int l = 0; l < words[i][k].Length; l++) //Polys
                        {
                            for (int m = 0; m < threadInfo1[j].wordMatches[k][l].Count; m++)
                            {
                                words[i][k][l].Add(threadInfo1[j].wordMatches[k][l][m]);
                            }
                        }
                    }
                }
                setProgress2((int)(((float)++i / fields.Length) * 100)); //Reducing finished
            }
            
            return words;
        }
        private void ThreadsWait(Thread[] threads)
        {
            for (int i = 0; i < threads.Length; i++)
            {
                while (threads[i].IsAlive)
                {
                    int progress = (int)(((float)totalChecks / requiredChecks) * 100);
                    if (progress < 0) throw new Exception("why");
                    setProgress3(progress);
                    Thread.Sleep(100);
                }
            }
        }
        public void dictionaryCheckGF_threaded(object obj)
        {
            ThreadInfo1 ti = (ThreadInfo1)obj;
            dictionaryCheckGF_threaded(ti.chars, ti.threadID, ti.threadCount, ti.minlength);
        }
        struct ThreadInfo1
        {
            public char[][][] chars;
            public List<WordMatchGF>[][] wordMatches;
            public int threadID;
            public int threadCount;
            public int minlength;
        }

        public void dictionaryCheckGF_threaded(char[][][] chars, int threadID, int threadCount, int minlength)
        {
            int numPolys = chars[0].Length;
            List<WordMatchGF>[][] wordMatch = new List<WordMatchGF>[chars.Length][]; //reductions, polys, words
            for (int i = 0; i < wordMatch.Length; i++)
            {
                wordMatch[i] = new List<WordMatchGF>[numPolys];
                for (int j = 0; j < wordMatch[i].Length; j++)
                {
                    wordMatch[i][j] = new List<WordMatchGF>();
                }
            }
            int wordsPerThread = dict.Length / threadCount;
            int start = threadID * wordsPerThread;
            int end = (threadID+1) * wordsPerThread;
            if (threadID == threadCount - 1) end = dict.Length;
            for (int i = start; i < end; i++)
            {
                string word = dict[i];
                if (word.Length >= minlength)
                {
                    int matchNeed = word.Length;
                    for (byte j = 0; j < chars.Length; j++) //reductions
                    {
                        for (int k = 0; k < chars[j].Length; k++) //polys
                        {
                            int currMatch = 0;
                            for (int l = 0; l < chars[j][k].Length - word.Length; l++)
                            {
                                if (chars[j][k][l] == word[currMatch])
                                {
                                    currMatch++;
                                    if (currMatch == matchNeed)
                                    {
                                        currMatch = 0;
                                        WordMatchGF match = new WordMatchGF();
                                        match.Word = word;
                                        Console.WriteLine("found: " + word);
                                        match.loc = l;
                                        wordMatch[j][k].Add(match);
                                        wordsFound++;
                                    }
                                }
                                else currMatch = 0;
                            }

                        }
                    }
                }
                totalChecks++;
            }
            threadInfo1[threadID].wordMatches = wordMatch;
        }
        public WordMatchGF[] listToArr_wordMatch(List<WordMatchGF> list)
        {
            WordMatchGF[] ret = new WordMatchGF[list.Count];
            for (int i = 0; i < ret.Length; i++)
            {
                ret[i] = list[i];
            }
            return ret;
        }
        public string dictionaryCheck(string[] lines, int minlength, int langIndex)
        {
            string findings = "";
            string[] dict = File.ReadAllLines(language[langIndex] + ".txt");
            for (int i = 0; i < dict.Length; i++) //word to search
            {
                if (dict[i].Length >= minlength)
                {
                    for (int j = 0; j < lines.Length; j++)
                    {
                        if (dict[i].Length <= lines[j].Length)
                        {
                            for (int k = 0; k < lines[j].Length - (dict[i].Length - 1); k++)
                            {
                                if (lines[j][k] == dict[i][0])
                                {
                                    bool match = true;
                                    for (int l = 1; l < dict[i].Length; l++)
                                    {
                                        if (dict[i][l] != lines[j][k + l])
                                        {
                                            match = false;
                                            break;
                                        }
                                    }
                                    if (match)
                                    {
                                        findings += dict[i] += " (" + (k) + ", " + (j+1) + ")\n";
                                    }
                                }
                            }
                        }
                        else
                        {
                            j = lines.Length;
                        }
                    }
                }
            }
            return findings;
        }
        public string dictionaryCheck(string line, int minlength, int langIndex)
        {
            return dictionaryCheck(new string[1] { line }, minlength, langIndex);
        }
    }
}
