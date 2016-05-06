using System;
using System.Text;
using System.Threading.Tasks;

namespace PopMail.EmailProxies.EmailInterpreter
{
    internal class UnstructuredTextFieldValue : FieldValue
    {
        internal string Value { get; set; }
        internal async Task<EndType> ReadFieldValue (BufferedByteReader reader)
        {
            var valueBuilder = new StringBuilder();
            var nextByte = await reader.ReadByteAsync();
            var endType = EndType.None;
            MimeState = PreviousMimeQuoted.NotMime;
            while ((endType == EndType.None))
            {
                switch (nextByte)
                {
                    case (byte)SpecialByte.CarriageReturn:
                        endType = await ProcessEol(reader);
                        break;
                    case (byte)SpecialByte.Equals:
                        var mimeResult = await MimeQuotedString(reader);
                        if (mimeResult == "=") MimeState = PreviousMimeQuoted.NotMime;
                        else
                        {
                            if (MimeState == PreviousMimeQuoted.MimeQuoted)
                            {
                                while (valueBuilder.Length > 0 && valueBuilder[valueBuilder.Length - 1] == ' ')
                                {
                                    valueBuilder.Remove(valueBuilder.Length - 1, 1);
                                }
                            }
                            MimeState = PreviousMimeQuoted.MimeQuoted;
                        }
                        valueBuilder.Append(mimeResult);
                        break;
                    case (byte)SpecialByte.Space:
                        valueBuilder.Append(Convert.ToChar(nextByte));
                        break;
                    default:
                        valueBuilder.Append(Convert.ToChar(nextByte));
                        MimeState= PreviousMimeQuoted.NotMime;
                        break;
                }
                if (endType == EndType.None) nextByte = await reader.ReadByteAsync();
            }
            Value = valueBuilder.ToString().Trim();
            return endType;
        }
    }
}
