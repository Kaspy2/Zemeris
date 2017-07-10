using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HtmlAgilityPack;
namespace Zemeris
{
    class MainProgram
    {
        static void Main(string[] args)
        {
            List<string> htmlFilesUrls = new List<string>();
            htmlFilesUrls.Add("C:/Users/nikolai/Desktop/TestAbby/1page.htm");

            List<Tuple<string, string, int, int, int, string>> wordIndex = new List<Tuple<string, string, int, int, int, string>>(); //word, lemma,doc,par,sent,pos
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

                    for (int parNo = 0; parNo < htmlFilesUrls.Count; parNo++)
                    {
                        string par = paragraphs.ElementAt(parNo);

                        SentDetection sentDetect = new SentDetection();
                        List<List<string>> sentences = sentDetect.detectSentences(par);

                        for (int sentNo =0; sentNo<sentences.Count; sentNo++)
                        {
                            List<string> sent = sentences.ElementAt(sentNo);
                            var taggedoutput = tagger.Tag(sent); //2d array to hold word, lemma,  POS Tag
                            
                            foreach (var taggedWord in taggedoutput)
                            {
                                if (!inpSt.isStopWord(taggedWord.Item1))
                                {
                                    wordIndex.Add(new Tuple<string, string, int, int, int, string>(taggedWord.Item1, taggedWord.Item2, docNo, parNo, sentNo, taggedWord.Item3));
                                }
                            }  
                        }
                    }
                }
            }
        }
    }
}
