using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HtmlAgilityPack;
using System.Text.RegularExpressions;
using System.IO;

namespace Zemeris
{
    class ProcessHTML
    {
        private string fileUrl;// { get; set; }
        public HtmlDocument htmlDoc { get; set; }

        public ProcessHTML(string fileUrl)
        {
            this.fileUrl = fileUrl;
            htmlDoc = new HtmlDocument();
            htmlDoc.Load(fileUrl);

        }

        public string getTitle()
        {
            return htmlDoc.DocumentNode.SelectSingleNode("//title").InnerText;
        }

        public List<string> getParagraphs()
        {
            var data = htmlDoc.DocumentNode.SelectNodes("//p");

            Regex rgx = new Regex("&nbsp;");

            List<string> paragraphs = new List<string>();
            if (data == null) return new List<string>();

            foreach (var d in data)
            {
                string result = rgx.Replace(d.InnerText, " ");
                paragraphs.Add(result);
                
            }

            return paragraphs;
        }

        public List<string> getHeadings(string h)
        {
            var data = htmlDoc.DocumentNode.SelectNodes("//h"+h);
            Regex rgx = new Regex("&nbsp;");

            List<string> heads = new List<string>();

            if (data == null) return new List<string>();
            foreach (var d in data)
            {
                string result = rgx.Replace(d.InnerText, " ");
                heads.Add(result);
              
            }

            return heads;
        }
        static void Main(string[] args)
        {
            Console.WriteLine(Directory.GetCurrentDirectory());
            ProcessHTML ph = new ProcessHTML((@"./ABBYY Output/1page.htm")); //directory to ABBYY output
            Console.WriteLine(ph.getTitle());
            Console.WriteLine();
            ph.getParagraphs();
            Console.ReadLine();

        }
    }
}
