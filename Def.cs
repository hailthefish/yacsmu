namespace yacsmu
{
    internal struct Def
    {
        internal static string TITLESCREEN = ".\\files\\yacsmu_titlescreen.txt";

        internal const int BUF_SIZE = 2048;
        internal const int MAX_BUFFER = 16384;
        
        internal const int STREAM_TIMEOUT = 250; // milliseconds

        internal const string NEWLINE = "\r\n";
        internal static readonly char[] NEWLINE_CHAR = NEWLINE.ToCharArray();

        //ASCII
        internal const byte BEL = 0x07;

        //Telnet Negotiation
        internal const byte IAC = 0xFF;
        internal const byte DO = 0xFD;
        internal const byte DONT = 0xFE;
        internal const byte WILL = 0xFB;
        internal const byte WONT = 0xFC;
        internal const byte NOP = 0xF1;
        //Subnegotiation
        internal const byte SB = 0xFA; //Subnegotiation option start
        internal const byte SE = 0xF0; //Subnegotiation option end
        internal const byte IS = 0x00;
        internal const byte SEND = 0x01;
        //Options
        internal const byte SGA = 0x03; // Suppress Go-ahead
        internal const byte RFC = 0x21; // Remote Flow Control
        internal const byte NAWS = 0x1F; // Negotiate about window size
        internal const byte ECHO = 0x01;
        internal const byte TTYPE = 0x18; // Terminal type
        //Misc
        internal const byte GA = 0xF9; //Telnet go-ahead
    }
}
