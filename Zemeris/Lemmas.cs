using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Zemeris
{
    class Lemmas
    {
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



    }
}
