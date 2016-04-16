using System;
using System.Text;
using System.Threading.Tasks;
using Windows.ApplicationModel.Resources;
using PopMail.EmailProxies.IP_helpers;

namespace PopMail.EmailProxies.EmailInterpreter
{
    public abstract class  FieldValue
    {
        internal enum PreviousMimeQuoted
        {
            MimeQuoted = 1,
            PreviousMimeQuoted,
            NotMime
        }
        internal enum SpecialByte : byte
        {
            Linefeed = 10,
            CarriageReturn = 13,
            Space = 32,
            Quote = 34,  // "
            LeftParenthesis = 40,  // (
            RightParernthesis = 41,  // )
            Comma = 44,  // ,
            Point = 46,  // .
            Slash = 47,  // /
            Colon = 58,  // :
            SemiColon = 59, //;
            LeftAngledBracket = 60,  // <
            Equals = 61, // =
            RightAngledBracket = 62,  // >
            QuestionMark = 63,  // ?
            At = 64,  // @
            LeftSquareBracket = 91,  // [
            BackSlash = 92,  // \
            RightSquareBracket = 93,  // ]
            Underscore = 95  // _
        }
        public enum EndType
        {
            None = 0,
            EndOfField,
            EndOfHeader
        }
        internal  PreviousMimeQuoted MimeState { get; set; }

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

                    case (byte)SpecialByte.BackSlash:
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
                if (nextByte == (byte)SpecialByte.BackSlash)
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

        internal async Task<string> MimeQuotedString(BufferedByteReader reader)
        {
            var resultString = "";
            var valueBuilder = new StringBuilder();
            var nextByte = await reader.ReadByteAhead();
            if (nextByte != (byte) SpecialByte.QuestionMark)
            { // not Mime encoded word....
                return "=";
            }
            // charset
            nextByte = await reader.ReadByteAhead();
            while (valueBuilder.Length < 50 && MimeCheckByte(nextByte))
            {
                valueBuilder.Append(Convert.ToChar(nextByte));
                nextByte = await reader.ReadByteAhead();
            }
            if (nextByte != (byte) SpecialByte.QuestionMark)
            {
                return "=";
            }
            var charSet = valueBuilder.ToString().ToLower();
            Encoding  charset;
            try
            {
                charset = Encoding.GetEncoding(charSet);
            }
            catch (Exception)
            {
                
                charset = null;
            }
            if (charset == null)
            {
                try
                {
                    var res = new ResourceLoader("EmailProxies/Resources");
                    charSet = res.GetString(charSet);
                    charset = Encoding.GetEncoding(charSet);
                }
                catch (Exception)
                {
                    var res = new ResourceLoader("EmailProxies/Resources");
                    charSet = res.GetString("us-ascii");
                    charset = Encoding.GetEncoding(charSet);
                }
            } 
            // encoding
            nextByte = await reader.ReadByteAhead();
            var encoding = nextByte;
            // B = ascii 66
            if (encoding == 66) resultString = await WordFromBase64(reader, charset);
            // Q  = ascii 81
            if (encoding == 81) resultString = await WordFromQuotedPrintable(reader, charset);
            if (resultString != "=")
            {
                reader.Clear();
            }
            return resultString;
        }

        private bool MimeCheckByte(byte byteToCheck)
        {
            switch (byteToCheck)
            {
                case (byte) SpecialByte.Space:
                case (byte) SpecialByte.LeftParenthesis:
                case (byte) SpecialByte.RightParernthesis:
                case (byte) SpecialByte.LeftAngledBracket:
                case (byte) SpecialByte.RightAngledBracket:
                case (byte) SpecialByte.At:
                case (byte) SpecialByte.Comma:
                case (byte) SpecialByte.SemiColon:
                case (byte) SpecialByte.Colon:
                case (byte) SpecialByte.CarriageReturn:
                case (byte) SpecialByte.QuestionMark:
                    return false;
                 default:
                    return true;
            }
        }

        private async Task<string> WordFromBase64(BufferedByteReader reader, Encoding charset)
        {
            string decoded;
            var nextByte = await reader.ReadByteAhead();
            if (nextByte != (byte) SpecialByte.QuestionMark) return "=";
            nextByte = await reader.ReadByteAhead();
            var valueBuilder = new StringBuilder();
            while (valueBuilder.Length < 800 && nextByte != (byte)SpecialByte.QuestionMark)
            {
                valueBuilder.Append(Convert.ToChar(nextByte));
                nextByte = await reader.ReadByteAhead();
            }
            if (nextByte != (byte) SpecialByte.QuestionMark) return "=";
            nextByte = await reader.ReadByteAhead();
            if (nextByte != (byte)SpecialByte.Equals) return "=";
            var base64String = valueBuilder.ToString();
            try
            {
                var base64DecodedBytes = Convert.FromBase64String(base64String);
               decoded = charset.GetString(base64DecodedBytes);

            }
            catch (Exception)
            {
                return "=";
            }
            return decoded;
        }

        private async Task<string> WordFromQuotedPrintable(BufferedByteReader reader, Encoding charset)
        {
 
            var nextByte = await reader.ReadByteAhead();
            if (nextByte != (byte)SpecialByte.QuestionMark) return "=";
            nextByte = await reader.ReadByteAhead();
            var valueBuilder = new StringBuilder();
            while (valueBuilder.Length < 100  && nextByte != (byte)SpecialByte.QuestionMark)
            {
                if (nextByte == (byte) SpecialByte.Equals)
                {
                    var numbers = new byte[1];
                    nextByte = await reader.ReadByteAhead();
                    numbers[0] = ConvertHexToNumber(nextByte);
                    numbers[0] = (byte) (numbers[0]*16);
                    nextByte = await reader.ReadByteAhead();
                    numbers[0] = (byte) (numbers[0] + ConvertHexToNumber(nextByte));
                    valueBuilder.Append(charset.GetString(numbers));
                }
                else
                {
                    if (nextByte == (byte) SpecialByte.Underscore)
                    {
                        valueBuilder.Append(' ');
                    }
                    else
                    {
                        if (!MimeCheckByte(nextByte)) return "=";

                        valueBuilder.Append(Convert.ToChar(nextByte));
                    }
                }
                nextByte = await reader.ReadByteAhead();
            }
            if (nextByte != (byte)SpecialByte.QuestionMark) return "=";
            nextByte = await reader.ReadByteAhead();
            if (nextByte != (byte)SpecialByte.Equals) return "=";
            reader.Clear();
            return valueBuilder.ToString();
        }

        private byte ConvertHexToNumber(byte nextByte)
        {
            byte number = 0;
            if (nextByte >= 48 && nextByte <= 57) // nummers 0 - 9
            {
                number = (byte)(nextByte - 48);
            }
            else
            {
                number = (byte)(nextByte - 55); // A = ascii 65 => 10
            }
            return number;
        }

        internal abstract Task<EndType> ReadFieldValue(BufferedByteReader reader);
    }
}

