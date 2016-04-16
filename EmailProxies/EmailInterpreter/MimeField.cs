using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Xaml.Media;

namespace PopMail.EmailProxies.EmailInterpreter
{
    public class MimeField : FieldValue
    {
        public string Value { get; set; }
        public Dictionary<string, string> Parameters { get; set; } 
            = new Dictionary<string, string>();

        internal override async Task<EndType> ReadFieldValue(BufferedByteReader reader)
        {
            var valueBuilder = new StringBuilder();

            var eol = new Eol();
            var endType = EndType.None;
            var nextByte = await reader.ReadByte();

            // skip leading spaces
            while (nextByte == (byte) SpecialByte.Space)
            {
                nextByte = await reader.ReadByte();
            }
            // First word: Value
            while
                (
                    nextByte != (byte)SpecialByte.SemiColon &&
                    nextByte !=(byte)SpecialByte.Space &&
                    nextByte != (byte)SpecialByte.CarriageReturn
                )
            {
                valueBuilder.Append(Convert.ToChar(nextByte));
                nextByte = await reader.ReadByte();
            }
            Value = valueBuilder.ToString().TrimStart();
            valueBuilder = new StringBuilder();

            // and now the parameters
            string key = null;
            while (endType == EndType.None)
            {
                switch (nextByte)
                {
                    case (byte)SpecialByte.CarriageReturn:
                        endType = await eol.ProcessEol(reader);
                        break;
                    case (byte)SpecialByte.Equals:
                        key = valueBuilder.ToString().Trim();
                        valueBuilder = new StringBuilder();
                        break;
                    case (byte)SpecialByte.Space:
                        if (valueBuilder.Length > 0 && valueBuilder[valueBuilder.Length - 1] != ' ')
                        {
                            valueBuilder.Append(' ');
                        }
                        break;
                    case (byte)SpecialByte.LeftParenthesis: // "(": begin comment
                        await ReadComment(reader);
                        break;
                    case (byte)SpecialByte.BackSlash: // "\": begin "quoted character"
                        valueBuilder.Append(Convert.ToChar(nextByte));
                        MimeState = PreviousMimeQuoted.NotMime;
                        break;
                    case (byte)SpecialByte.Quote: //  """: begin quoted string
                        valueBuilder.Append(await ReadQuotedString(reader));
                        MimeState = PreviousMimeQuoted.NotMime;
                        break;
                    case (byte)SpecialByte.SemiColon: // ";" End of parameter
                        if (key == null) break; // start of first parameter
                        Parameters.Add(key, valueBuilder.ToString().Trim());
                        key = null;
                        valueBuilder = new StringBuilder();
                        break;
                    default: // alle andere gevallen
                        valueBuilder.Append(Convert.ToChar(nextByte));
                        MimeState = PreviousMimeQuoted.NotMime;
                        break;
                }
                if (endType == EndType.None) nextByte = await reader.ReadByte();
            }
            if (key != null) Parameters.Add(key, valueBuilder.ToString().Trim());

            return endType;
        }
     }
}
