using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using PopMail.EmailProxies.IP_helpers;

namespace PopMail.EmailProxies.EmailInterpreter
{
    public class IdentificationFieldValue : FieldValue
    {
        public List<string> Identifiers { get; set; }
        public IdentificationFieldValue()
        {
            Identifiers = new List<string>();
        }
        private async Task<string> ReadOneId(IByteStreamReader reader)
        {
            var valueBuilder = new StringBuilder();
            var nextByte = await reader.ReadByteAsync();
            while (nextByte != (byte)SpecialByte.RightAngledBracket)
            {
                valueBuilder.Append(Convert.ToChar(nextByte));
                nextByte = await reader.ReadByteAsync();
            }
            return valueBuilder.ToString();
        }
        internal async Task<EndType> ReadFieldValue(BufferedByteReader reader)
        {

            var nextByte = await reader.ReadByteAsync();

            var endType = EndType.None;
            while ((endType == EndType.None))
            {
                if (nextByte == (byte)SpecialByte.CarriageReturn)
                {
                    endType = await ProcessEol(reader);
                    if (endType == EndType.None) nextByte = await reader.ReadByteAsync();
                    continue;
                }
                switch (nextByte)
                {
                    case (byte)SpecialByte.LeftParenthesis: // "(": begin comment
                        await ReadComment(reader);
                        break;
                    case (byte)SpecialByte.LeftAngledBracket: // "<": begin id
                        Identifiers.Add(await ReadOneId(reader));
                        break;
                    default: // alle andere gevallen
                        break;
                }
                nextByte = await reader.ReadByteAsync();
            }
            return endType;
        }
    }
}
