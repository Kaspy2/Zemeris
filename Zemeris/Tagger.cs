using edu.stanford.nlp.tagger.maxent;
using LemmaSharp.Classes;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Zemeris
{
    class Tagger
    {
        // Loading POS Tagger
        public List<Tuple<string, string, string>> Tag(List<string> sentence)
        {
            var currentDirectory = Directory.GetCurrentDirectory();
            var dataFilePath = string.Format("{0}/{1}/{2}", currentDirectory, "../../../Lemma/Test/Data/Custom", "full7z-mlteast-en-modified.lem");
            
            var tagger = new MaxentTagger(@"spostag\models\wsj-0-18-bidirectional-nodistsim.tagger");
            //a maximum entropy tagger

            StringBuilder sb = new StringBuilder();
            foreach(string s in sentence)
            {
                sb.Append(s + " ");
            }
            
            string taggedA = tagger.tagTokenizedString(sb.ToString().Trim());
            List<Tuple<string,string,string>> outp = new List<Tuple<string,string,string>>(); //2d array to hold word, lemma,  POS Tag

            using (var fstream = File.OpenRead(dataFilePath))
            {
                var lemmatizer = new Lemmatizer(fstream);
                foreach (string pSplit in taggedA.Split(' '))
                {
                    if (pSplit.Length > 0 && pSplit != " ")
                    {
                        string word = pSplit.Substring(0, pSplit.LastIndexOf('_'));
                        string tag = pSplit.Substring(pSplit.LastIndexOf('_') + 1);
                        string lemma = lemmatizer.Lemmatize(word);
                        Tuple<string, string, string> currPSplit = new Tuple<string, string, string>(word, lemma, tag);
                        outp.Add(currPSplit);
                        //Console.WriteLine(word + " : " + tag);
                    }
                }
            }


            //Console.WriteLine("Tokenized tagged string is: "+taggedA);

            return outp;
        }

    }
}



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

/*
var currentDirectory = Directory.GetCurrentDirectory();
var dataFilePath = string.Format("{0}/{1}/{2}", currentDirectory, "../../../Lemma/Test/Data/Custom", "full7z-mlteast-en-modified.lem");
using (var fstream = File.OpenRead(dataFilePath))
{
var lemmatizer = new Lemmatizer(fstream);

int cs = 0, cf = 0;

string[] arr = new string[1160];

foreach (string[] currStr in x)
{
string lemma = lemmatizer.Lemmatize(currStr[0]);    //finding the lemma of currStr[0] - where currStr[0] is the word
List<string> tempList = new List<string>();
tempList.Add(currStr[0]);   //word
tempList.Add(lemma);        //lemma
tempList.Add(currStr[1]);   //x >>>> TO BECOME PARAGRAPH NUM
tempList.Add(currStr[2]);   //y >>>> TO BECOME SENTENCE NUM
tempList.Add(currStr[3]);   //page
tempList.Add(currStr[4]);   //block

//POS WAS HERE
}

//print the exception words (if any)
Array.Resize(ref arr, cf);
foreach (string s in arr)
{
Console.WriteLine(s);
}

//y now contain sets of (word, lemma, left, top, page, block, tag) 
Console.WriteLine("Successful: " + cs + " Exceptions:" + cf);
Console.WriteLine("-------------------------------------------------");

}
*/
