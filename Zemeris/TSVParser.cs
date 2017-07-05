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
        public List <string []> Parse(string path, bool emptyInclude)
        {
            List<string[]> tsvWords = new List<string[]>();
            //char[] delimiters = new char[] { '\t' };
            using (StreamReader reader = new StreamReader(path))
            {

                if (emptyInclude)
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


                        if (parts[6] != "left")
                        {
                            //add an array containing word, left, top, width, height, page, conf
                            tsvWords.Add(new string[] { parts[11], parts[6], parts[7], parts[8], parts[9], parts[1], parts[10] });
                            //Console.WriteLine("Text: "+parts[11]);
                        }
                    }
                }
                else
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


                        if (parts[11].Trim() != "" && parts[6] != "left")
                        {
                            //add an array containing word, x, y, page, block
                            tsvWords.Add(new string[] { parts[11], parts[6], parts[7], parts[1], parts[2] });
                            //Console.WriteLine("Text: "+parts[11]);
                        }
                    }
                }
            }
            return tsvWords;
        }
    }
}
