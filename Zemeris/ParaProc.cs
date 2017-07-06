using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Zemeris
{
    class ParaProc
    {
        public int NextLine(List<string[]> proper, int currentEOL)
        {
            currentEOL++;
            while(currentEOL < proper.Count && Int32.Parse(proper[currentEOL][6]) == -1)
            {
                //wait it out
                currentEOL++;
            }

            return currentEOL;  //returns next line left word
        }

        public int PreviousLine(List<string[]> proper, int currentEOL)  //given any word in a line
        {
            while (currentEOL < proper.Count && Int32.Parse(proper[currentEOL][6]) != -1)   //goes to the first -1 before the line
            {
                currentEOL--;
            }
            while (currentEOL < proper.Count && Int32.Parse(proper[currentEOL][6]) == -1)   //cycles to the last word of previous line
            {
                currentEOL--;
            }

            return currentEOL;  //returns next line left word
        }

        public int ThisLineLast(List<string[]> proper, int currentEOL)
        {
            while (currentEOL < proper.Count && Int32.Parse(proper[currentEOL][6]) != -1)     //while words and not eol reached
            {
                currentEOL++;
            }
            currentEOL--; //now last word

            return currentEOL;
        }

        public int LineLeft(List<string[]> proper, int currentEOL)  //gets first word left position
        {
            bool moved = false;
            while(currentEOL<proper.Count && currentEOL > 0 && Int32.Parse(proper[currentEOL][6]) != -1)
            {
                currentEOL--;   //backtracks to the first -1 found
                moved = true;
            }
            if (moved) currentEOL++;   //and now goes one forward to the first word in the line
            else if (currentEOL >= proper.Count)
            {
                return LineLeft(proper,proper.Count-1);
            }
            return Int32.Parse(proper[currentEOL][1]);
        }

        public int NextLineLeft(List<string[]> proper, int currentEOL)  //gets next line left
        {
            int nll = LineLeft(proper,NextLine(proper, ThisLineLast(proper,currentEOL)));

            return nll;
        }

        public int LineRight(List<string[]> proper, int currentEOL)
        {
            return Int32.Parse(proper[ThisLineLast(proper, currentEOL)][3]) + Int32.Parse(proper[ThisLineLast(proper, currentEOL)][1]); //width + left = right axis
        }

        public int NextLineRight(List<string[]> proper, int currentEOL)
        {
            currentEOL = ThisLineLast(proper,NextLine(proper, currentEOL));          //now end of next line

            return LineRight(proper,currentEOL);
        }


        public bool NLL(List<string[]> proper, int b, int limit)
        {
            return (LineLeft(proper, b) < NextLineLeft(proper, b) + limit) && (LineLeft(proper, b) > NextLineLeft(proper, b) - limit);
        }

        public bool NLR(List<string[]> proper, int b, int limit)
        {
            return (LineRight(proper, b) < NextLineRight(proper, b) + limit) && (LineRight(proper, b) > NextLineRight(proper, b) - limit);
        }

        public bool CheckLeft(List<string[]> proper, int b1, int b2, int limit)
        {
            return (LineLeft(proper, b1) < LineLeft(proper, b2) + limit) && (LineLeft(proper, b1) > LineLeft(proper, b2) - limit);
        }

        public List<string> GenerateParagraphs(List<string[]> proper, int limit)
        {
            List<string> paragraphs = new List<string>();
            StringBuilder strcur = new StringBuilder();     //current par
            int listMarg = 0, curPage = 1, margLineNum = 0;
            bool dashDetect = false, flush = false, newParNextLine = false, dashDone = false, listDetect = false;
            
            //if last is ':' get next line's left, next next line's left (lookahead - make sure not same) to get indentation levels

            //if same indentation levels add line

            // ; or . add para
            //if punc mark and next line not same right level

            //if not same right level, flush to para
            //if curr line less right level than next line flush to para ---------------

            //keep checking deeper for same indentation until same found, once same found get the first line with the different indentation and generate a paragraph

            for (int b = 0; b < proper.Count; b++)            //cycle through proper
            {
                if (Int32.Parse(proper[b][6]) == -1)         //if -1, check next line to see if same indentation
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
                        if (NLR(proper, b, limit) && NLL(proper, b, limit))    //if same right and left indentation
                        {
                            //          | |
                            flush = false;  //since no line indentation change means that should be in same paragraph
                        }
                        else if ((LineRight(proper, b) - limit > NextLineRight(proper, b))  //if same left indentation but next line is shorter than this line
                            && NLL(proper, b, limit))
                        {
                            //          | /
                            newParNextLine = true;  //as next line is end of para
                        }
                        else if (NextLineRight(proper, b) - limit > LineRight(proper, b))  //if next line ends more to the right
                        {   //if this line is shorter than next, also next para
                            //this means end of current par so flush to para
                            //          | \
                            flush = true;
                        }
                        else if (NextLineLeft(proper, b) - limit > LineLeft(proper, b))     //if next line is indented
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
                    if (strcur.Length > 1 && b < proper.Count - 1 && proper[b][0].Length > 0 && (Int32.Parse(proper[b + 1][6]) == -1 || strcur.ToString()[strcur.ToString().Length - 2] == '-'))    //here
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
                            listMarg = LineLeft(proper, b);
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
                        //if (CheckLeft(proper, NextLine(proper, b), margLineNum, limit)) //CheckLeft(proper, b, margLineNum, limit) ||
                        if (CheckLeft(proper, NextLine(proper, b), margLineNum, limit))   //AND HERE
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

                    if (strcur.ToString().Contains("same becomes better understood"))
                    {
                        //just a general search for testing purposes
                    }
                }
            }

            return paragraphs;
        }

    }
}


/*
public int CaseChecker(List<string[]> proper, int b)    //title, subtitle, word, unknown
{
    return 1;
}
*/