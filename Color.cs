using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;
using System.Linq;
using Serilog;

namespace yacsmu
{
    internal struct Color
    {

        internal static string ParseTokens(string input, bool useANSI)
        {
            // useANSI = whether or not a client wants to recieve ANSI color/style codes
            // This will be useful later when we have telnet negotiation and client options

            if (useANSI)
            {
                string result = Tokens.randomToken.Replace(input, m => RandomFG());
                return Tokens.parseRegex.Replace(result, m =>
                { return Tokens.mapANSI.ContainsKey(Regex.Escape(m.Groups[0].Value)) ? Tokens.mapANSI[Regex.Escape(m.Value)] : m.Value; });
            }
            else
            {
                //Find all the things that look like tokens and get rid of them.
                string result = Tokens.randomToken.Replace(input, string.Empty);
                return Tokens.parseRegex.Replace(result, m => string.Empty);
            }

        }


        // Tokens for parsing

        internal struct Tokens
        {
            internal const string FG_TOKEN = "&";
            internal const string BG_TOKEN = "^";
            internal const string ST_TOKEN = "&";

            internal const string RANDOM_TOKEN = "&?";

            internal static readonly Dictionary<string, string> mapANSI = new Dictionary<string, string>()
            {
                // Styles
                { Regex.Escape(ST_TOKEN + "X") , Style.Reset },
                { Regex.Escape(ST_TOKEN + "V") , Style.Swap },
                { Regex.Escape(ST_TOKEN + "v") , Style.SwapOff },
                { Regex.Escape(ST_TOKEN + "S") , Style.Strike },
                { Regex.Escape(ST_TOKEN + "s") , Style.StrikeOff },
                { Regex.Escape(ST_TOKEN + "I") , Style.Italic },
                { Regex.Escape(ST_TOKEN + "i") , Style.ItalicOff },
                { Regex.Escape(ST_TOKEN + "U") , Style.Under },
                { Regex.Escape(ST_TOKEN + "u") , Style.UnderOff },
                // Backgrounds
                { Regex.Escape(BG_TOKEN + "k") , BG.Black },
                { Regex.Escape(BG_TOKEN + "r") , BG.DRed },
                { Regex.Escape(BG_TOKEN + "g") , BG.DGreen },
                { Regex.Escape(BG_TOKEN + "y") , BG.Brown },
                { Regex.Escape(BG_TOKEN + "b") , BG.DBlue },
                { Regex.Escape(BG_TOKEN + "p") , BG.Purple },
                { Regex.Escape(BG_TOKEN + "c") , BG.DCyan },
                { Regex.Escape(BG_TOKEN + "w") , BG.Gray },
                { Regex.Escape(BG_TOKEN + "d") , BG.Default },
                // Foregrounds
                { Regex.Escape(FG_TOKEN + "k") , FG.Black },
                { Regex.Escape(FG_TOKEN + "K") , FG.DGray },
                { Regex.Escape(FG_TOKEN + "r") , FG.DRed },
                { Regex.Escape(FG_TOKEN + "R") , FG.Red},
                { Regex.Escape(FG_TOKEN + "g") , FG.DGreen },
                { Regex.Escape(FG_TOKEN + "G") , FG.Green },
                { Regex.Escape(FG_TOKEN + "y") , FG.Brown },
                { Regex.Escape(FG_TOKEN + "Y") , FG.Yellow },
                { Regex.Escape(FG_TOKEN + "b") , FG.DBlue },
                { Regex.Escape(FG_TOKEN + "B") , FG.Blue },
                { Regex.Escape(FG_TOKEN + "p") , FG.Purple },
                { Regex.Escape(FG_TOKEN + "P") , FG.Pink },
                { Regex.Escape(FG_TOKEN + "c") , FG.DCyan },
                { Regex.Escape(FG_TOKEN + "C") , FG.Cyan },
                { Regex.Escape(FG_TOKEN + "w") , FG.Gray },
                { Regex.Escape(FG_TOKEN + "W") , FG.White },
                { Regex.Escape(FG_TOKEN + "D") , FG.Default },
                // Misc/Escaping Tokens/etc
                //{ Regex.Escape(ST_TOKEN + ST_TOKEN), ST_TOKEN}, commented out because ST and BG are currently the same
                { Regex.Escape(BG_TOKEN + BG_TOKEN), BG_TOKEN},
                { Regex.Escape(FG_TOKEN + FG_TOKEN), FG_TOKEN},
                { Regex.Escape(FG_TOKEN), string.Empty},
                // { Regex.Escape(ST_TOKEN), string.Empty}, comented out because ST and BG are currently the same
                { Regex.Escape(BG_TOKEN), string.Empty},
                {Regex.Escape(RANDOM_TOKEN), RANDOM_TOKEN }, // We replace this separately
            };

            internal static readonly Regex parseRegex = new Regex(string.Join("|", mapANSI.Keys));
            internal static readonly Regex randomToken = new Regex(Regex.Escape(RANDOM_TOKEN));
        }



        /*  
         *  \u001b  ANSI escape character
         *  [       goes after the escape character to open a sequence
         *  ;       separates sequence elements
         *  m       ends an ANSI sequence
        */

        //Style & General
        internal struct Style
        {
            internal const string Reset = "\u001b[0m"; // Resets everything to default

            internal const string Swap = "\u001b[7m"; // Inverts foreground and background colors
            internal const string SwapOff = "\u001b[27m"; // Disables inversion

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

        internal static readonly string[] Foreground = new string[]
        {
            FG.Black, FG.DGray, FG.DRed, FG.Red, FG.DGreen, FG.Green,
            FG.Brown, FG.Yellow, FG.DBlue, FG.Blue, FG.Purple, FG.Pink,
            FG.DCyan, FG.Cyan, FG.Gray, FG.White, FG.Default
        };

        internal static readonly string[] Background = new string[]
        {
            BG.Black, BG.DRed, BG.DGreen,
            BG.Brown, BG.DBlue, BG.Purple,
            BG.DCyan, BG.Gray, BG.Default
        };

        internal static string RandomFG()
        {
            // -2, +1 so that we don't get black or 'default' because default is boring.
            return Foreground[RandomGen.Roll((byte)(Foreground.Length-2))+1];
        }

        // Hell no I'm not making a random background color function.

    }
}
