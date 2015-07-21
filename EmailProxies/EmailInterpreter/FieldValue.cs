using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PopMailDemo.EmailProxies.EmailInterpreter
{
    public class  FieldValue
    {
        internal enum SpecialByte : byte
        {
            Linefeed = 10,
            CarriageReturn = 13,
            Space = 32,
            Quote = 34, // "
            LeftParenthesis = 40, // (
            RightParernthesis = 41, // )
            Comma = 44,  // ,
            Colon = 58, // :
            SemiColon = 59, //;
            LeftAngledBracket = 60,  // <
            RightAngledBracket = 62, // >
            Backslash = 92 // \
        }
        internal enum EndType
        {
            None = 0,
            EndOfField,
            EndOfHeader
        }
        internal class EOLResult
        {
            internal bool End { get; set; }
            internal byte NextByte { get; set; }
        }
        internal async Task<string> ReadQuotedString(IByteStreamReader Reader)
        {
            var valueBuilder = new StringBuilder();
            var nextByte = await Reader.ReadByte();
            var eol = new EOL();
            while (!((eol.End) || (nextByte == (byte)SpecialByte.Quote)))
            {
                if (nextByte == (byte)SpecialByte.CarriageReturn)
                {
                    nextByte = await eol.ProcessEOL(Reader);
                    continue;
                }
                if (nextByte == (byte)SpecialByte.Backslash)
                {
                    nextByte = await Reader.ReadByte();
                    valueBuilder.Append(Convert.ToChar(nextByte));
                }
                else
                {
                    valueBuilder.Append(Convert.ToChar(nextByte));
                }
                nextByte = await Reader.ReadByte();
            }
            return valueBuilder.ToString();
        }
        internal async Task ReadComment(IByteStreamReader Reader)
        {
            var nextByte = await Reader.ReadByte();
            while (nextByte != (byte)SpecialByte.RightParernthesis)
            {
                if (nextByte == (byte)SpecialByte.Backslash)
                {
                    nextByte = await Reader.ReadByte();
                }
                else
                {
                    if (nextByte == (byte)SpecialByte.LeftParenthesis)
                    {
                        await ReadComment(Reader);
                    }
                }
                nextByte = await Reader.ReadByte();
            }
        }
    }
}

