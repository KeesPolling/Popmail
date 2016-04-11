using System;
using System.Text;
using System.Threading.Tasks;
using PopMail.EmailProxies.IP_helpers;

namespace PopMail.EmailProxies.EmailInterpreter
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
        public enum EndType
        {
            None = 0,
            EndOfField,
            EndOfHeader
        }
        internal class QuotedStringResult
        {
            internal EndType End { get; set; }
            internal string QuotedStringValue { get; set; }
        }
        internal async Task<QuotedStringResult> ReadQuotedString(BufferedByteReader reader)
        {
            var valueBuilder = new StringBuilder();
            var nextByte = await reader.ReadByte();
            var eol = new Eol();
            var endType = EndType.None;
            while ((endType == EndType.None) && (nextByte != (byte)SpecialByte.Quote))
            {
                switch (nextByte)
                {
                    case (byte)SpecialByte.CarriageReturn:
                        endType = await eol.ProcessEol(reader);
                        break;

                    case (byte)SpecialByte.Backslash:
                        nextByte = await reader.ReadByte();
                        valueBuilder.Append(Convert.ToChar(nextByte));
                        break;
                    default:
                        valueBuilder.Append(Convert.ToChar(nextByte));
                        break;
                }
                if (endType == EndType.None) nextByte = await reader.ReadByte();
            }
            var thisResult = new QuotedStringResult()
            {
                End = endType,
                QuotedStringValue = valueBuilder.ToString()
            };
            return thisResult;
        }
        internal async Task ReadComment(IByteStreamReader reader)
        {
            var nextByte = await reader.ReadByte();
            while (nextByte != (byte)SpecialByte.RightParernthesis)
            {
                if (nextByte == (byte)SpecialByte.Backslash)
                {
                    nextByte = await reader.ReadByte();
                }
                else
                {
                    if (nextByte == (byte)SpecialByte.LeftParenthesis)
                    {
                        await ReadComment(reader);
                    }
                }
                nextByte = await reader.ReadByte();
            }
        }
    }
}

