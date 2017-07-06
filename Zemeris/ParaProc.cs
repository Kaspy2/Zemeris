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


    }
}
