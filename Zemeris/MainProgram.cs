using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HtmlAgilityPack;
using System.IO;
using CsvHelper;

namespace Zemeris
{
    class MainProgram
    {
        static void Main(string[] args)
        {
            List<string> htmlFilesUrls = new List<string>();
            Console.WriteLine(Directory.GetCurrentDirectory());
            
            foreach (string f in Directory.GetFiles(Directory.GetCurrentDirectory() + "\\ABBYY Output\\"))
            {
                Console.WriteLine(f);
                htmlFilesUrls.Add(f);
            }
            List<Tuple<string, string, int, int, int, string>> wordIndex = new List<Tuple<string, string, int, int, int, string>>(); //word, lemma,doc,par,sent,pos
            List<Tuple<string, string,  int, int, int, string>> wordGroupings = new List<Tuple<string, string, int, int, int, string>>();

            using (Tagger tagger = new Tagger())
            {
                InputString inpSt = new InputString();
                
                for (int docNo = 0; docNo < htmlFilesUrls.Count; docNo++)
                {
                    string dir = htmlFilesUrls.ElementAt(docNo);

                    ProcessHTML ph = new ProcessHTML(dir);
                    string title = ph.getTitle();
                    List<string> paragraphs = ph.getParagraphs();
                    List<string> headings = ph.getHeadings("1");

                    for (int parNo = 0; parNo < paragraphs.Count; parNo++)
                    {
                        string par = paragraphs.ElementAt(parNo);

                        SentDetection sentDetect = new SentDetection();
                        List<List<string>> sentences = sentDetect.detectSentences(par);

                        
                        for (int sentNo =0; sentNo<sentences.Count; sentNo++)
                        {
                            String prevWord = null;
                            List<string> sent = sentences.ElementAt(sentNo);
                            var taggedoutput = tagger.Tag(sent); //2d array to hold word, lemma,  POS Tag
                            
                            foreach (var taggedWord in taggedoutput)
                            {
                                if (!(inpSt.isStopWord(taggedWord.Item1) || inpSt.isStopWord(taggedWord.Item2)))
                                {
                                    if(prevWord == null) prevWord = taggedWord.Item2;
                                    else
                                    {
                                        wordGroupings.Add(new Tuple<string, string, int, int, int, string>(prevWord,taggedWord.Item2, docNo, parNo, sentNo, taggedWord.Item3));
                                        prevWord = taggedWord.Item2;
                                    }

                                    wordIndex.Add(new Tuple<string, string, int, int, int, string>(taggedWord.Item1.ToLower(), taggedWord.Item2.ToLower(), docNo, parNo, sentNo, taggedWord.Item3));
                                }
                            }  
                        }
                    }
                }
            }

            using (StreamWriter file = new StreamWriter(@"./OutputTSV/TestFile.csv", false))
            {
                var csv = new CsvWriter(file);
                csv.WriteRecords(wordIndex);
            }

            using (StreamWriter file = new StreamWriter(@"./OutputTSV/TestFile2.csv", false))
            {
                
                var csv = new CsvWriter(file);
                csv.WriteRecords(wordGroupings);
            }




        }
    }
}
