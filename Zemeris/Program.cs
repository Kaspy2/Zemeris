using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Runtime.InteropServices;
using System.Text.RegularExpressions;
using LemmaSharp.Classes;
using Test.Classes;
using edu.stanford.nlp.tagger.maxent;
using System.Drawing.Imaging;

/// <summary>
/// uncomment out lines with four slashes (////) for full functionality (Image generation from pdf, OCR, saving output TSV to disk)
/// </summary>
namespace Zemeris
{
    class Program
    {
        static void Main(string[] args)
        { 
            // Loading POS Tagger
            var tagger = new MaxentTagger(@"spostag\models\wsj-0-18-bidirectional-nodistsim.tagger");
            //a maximum entropy tagger
            //left 7, top 8, width 9, height 10 (-1 for proper index)

            InputString i = new InputString();  //create a new instance on the object InputString 
            //to be used for basic processing of the PDFs into imgs and imgs into text

            Console.WriteLine("Generating Images ...");
            ////i.GenerateImgs();         //generate images from PDFs
            Console.WriteLine("Done!");
            Console.WriteLine("Generating OCR ...");
            ////i.GenerateOCR();          //call tesseract to generate text from the images
            Console.WriteLine("Done");

            //now output TSVs from Tesseract OCR are in the output directory and we can start processing the output
            TSVParser t = new TSVParser();

            foreach (string fileName in Directory.GetFiles(@"Tesseract\runTessTSV\output")) //for each TSV file in the output folder
            {
                List<string[]> proper = t.Parse(fileName);  //parse it
                Console.WriteLine("--------------------------------------------------");
                Console.WriteLine("Number of elements in the list (before filtering): " + proper.Count);
                //Console.WriteLine(og);

                //decorate the console with a loading spinner
                Spinner turner = new Spinner(Console.CursorLeft,Console.CursorTop);
                turner.Start();

                //filter the list
                List<string[]> x = i.RemoveGibberish(i.RemoveStopWords(proper)); //now a list with no stop words and no gibberish

                turner.Stop();

                Console.WriteLine("Number of elements in the list (after filtering): " + x.Count);
                Console.WriteLine("-------------------------------------------------");

                List<List<string>> y = new List<List<string>>();    //list of lists

                //now lemmatize and POS Tag
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
                        tempList.Add(currStr[1]);   //x
                        tempList.Add(currStr[2]);   //y
                        tempList.Add(currStr[3]);   //page
                        tempList.Add(currStr[4]);   //block

                        //now POS Tagging
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

                List<List<List<string>>> pbw = new List<List<List<string>>>();  //pages > blocks > words

                string AFile = fileName.Remove(0,fileName.LastIndexOf('\\')+1);

                using (StreamWriter file = new StreamWriter(@"./OutputTSV/" + AFile, true))
                {

                    foreach (List<string> currElem in y)
                    {
                        ////Console.WriteLine("Word: " + currElem[0] + "\tLemma: " + currElem[1] + "\tLeft(x): " + currElem[2] + "\tTop(y): " + currElem[3] + "\tPOSTag: " + currElem[6]);
                        ////file.WriteLine(currElem[0] + "\t" + currElem[1] + "\t" + currElem[2] + "\t" + currElem[3] + "\t" + currElem[6]);
                        //dumps word, lemma, x, y, POS in console and tsv file

                        //now group words by same page and same block
                        int page = Convert.ToInt32(currElem[4]);
                        int block = Convert.ToInt32(currElem[5]);

                        while (pbw.Count <= page)
                        {
                            List<List<string>> temp = new List<List<string>>();
                            pbw.Add(temp);
                        }
                        while (pbw[page].Count <= block)
                        {
                            List<string> temp = new List<string>();
                            pbw[page].Add(temp);
                        }
                        pbw[page][block].Add(currElem[0]);          //add the word to the relevant page and block

                    }
                }

                //display contents of each block in each page
                foreach(List<List<string>> page in pbw)
                {
                    foreach (List<string> block in page)
                    {
                        string blockString = String.Join(" ", block);
                        Console.WriteLine("Page "+pbw.IndexOf(page)+" Block "+page.IndexOf(block)+":"+blockString);
                    }
                }

            }   //end of foreach document

            Console.WriteLine("Done!");
            Console.ReadLine();

        }
    }
}


//discarded code

//var tagger = new MaxentTagger("taggers / left3words - distsim - wsj - 0 - 18.tagger");
/*
// add examples
var examples = new List<Tuple<string, string>>()
{
    new Tuple<string,string>("'ll", "will"),
    new Tuple<string, string>("having","should")
};
foreach (var example in examples)
{
    var lemma = lemmatizer.Lemmatize(example.Item1);
    //Console.WriteLine("{0} --> {1} {2}", example.Item1, lemma, lemma != example.Item2 ? ("!= " + example.Item2):"");
    Console.WriteLine(example.Item1 + " is reduced to " + lemma);
}
*/
/* the not working version
Process process = new Process();
process.StartInfo.WorkingDirectory = "C:\\\\Users\\gabrielf\\Desktop\\Zemeris\\Zemeris\\bin\\Debug\\Tesseract\\runTessTSV";
process.StartInfo.FileName = "runIt.bat";
process.StartInfo.UseShellExecute = false;
process.StartInfo.CreateNoWindow = false;
process.StartInfo.RedirectStandardOutput = false;
process.Start();
*/
//Process.Start(@".\Tesseract\tesseract", arg).WaitForExit();
//Process.Start("runItImproved.bat ", arg).WaitForExit();
//System.Diagnostics.Process.Start("runItImproved.bat ",inPath+ " -psm 3 " + outPath);
// List<string> words = System.IO.File.ReadAllText("dictionaryFULL.txt").Replace("\r", "").Split('\n').ToList();  //list of all words in FULL dictionary
// note - use of full dictionary requires case changes