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
        const int limit = 10;

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
                List<string[]> proper = t.Parse(fileName,false);  //parse it

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

                List<string> paragraphs = new List<string>();

                StringBuilder strcur = new StringBuilder();     //current par
                int listMarg = 0, curPage = 1, margLineNum = 0;
                bool dashDetect = false, flush = false, newParNextLine = false, dashDone = false, listDetect = false ;
                ParaProc papr = new ParaProc();
                //if last is ':' get next line's left, next next line's left (lookahead - make sure not same) to get indentation levels

                //if same indentation levels add line

                // ; or . add para
                //if punc mark and next line not same right level

                //if not same right level, flush to para
                //if curr line less right level than next line flush to para ---------------

                //keep checking deeper for same indentation until same found, once same found get the first line with the different indentation and generate a paragraph

                for (int b = 0; b<proper.Count; b++)            //cycle through proper
                {
                    if(Int32.Parse(proper[b][6]) == -1)         //if -1, check next line to see if same indentation
                    {
                        //bool sameMarg = true;
                        if (Int32.Parse(proper[b][5]) != curPage)   //seperates pages
                        {
                            curPage = Int32.Parse(proper[b][5]);
                            paragraphs.Add(strcur.ToString());
                            strcur = new StringBuilder();
                            flush = true;
                        }

                        while (b < proper.Count && Int32.Parse(proper[b][6]) == -1)     //skip empties
                        {
                            b++;
                        }
                        
                        if (dashDetect)
                        {
                            int rewind = b;
                            dashDetect = false;
                            while (b < proper.Count && Int32.Parse(proper[b][6]) != -1 && Int32.Parse(proper[b][5]) == curPage)     //add the whole line
                            {
                                strcur.Append(proper[b][0] + " ");
                                b++;
                            }
                            dashDone = true;
                            b = rewind;
                            //now at last element in line after dash
                        }

                        if (!flush && !listDetect)
                        {
                            if (papr.NLR(proper,b,limit) && papr.NLL(proper, b, limit))    //if same right and left indentation
                            {
                                //          | |
                                flush = false;  //since no line indentation change means that should be in same paragraph
                            }
                            else if ((papr.LineRight(proper, b) - limit > papr.NextLineRight(proper, b))  //if same left indentation but next line is shorter than this line
                                && papr.NLL(proper,b,limit))
                            {
                                //          | /
                                newParNextLine = true;  //as next line is end of para
                            }
                            else if (papr.NextLineRight(proper, b) - limit > papr.LineRight(proper, b))  //if next line ends more to the right
                            {   //if this line is shorter than next, also next para
                                //this means end of current par so flush to para
                                //          | \
                                flush = true;
                            }
                            else if (papr.NextLineLeft(proper, b) - limit > papr.LineLeft(proper, b))     //if next line is indented
                            {   //next line starts a new para
                                //          \
                                flush = true;
                            }
                        }

                        if (!dashDone)    //if no dashes were processed
                        {
                            while (b < proper.Count && Int32.Parse(proper[b][6]) != -1)     //add the whole line to the current paragraph
                            {
                                strcur.Append(proper[b][0] + " ");
                                b++;
                            }
                            b--;    //last word
                        }
                        else dashDone = false;  //else a dash has been processed


                        //now check if the line just processed ends with a - (for seperated words) or : (for lists)
                        if (strcur.Length > 1 && b < proper.Count - 1 && proper[b][0].Length > 0 && (Int32.Parse(proper[b + 1][6]) == -1 || strcur.ToString()[strcur.ToString().Length-2] == '-'))    //here
                        {
                            if (proper[b][0].ElementAt(proper[b][0].Length - 1) == '-' || strcur.ToString()[strcur.ToString().Length - 2] == '-') //if a dash
                            {
                                dashDetect = true;
                                strcur.Remove(strcur.Length - 2, 2);   //remove "- " so word can be continued
                            }

                            else if (proper[b][0].ElementAt(proper[b][0].Length - 1) == ':' && !listDetect)    //if : initiate a list
                            {
                                listDetect = true;
                                flush = true;
                                listMarg = papr.LineLeft(proper, b);
                                margLineNum = b;
                            }
                        }

                        if (listDetect)
                        {
                            if (strcur.ToString().Contains(';'))    //if in a list and the element contains a semi-colon
                            {
                                flush = true;
                            }
                            if (strcur.ToString().LastIndexOf('.') == strcur.ToString().Length - 2)    //if in a list and the element contains a semi-colon
                            {//not getting to here
                                flush = true;
                                listDetect = false;
                            }
                            //if margin returns to original level in next two lines
                            //if (papr.CheckLeft(proper, papr.NextLine(proper, b), margLineNum, limit)) //papr.CheckLeft(proper, b, margLineNum, limit) ||
                            if (papr.CheckLeft(proper, papr.NextLine(proper, b), margLineNum, limit))   //AND HERE
                            {
                                listDetect = false;
                                flush = true;
                            }
                        }


                        if ((flush && !dashDetect))    //if flushing // || (flush && listDetect)
                        {
                            paragraphs.Add(strcur.ToString());
                            strcur = new StringBuilder();
                            flush = false;
                        }
                        else if (newParNextLine)
                        {
                            newParNextLine = false;
                            flush = true;
                        }

                        if (strcur.ToString().Contains("same becomes better understood")) {
                            //just a general search for testing purposes
                        }
                    }
                }

                foreach(string par in paragraphs)
                {
                    Console.WriteLine("----------------------------------------------------------------");
                    Console.WriteLine(par);
                }

                Console.WriteLine("--------------------------------------------------");
                Console.WriteLine("Number of elements in the list (before filtering): " + proper.Count);
                //Console.WriteLine(og);

                //decorate the console with a loading spinner
                //Spinner turner = new Spinner(Console.CursorLeft,Console.CursorTop);
                //turner.Start();

                //filter the list
                List<string[]> x = i.RemoveGibberish(i.RemoveStopWords(proper)); //now a list with no stop words and no gibberish

                //turner.Stop();

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
