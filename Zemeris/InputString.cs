using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Zemeris
{

    class InputString
    {
        private int pc;
        private List<Process> plist = new List<Process>();

        public List<string[]> RemoveStopWords(List <string[]> input) //input words
        {
            List<string> words = System.IO.File.ReadAllText("stopwords.txt").Replace("\r", "").Split('\n').ToList();  //list of all stop words

            List<string[]> temp = new List<string[]>();

            foreach (string[] x in input)
            {
                //Console.WriteLine("Processing " + x.ToLower());
                //check to see if each word is in stop words and if it is remove it
                if ( !words.Contains( x[0].ToLower() ) )
                {
                    temp.Add(x);
                }
            }

            return temp;
        }

        public List<string[]> RemoveGibberish (List<string[]> input)
        {
            List<string> words = System.IO.File.ReadAllText("dictionary.txt").Replace("\r", "").Split('\n').ToList();  //list of all words in dictionary

            List<string[]> temp = new List<string[]>();

            foreach (string[] x in input)
            {
                //Console.WriteLine("Processing " + x.ToLower());
                //check to see if each word is in stop words and if it is remove it
                if (x[0]!="" && x[0]!=null && words.Contains(x[0].ToLower()))
                {
                    temp.Add(x);
                    //Console.WriteLine("Added " + x);
                }
            }

            return temp;
        }

        public void GenerateOCR()       //takes most time to complete so decided to run 3 versions asynchronously
        {
            const int MAXPROCESSES = 4;
            string inPath = @"./runTessTSV/list.txt";           //input path for imgs dir (model)
            string outPath = @"./runTessTSV/output/myFileName"; //output path for tsv dir
            string arg;

            //for every folder create a list
            //for every item in the folder add to list
            //run for each list

            string ogdir = Directory.GetCurrentDirectory();

            Directory.SetCurrentDirectory(@"Tesseract\runTessTSV\images");
            string[] folders = Directory.GetDirectories(@".\");  //every folder
            
            foreach (string folderName in folders)
            {
                string[] folderImgs = Directory.GetFiles(folderName);   //every image in that folder
                for (int c = 0; c<folderImgs.Length; c++)
                {
                    folderImgs[c] = @"runTessTSV\images\" + folderImgs[c];
                }
                //add folder runTessTSV\images\
                System.IO.File.WriteAllLines(@"..\"+folderName+".txt", folderImgs);    //append to text file

            }

            Directory.SetCurrentDirectory(ogdir);
            //Console.WriteLine(ogdir);

            pc = 0;
            string list;
            foreach (string lista in folders)
            {
                list = lista.Remove(0, 2);
                inPath = @".\runTessTSV\"+list+".txt";          //input path for imgs dir
                outPath = @".\runTessTSV\output\"+list;         //output path for tsv dir
                arg = inPath + " -psm 12 " + outPath + " tsv";
                //Console.WriteLine(Directory.GetCurrentDirectory());
                //Console.WriteLine(arg);

                if (plist.Count < MAXPROCESSES)
                {
                    Process p = new Process();
                    p.Exited += UpdatePC;
                    p.Disposed += UpdatePC;
                    p.ErrorDataReceived += UpdatePC;
                    p.OutputDataReceived += UpdatePC;
                    p.StartInfo.FileName = "runItImproved.bat ";
                    p.StartInfo.Arguments = arg;
                    plist.Add(p);
                    p.Start();

                    pc++;
                }
                else
                {
                    while (plist.Count >= MAXPROCESSES) //should never be > but just in case
                    {
                        //wait and ping the process list every second to check if a process still exists
                        foreach (Process a in plist)
                        {
                            if (a.HasExited)
                            {
                                plist.Remove(a); break;
                            }
                        }
                        System.Threading.Thread.Sleep(1000);
                    }
                    Process p = new Process();
                    p.Exited += UpdatePC;
                    p.Disposed += UpdatePC;
                    p.ErrorDataReceived += UpdatePC;
                    p.OutputDataReceived += UpdatePC;
                    p.StartInfo.FileName = "runItImproved.bat ";
                    p.StartInfo.Arguments = arg;
                    plist.Add(p);
                    p.Start();

                    //pc++;
                }

            }
            while (plist.Count > 0) //while a process is still running
            {
                foreach (Process a in plist)
                {
                    if (a.HasExited)
                    {
                        plist.Remove(a); break;
                    }
                }
                System.Threading.Thread.Sleep(1000);    //no need to keep running the while so sleep and ping every second
            }
        }

        private void UpdatePC(object sender, EventArgs e)   //technically may not be needed
        {
            foreach(Process a in plist)
            {
                if (a.HasExited)
                {
                    plist.Remove(a); break;
                }
            }
            pc--;
        }

        public void GenerateImgs()
        {
            //first get the input PDFs from the folder

            string[] pdFiles = Directory.GetFiles(@"PDFs");

            int size = 450, dpi = 600; //size and dpi of the output page - dpi not affective

            string ogdir = Directory.GetCurrentDirectory();
            //Console.WriteLine(ogdir);

            foreach (string individualPDF in pdFiles)
            {
                string iName = individualPDF.Substring(individualPDF.LastIndexOf('\\') + 1);
                string imageName = iName.Substring(0, iName.LastIndexOf(".pdf"));

                if (!System.IO.Directory.Exists(@"Tesseract\runTessTSV\images\" + imageName))                 //if directory doesn't exist
                {   //file with that name was not proccessed before
                    //so create a new dir
                    System.IO.Directory.CreateDirectory(@"Tesseract\runTessTSV\images\" + imageName);
                    Console.WriteLine("Proccessing " + iName);

                    try
                    {
                        using (var document = PdfiumViewer.PdfDocument.Load(individualPDF))
                        {
                            try
                            {
                                Directory.SetCurrentDirectory(@"Tesseract\runTessTSV\images\" + imageName);
                            }
                            catch (Exception ex)
                            {
                                Console.WriteLine("Error changing directory\n" + ex);
                            }

                            //document.PageCount is an n-digit number - we need all imageNames to be n-digit numbers
                            int count = 0, n = document.PageCount;
                            
                            while (n != 0)
                            {
                                n /= 10;
                                count++;
                            }
                            

                            for (int cp = 0; cp < document.PageCount; cp++)
                            {

                                var image = document.Render(cp,                             //page num
                                    (int)document.PageSizes[cp].Width / 72 * size,          //size x
                                    (int)document.PageSizes[cp].Height / 72 * size,         //size y
                                    dpi,                                                    //dpi x
                                    dpi,                                                    //dpi y
                                    true);                                                  //to output

                                

                                image.Save(cp.ToString().PadLeft(count,'0') + ".tiff", ImageFormat.Tiff);
                                image.Dispose();
                            }


                        }
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine("Exception during PDF conversion!\n" + ex);
                        Console.WriteLine("\nImageName: " + imageName);
                        // handle exception here;
                    }
                    finally
                    {
                        Directory.SetCurrentDirectory(ogdir);
                    }
                }
                else
                {
                    Console.WriteLine("Folder "+imageName+" already exists - rename or delete to process this file!");
                }
            }
        }

    }
}