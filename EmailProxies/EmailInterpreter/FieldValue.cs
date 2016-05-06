using System;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using Windows.ApplicationModel.Resources;
using PopMail.EmailProxies.IP_helpers;

namespace PopMail.EmailProxies.EmailInterpreter
{
    public abstract class  FieldValue
    {
        #region SpecialByte
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
        #endregion SpecialByte

        #region EndOfLine

        public enum EndType
        {
            None = 0,
            EndOfField,
            EndOfHeader
        }

        private EndType _endType;

        /// <summary>
        /// Processes an end of line to determine wether it is part of a folding white space, 
        ///  an end of field or an end of header.
        ///  assumes a Carriage return (x0D) has just been read from the "reader".
        /// </summary>
        /// <param name="reader">BufferdByteReader to acces the underlying stream</param>
        /// <returns>Enum EndType</returns>
        internal async Task<FieldValue.EndType> ProcessEol(BufferedByteReader reader)
        {
            var rs = await reader.ReadByteAsync();
            if (rs != (byte)FieldValue.SpecialByte.Linefeed)
            {
                throw new FormatException("carriagereturn must be followed by linefeed");
            }
            _endType = FieldValue.EndType.EndOfField; // unless folowed by a space ( a folding white space = FWS)
            rs = await reader.ReadByteAhead();
            if (rs == (byte)FieldValue.SpecialByte.Space)
            {
                rs = await reader.ReadByteAhead();
                while (rs == (byte)FieldValue.SpecialByte.Space)
                {
                    rs = await reader.ReadByteAhead();
                }
                _endType = FieldValue.EndType.None; // unless folowed by a crlf (End of header)
                // leave 1 space and the not space character on the buffer
                if (reader.BufferSize > 2) reader.RemoveFirst(reader.BufferSize - 2);
            }
            // Two crlf's with or without spaces in between signify te end of the Header 
            if (rs != (byte)FieldValue.SpecialByte.CarriageReturn) return _endType;

            reader.Clear();
            rs = await reader.ReadByteAsync();
            if (rs != (byte)FieldValue.SpecialByte.Linefeed)
            {
                throw new FormatException("carriagereturn must be followed by linefeed");
            }

            return FieldValue.EndType.EndOfHeader;
        }
        #endregion EndOfLine

        #region Quoted String
        internal class QuotedStringResult
        {
            internal EndType End { get; set; }
            internal string QuotedStringValue { get; set; }
        }

        /// <summary>
        /// Processes a quoted string
        ///  assumes a quote has just been read from the "reader".
        /// </summary>
        /// <param name="reader">BufferdByteReader to acces the underlying stream</param>
        /// <returns>QuotedStringResult (quoted string value & EbdType</returns>
        internal async Task<QuotedStringResult> ReadQuotedString(BufferedByteReader reader)
        {
            var valueBuilder = new StringBuilder();
            var nextByte = await reader.ReadByteAsync();
            var endType = EndType.None;
            while ((endType == EndType.None) && (nextByte != (byte)SpecialByte.Quote))
            {
                switch (nextByte)
                {
                    case (byte)SpecialByte.CarriageReturn:
                        endType = await ProcessEol(reader);
                        break;

                    case (byte)SpecialByte.BackSlash:
                        nextByte = await reader.ReadByteAsync();
                        valueBuilder.Append(Convert.ToChar(nextByte));
                        break;
                    default:
                        valueBuilder.Append(Convert.ToChar(nextByte));
                        break;
                }
                if (endType == EndType.None) nextByte = await reader.ReadByteAsync();
            }
            var thisResult = new QuotedStringResult()
            {
                End = endType,
                QuotedStringValue = valueBuilder.ToString()
            };
            return thisResult;
        }
        internal async Task<EndType> ReadQuotedString(BufferedByteReader reader, BinaryWriter writer)
        {
            var nextByte = await reader.ReadByteAsync();
            var endType = EndType.None;
            while ((endType == EndType.None) && (nextByte != (byte)SpecialByte.Quote))
            {
                switch (nextByte)
                {
                    case (byte)SpecialByte.CarriageReturn:
                        endType = await ProcessEol(reader);
                        break;

                    case (byte)SpecialByte.BackSlash:
                        nextByte = await reader.ReadByteAsync();
                        writer.Write(nextByte);
                        break;
                    default:
                        writer.Write(nextByte);
                        break;
                }
                if (endType == EndType.None) nextByte = await reader.ReadByteAsync();
            }
            return endType;
        }
        #endregion Quoted String

        #region ReadComment
        internal async Task ReadComment(IByteStreamReader reader)
        {
            var nextByte = await reader.ReadByteAsync();
            while (nextByte != (byte)SpecialByte.RightParernthesis)
            {
                if (nextByte == (byte)SpecialByte.BackSlash)
                {
                    nextByte = await reader.ReadByteAsync();
                }
                else
                {
                    if (nextByte == (byte)SpecialByte.LeftParenthesis)
                    {
                        await ReadComment(reader);
                    }
                }
                nextByte = await reader.ReadByteAsync();
            }
        }
        #endregion ReadComment

        #region MimeQuotedString

        internal enum PreviousMimeQuoted
        {
            MimeQuoted = 1,
            PreviousMimeQuoted,
            NotMime
        }

        internal PreviousMimeQuoted MimeState { get; set; }

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
            catch (Exception ex)
            {
               charset = null;
            }
            if (charset == null)
            {
                try
                {
                    // Provide the base class for an encoding provider, 
                    // which supplies encodings that are unavailable on this particular platform. 
                    Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
                    // Now we can try again.
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

        internal bool MimeCheckByte(byte byteToCheck)
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
                switch (nextByte)
                {
                    case (byte)SpecialByte.Equals:
                        // two byte Hex number ahead
                        var number = new byte[1]; // GetString needs a Byte Array 
                        // First Byte ....
                        nextByte = await reader.ReadByteAhead();
                        number[0] = ConvertHexToNumber(nextByte);
                        
                        number[0] = (byte)(number[0] * 16);
                        // second Byte
                        nextByte = await reader.ReadByteAhead();
                        number[0] = (byte)(number[0] + ConvertHexToNumber(nextByte));
                        // Now we know the character in the encoding
                        valueBuilder.Append(charset.GetString(number));
                        break;
                    case (byte)SpecialByte.Underscore:
                        valueBuilder.Append(' ');
                        break;
                    default:
                        if (!MimeCheckByte(nextByte)) return "=";
                        valueBuilder.Append(Convert.ToChar(nextByte));
                        break;
                }
                nextByte = await reader.ReadByteAhead();
            }
            if (nextByte != (byte)SpecialByte.QuestionMark) return "=";
            nextByte = await reader.ReadByteAhead();
            if (nextByte != (byte)SpecialByte.Equals) return "=";
            reader.Clear();
            return valueBuilder.ToString();
        }

        private static byte ConvertHexToNumber(byte nextByte)
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
        #endregion MimeQuotedString

    }
}

