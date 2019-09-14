namespace yacsmu
{
    internal struct Def
    {
        internal const int OUTBUF_SIZE = 2048;

        internal const string NEWLINE = "\r\n";

        //Telnet Negotiation
        internal const byte IAC = 0xFF;
        internal const byte DO = 0xFD;
        internal const byte DONT = 0xFE;
        internal const byte WILL = 0xFB;
        internal const byte WONT = 0xFC;
        internal const byte NOP = 0xF1;
        //Subnegotiation
        internal const byte SB = 0xFA;
        internal const byte SE = 0xF0;
        internal const byte IS = 0x00;
        internal const byte SEND = 0x01;
        //Options
        internal const byte SGA = 0x03; // Suppress Go-ahead
        internal const byte RFC = 0x21; // Remote Flow Control
        internal const byte NAWS = 0x1F; // Negotiate about window size
        internal const byte ECHO = 0x01;
        internal const byte TYPE = 0x18; // Terminal type
        //Misc
        internal const byte GA = 0xF9; //Telnet go-ahead
    }
}
