using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Runtime.InteropServices;
using System.Text.RegularExpressions;
using LemmaSharp.Classes;
using edu.stanford.nlp.tagger.maxent;
using System.Drawing.Imaging;

/// <summary>
/// uncomment out lines with four slashes (////) for full functionality (Image generation from pdf, OCR, saving output TSV to disk)
/// </summary>
namespace Zemeris
{
    class Program
    {
        const int limit = 10;

        static void Main(string[] args)
        {
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
                List<string[]> proper = t.Parse(fileName,false);  //parse it
                
                //page selection -> to move to new class
                //pages = proper [i] [4]
                int c = 0, lastPageNum = 1;
                List<int> myarr = new List<int>();
                Dictionary<int, int> md = new Dictionary<int, int>();
                string[] lastItem = proper[proper.Count - 1];
                foreach (var a in proper)
                {
                    //Console.WriteLine(a[3] + ": " + a[0]);
                    if(Int32.Parse(a[3]) == lastPageNum)
                    {
                        c++;
                        if (a == lastItem)
                        {
                            myarr.Add(c);
                            md.Add(lastPageNum, c);
                        }
                    }
                    else
                    {
                        myarr.Add(c);
                        md.Add(lastPageNum, c);
                        c = 0;
                        lastPageNum = Int32.Parse(a[3]);
                    }
                }
                int thispage = 1;
                foreach (int xiint in myarr)
                {
                    Console.WriteLine("Page "+thispage + ": " +xiint);
                    thispage++;
                }

                int fourty = (int)Math.Floor((double)md.Count * 40 / 100);
                if (fourty == 0) fourty = 1;

                var orderedMD = from pair in md orderby pair.Value ascending select pair;

                int meanMin = 0, meanMax = 0;
                for(int ix = 0; ix<fourty; ix++)
                {
                    meanMin += orderedMD.ElementAt(ix).Value;
                    meanMax += orderedMD.ElementAt(orderedMD.Count()-1 - ix).Value;

                }

                meanMin /= fourty;
                meanMax /= fourty;
                List<int> pagesToInclude = new List<int>();
                pagesToInclude.Add(1);
                foreach(KeyValuePair<int,int> kvp in md)
                {
                    int distanceFromMin = Math.Abs(kvp.Value - meanMin);
                    int distanceFromMax = Math.Abs(kvp.Value - meanMax);

                    if(distanceFromMax < distanceFromMin  &&  kvp.Key != 1)
                    {
                        //page to be included
                        pagesToInclude.Add(kvp.Key);
                    }
                }

                var apl = t.Parse(fileName, true);
                var proper2 = from stringArr in apl where (pagesToInclude.Contains(Int32.Parse(stringArr[5]))) select stringArr;

                proper = new List<string[]>();
                proper.AddRange(proper2);
                //proper = proper2.ToList();  //now contains list of valids with empties
                //an array containing word, left, top, width, height, page, conf
                //proper to be paragraph processed

                //now for segmentation into paragraphs

                /*
                 Check for "" / -1
                 Check left for indentation

                 */

                /*
                 New page detection
                 */

                ParaProc papr = new ParaProc();
                SentDetection sentDetect = new SentDetection();

                List<string> paragraphs = papr.GenerateParagraphs(proper, limit);
                Dictionary<String, List<Tuple<int,int>>> wordOccurances = new Dictionary<string, List<Tuple<int, int>>>();

                for (int parNo = 0; parNo < paragraphs.Count; parNo++)
                {
                    Console.WriteLine("Paragraph " + parNo + ": ");
                    Console.WriteLine(paragraphs.ElementAt(parNo));
                    Console.WriteLine("");
                    List<List<string>> sentences = sentDetect.detectSentences(paragraphs.ElementAt(parNo));

                    for (int sentNo = 0; sentNo < sentences.Count; sentNo++)
                    {
                        List<string> sentTokens = sentences.ElementAt(sentNo);
                        Console.WriteLine("Sentence " + sentNo);
                        foreach(string tempTok in sentTokens)
                        {
                            Console.Write(tempTok + " - ");
                        }
                        Console.WriteLine("");

                        foreach(string tok in sentTokens)
                        {
                            string processedTok = SentDetection.postProcessWord(tok);

                            if (!wordOccurances.ContainsKey(processedTok)) wordOccurances[processedTok] = new List<Tuple<int, int>>();

                            wordOccurances[processedTok].Add(new Tuple<int, int>(parNo, sentNo));

                        }
                    }
                }

                foreach(KeyValuePair<String, List<Tuple<int,int>>> pair in wordOccurances)
                {
                    Console.WriteLine("Word: "+ pair.Key + ":");
                    foreach (Tuple<int, int> list in pair.Value)
                    {
                        Console.WriteLine("Paragraph: " + list.Item1 + " Sentence: " + list.Item2);
                    }
                }

                foreach (string par in paragraphs)
                {
                    Console.WriteLine("----------------------------------------------------------------");
                    Console.WriteLine(par);
                }


                /*
                Console.WriteLine("--------------------------------------------------");
                Console.WriteLine("Number of elements in the list (before filtering): " + proper.Count);
                //Console.WriteLine(og);

                //filter the list
                List<string[]> x = i.RemoveGibberish(i.RemoveStopWords(proper)); //now a list with no stop words and no gibberish

                Console.WriteLine("Number of elements in the list (after filtering): " + x.Count);
                Console.WriteLine("-------------------------------------------------");
                */
                //DOM > Parsed DOM > List of paras > 
                Console.WriteLine("Initializing POS Tagging ...");
                Tagger xa = new Tagger();
                xa.Tag(/**/);  //pass sentence, returns tagged
                Console.WriteLine("Done!");
                Console.WriteLine("-------------------------------------------------");
                Console.WriteLine("Removing stopwords ...");

                Console.WriteLine("Done!");
                Console.WriteLine("-------------------------------------------------");
                Console.WriteLine("Lemmatizing ...");

                Console.WriteLine("Done!");
                Console.WriteLine("-------------------------------------------------");

                List<List<string>> y = new List<List<string>>();    //list of lists

                //now lemmatize and POS Tag
                // LEMMA WAS HERE

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

                ////
                //display contents of each block in each page
                /*
                foreach(List<List<string>> page in pbw)
                {
                    foreach (List<string> block in page)
                    {
                        string blockString = String.Join(" ", block);
                        if(blockString != "")
                        {
                            Console.WriteLine("Page " + pbw.IndexOf(page) + " Block " + page.IndexOf(block) + ":" + blockString);
                        }
                    }
                }
                */
                ////

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


//Console.WriteLine();
/* THIS WAS BEING USED
if (newPar)     //if starting new paragraph
{
    curMargin = Int32.Parse(proper[b][1]);
    newPar = false;
}
else if (!(curMargin + limit > Int32.Parse(proper[b][1])) || !(curMargin - limit < Int32.Parse(proper[b][1])))    //if not same indentation
{
    if (curMargin < Int32.Parse(proper[b][1]) - limit)  //if indented inward (right)
    {
        paragraphs.Add(strcur.ToString());
        newPar = true;
        strcur = new StringBuilder();
        //while indentation not in range

    }
    else if (curMargin > Int32.Parse(proper[b][1]) + limit)  //if indented outward (left)
    {
        paragraphs.Add(strcur.ToString());
        newPar = true;
        strcur = new StringBuilder();
        //while indentation not in range

    }
}
*/

/*
else if (curMargin + limit > Int32.Parse(proper[b][1]) && curMargin - limit < Int32.Parse(proper[b][1]))    //if same indentation
{

}
*/



/*
if (Int32.Parse(proper[b-1][6]) != -1)  //if previous is -1, start checking
//else if not -1, add the word to the current par
{
    strcur.Append(proper[b][0]);
}
//do stuff
*/


/*
int alr = papr.LineRight(proper, b);
int anlr = papr.NextLineRight(proper, b);
int all = papr.LineLeft(proper, b);
int anll = papr.NextLineLeft(proper, b);
string anslr = proper[b][0];
*/

//if state unkown check?

// 3 -1s


//TITLE , Sub Title, words, unknown


/*
 * and / as / as if / as long as / at / but / buy / even if / for / from / if / if only / in / into / like / near / now that / nor / of / off / on / on top of / once / onto / or / out of / over / past / so / so that / than / that / till / to / up / upon / with / when / yet
 * */




//-1 counter for next (if more than 3, flush)
/*
int tempRew = b;
while (b < proper.Count && Int32.Parse(proper[b][6]) != -1)     //skip words
{
    b++;
}
while (b < proper.Count && Int32.Parse(proper[b][6]) == -1)     //skip empties
{
    b++;
    empty++;
}
if (empty > 2)
{
    flush = true;
    b = tempRew;
}
empty = 0;
b = tempRew;
*/
