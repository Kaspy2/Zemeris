using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using edu.stanford.nlp.process;
using edu.stanford.nlp.ling;
namespace SentenceDetection
{
    class SentenceDetection
    {
       
        private string convertListToStr(List<string> al)
        {
            List<String> list = new List<string>();
            StringBuilder sb = new StringBuilder();
            foreach(string word in al)
            {
                string value = word;
                if (value.Equals("-LRB-")) value = "(";
                else if (value.Equals("-RRB-")) value = ")";
                else if (value.Equals("-RSB-")) value = "]";
                else if (value.Equals("-LSB-")) value = "[";
                else if (value.Equals("-LCB-")) value = "{";
                else if (value.Equals("-RCB-")) value = "}";

                sb.Append(value+" ");
            }
            return sb.ToString().Trim();
        }

        private List<List<string>> postProcessSentences(DocumentPreprocessor sentencesIn)
        {
            //convert from java objects into easier to work with nested lists
            List<List<string>> sentences = new List<List<string>>();
            foreach (java.util.ArrayList sent in sentencesIn)
            {
                List<string> temp = new List<string>();
                foreach (CoreLabel word in sent)
                {
                    temp.Add(word.value());
                }
                sentences.Add(temp);
            }


            bool mergeSent = false;
            bool addedAlready = false;
            List<List<string>> sentences_final = new List<List<string>>();

            //for every sentence
            for (int i = 0; i < sentences.Count(); i++)
            {
                List<string> sent = sentences.ElementAt(i);
                addedAlready = false;
                
                //if this sentence needs to be merged with the previous one
                if (mergeSent)
                {
                    if (sent.ElementAt(0).Equals("No."))
                    {
                        sentences_final.RemoveAt(sentences_final.Count() - 1);
                        sentences_final.Add(sentences.ElementAt(i - 1).Concat(sentences.ElementAt(i)).ToList());

                        mergeSent = false;
                        addedAlready = true;
                    }
                }

                //check if the last two words of this sentence are Pat . as this means 
                //that in the original sentence it wouldve been written as Pat. (for Pat. No.)
                if (sent.ElementAt(sent.Count() - 2).Equals("Pat") && sent.ElementAt(sent.Count() - 1).Equals("."))
                {
                    mergeSent = true;
                }

                //if there already was an addition
                if (!addedAlready)
                {
                    sentences_final.Add(sent);
                }
            }
            return sentences_final;
        }

        public  List<string> detectSentences(String text)
        {
            java.io.StringReader _stringReader = new java.io.StringReader(text);
            DocumentPreprocessor sentences_java = new DocumentPreprocessor(_stringReader, DocumentPreprocessor.DocType.Plain);

            List<List<string>> sentences = postProcessSentences(sentences_java);

            List<string> outp = new List<string>();
            foreach (List<string> sent in sentences)
            {   
                outp.Add(convertListToStr(sent).Trim());
            }
            return outp;
        }

        static void Main(string[] args)
        {
            string paragraph = System.IO.File.ReadAllText("text.txt");
            SentenceDetection sd = new SentenceDetection();
            List<string> sentences = sd.detectSentences(paragraph);
           
            foreach(string sentence in sentences){
                Console.WriteLine(sentence);
                Console.WriteLine("");
            }
            

            Console.ReadLine();

        }
    }
}
