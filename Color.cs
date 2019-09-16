using System;
using System.Collections.Generic;
using System.Text;

namespace yacsmu
{
    internal struct Color
    {
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
            internal const string Black = "\u001b[0;40m";
            internal const string DGray = "\u001b[1;40m";

            internal const string DRed = "\u001b[0;41m";
            internal const string Red = "\u001b[1;41m";

            internal const string DGreen = "\u001b[0;42m";
            internal const string Green = "\u001b[1;42m";

            internal const string Brown = "\u001b[0;43m";
            internal const string Yellow = "\u001b[1;43m";

            internal const string DBlue = "\u001b[0;44m";
            internal const string Blue = "\u001b[1;44m";

            internal const string Purple = "\u001b[0;45m";
            internal const string Pink = "\u001b[1;45m";

            internal const string DCyan = "\u001b[0;46m";
            internal const string Cyan = "\u001b[1;46m";

            internal const string Gray = "\u001b[0;47m";
            internal const string White = "\u001b[1;47m";

            internal const string Default = "\u001b[49m";
        }

    }
}
