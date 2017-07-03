using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Zemeris
{
    class TSVParser
    {
        public List <string []> Parse(string path)
        {
            List<string[]> tsvWords = new List<string[]>();
            //char[] delimiters = new char[] { '\t' };
            using (StreamReader reader = new StreamReader(path))
            {
                while (true)
                {
                    string line = reader.ReadLine();
                    if (line == null)
                    {
                        break;
                    }

                    //string[] parts = line.Split(delimiters);
                    string[] parts = line.Split('\t');
                    //Console.WriteLine("{0} field(s)", parts.Length);

                    if (parts[11] != "" && parts[6]!="left")
                    {
                        //add an array containing word, x, y, page, block
                        tsvWords.Add(new string []{ parts[11], parts[6], parts[7], parts[1], parts[2] });
                        //Console.WriteLine("Text: "+parts[11]);
                    }
                    
                }
            }
            return tsvWords;
        }
    }
}
