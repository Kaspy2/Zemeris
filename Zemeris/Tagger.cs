using edu.stanford.nlp.tagger.maxent;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Zemeris
{
    class Tagger
    {
        // Loading POS Tagger
        public List<Tuple<string, string>> Tag(string sentence)
        {
            var tagger = new MaxentTagger(@"spostag\models\wsj-0-18-bidirectional-nodistsim.tagger");
            //a maximum entropy tagger


            //now POS Tagging

            /*
            try
            {
                string stringWithTag = tagger.tagString(currStr[0].ToLower());
                //tagger.addTag(currStr[0].ToLower()) //returns an integer - different depending on word
                string tag = stringWithTag.Substring(stringWithTag.LastIndexOf('_') + 1);
                if (lemma != "")
                {
                    //Console.WriteLine(currStr[0] + " --> " + lemma + " : " + tag);
                    tempList.Add(tag);  //POS tag
                    cs++;
                    y.Add(tempList);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Exception!");
                arr[cf] = currStr[0];
                cf++;
            }
            */

            string taggedA = tagger.tagString(sentence);
            List<Tuple<string,string>> outp = new List<Tuple<string,string>>(); //2d array to hold word, POS Tag

            foreach(string pSplit in taggedA.Split(' '))
            {
                if (pSplit.Length>0 && pSplit!=" ") {
                    string word = pSplit.Substring(0, pSplit.LastIndexOf('_'));
                    string tag = pSplit.Substring(pSplit.LastIndexOf('_') + 1);

                    Tuple<string, string> currPSplit = new Tuple<string, string>(word, tag);
                    outp.Add(currPSplit);
                    Console.WriteLine(word + " : " + tag);
                }
            }


            Console.WriteLine("Tokenized tagged string is: "+taggedA);

            return outp;
        }

    }
}
