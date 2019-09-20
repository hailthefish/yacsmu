using System;
using System.Collections.Generic;
using System.Text;

namespace yacsmu
{
    internal struct Color
    {

        internal string ParseTokens(string input, bool useANSI)
        {
            // useANSI = whether or not a client wants to recieve ANSI color/style codes
            // This will be useful later when we have telnet negotiation and client options
            if (useANSI)
            {
                //Find all the things that look like tokens and replace them with ANSI codes
            }
            else
            {
                //Find all the things that look like tokens and get rid of them.
            }
        }


        // Tokens for parsing

        internal struct Tokens
        {
            internal const string FG_TOKEN = "&";
            internal const string BG_TOKEN = "^";
            internal const string ST_TOKEN = "%";

            internal struct Style
            {
                internal const string Reset = "X";

                internal const string Swap = "V";
                internal const string SwapOff = "V";

                internal const string Strike = "S";
                internal const string StrikeOff = "s";

                internal const string Italic = "I";
                internal const string ItalicOff = "i";

                internal const string Under = "U";
                internal const string UnderOff = "u";
            }

            internal struct FG
            {
                internal const string Black = "k";
                internal const string DGray = "K";

                internal const string DRed = "r";
                internal const string Red = "R";

                internal const string DGreen = "g";
                internal const string Green = "G";

                internal const string Brown = "y";
                internal const string Yellow = "Y";

                internal const string DBlue = "b";
                internal const string Blue = "B";

                internal const string Purple = "p";
                internal const string Pink = "P";

                internal const string DCyan = "c";
                internal const string Cyan = "C";

                internal const string Gray = "w";
                internal const string White = "W";

                internal const string Default = "d";

                internal const string Random = "?";
            }
            
            internal struct BG
            {
                internal const string Black = "k";

                internal const string DRed = "r";

                internal const string DGreen = "g";

                internal const string Brown = "y";

                internal const string DBlue = "b";

                internal const string Purple = "p";

                internal const string DCyan = "c";

                internal const string Gray = "w";

                internal const string Default = "d";
            }
            
        }



        /*  
         *  \u001b  ANSI escape character
         *  [       goes after the escape character to open a sequence
         *  ;       separates sequence elements
         *  m       ends an ANSI sequence
        */

        internal const string Reset = "\u001b[0m"; // Resets everything to default

        internal const string Swap = "\u001b[7m"; // Inverts foreground and background colors
        internal const string SwapOff = "\u001b[27m"; // Disables inversion

        //Style
        internal struct Style
        {
            internal const string Strike = "\u001b[9m"; // Strikethrough
            internal const string StrikeOff = "\u001b[29m"; // Disables strikethrough

            internal const string Italic = "\u001b[3m"; // Italics or flashing, depending on client
            internal const string ItalicOff = "\u001b[23m";

            internal const string Under = "\u001b[4m"; // Underline
            internal const string UnderOff = "\u001b[24m";
        }
        
        internal struct FG //Foreground colors
        {
            internal const string Black = "\u001b[0;30m";
            internal const string DGray = "\u001b[1;30m";

            internal const string DRed = "\u001b[0;31m";
            internal const string Red = "\u001b[1;31m";

            internal const string DGreen = "\u001b[0;32m";
            internal const string Green = "\u001b[1;32m";

            internal const string Brown = "\u001b[0;33m";
            internal const string Yellow = "\u001b[1;33m";

            internal const string DBlue = "\u001b[0;34m";
            internal const string Blue = "\u001b[1;34m";

            internal const string Purple = "\u001b[0;35m";
            internal const string Pink = "\u001b[1;35m";

            internal const string DCyan = "\u001b[0;36m";
            internal const string Cyan = "\u001b[1;36m";

            internal const string Gray = "\u001b[0;37m";
            internal const string White = "\u001b[1;37m";

            internal const string Default = "\u001b[39m";
        }

        internal struct BG //Background colors
        {
            internal const string Black = "\u001b[40m";

            internal const string DRed = "\u001b[41m";

            internal const string DGreen = "\u001b[42m";

            internal const string Brown = "\u001b[43m";

            internal const string DBlue = "\u001b[44m";

            internal const string Purple = "\u001b[45m";

            internal const string DCyan = "\u001b[46m";

            internal const string Gray = "\u001b[47m";

            internal const string Default = "\u001b[49m";
        }

        internal static readonly List<string> Foreground = new List<string>()
        {
            FG.Black, FG.DGray, FG.DRed, FG.Red, FG.DGreen, FG.Green,
            FG.Brown, FG.Yellow, FG.DBlue, FG.Blue, FG.Purple, FG.Pink,
            FG.DCyan, FG.Cyan, FG.Gray, FG.White, FG.Default
        };

        internal static readonly List<string> Background = new List<string>()
        {
            BG.Black, BG.DRed, BG.DGreen,
            BG.Brown, BG.DBlue, BG.Purple,
            BG.DCyan, BG.Gray, BG.Default
        };

        internal static string RandomFG()
        {
            // -2, +1 so that we don't get black or 'default' because default is boring.
            return Foreground[RandomGen.Roll((byte)(Foreground.Count-2))+1];
        }

        // Hell no I'm not making a random background color function.

    }
}
