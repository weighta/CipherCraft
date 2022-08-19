using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace CipherCraft
{
    public class Dictionary
    {
        public string[] language = new string[] { "EN", "GE" };

        public Dictionary()
        {

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
                if (i == 100000)
                {
                    Console.WriteLine(language[langIndex] + " Quarter of the way, don't stop!");
                }
            }
            Console.WriteLine(language[langIndex] + " done");
            return findings;
        }
    }
}
