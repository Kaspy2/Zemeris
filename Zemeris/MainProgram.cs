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

            
            foreach(string dir in htmlFilesUrls)
            {
                ProcessHTML ph = new ProcessHTML(dir);
                string title = ph.getTitle();
                List<string> paragraphs = ph.getParagraphs();
                List<string> headings = ph.getHeadings("1");

                foreach(string par in paragraphs)
                {
                    SentDetection sentDetect = new SentDetection();
                    List<List<string>> sentences = sentDetect.detectSentences(par);

                    foreach(List<string> sent in sentences)
                    {
                        //Pos
                        //lemmat

                    }
                }

            }
            
        }
    }
}
