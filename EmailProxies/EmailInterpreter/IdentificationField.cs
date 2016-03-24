using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace PopMail.EmailProxies.EmailInterpreter
{
    public class IdentificationField : FieldValue
    {
        public List<string> Identifiers { get; set; }
        public IdentificationField()
        {
            Identifiers = new List<string>();
        }
        private async Task<string> ReadOneId(IByteStreamReader Reader)
        {
            var valueBuilder = new StringBuilder();
            var nextbyte = await Reader.ReadByte();
            while (nextbyte != (byte)SpecialByte.RightAngledBracket)
            {
                valueBuilder.Append(Convert.ToChar(nextbyte));
                nextbyte = await Reader.ReadByte();
            }
            return valueBuilder.ToString();
        }
        internal async Task<byte> ReadIdentifiers(IByteStreamReader Reader)
        {

            var eol = new EOL();
            var nextByte = await Reader.ReadByte();

            while (!eol.End)
            {
                if (nextByte == (byte)SpecialByte.CarriageReturn)
                {
                    nextByte = await eol.ProcessEOL(Reader);
                    continue;
                }
                switch (nextByte)
                {
                    case (byte)SpecialByte.LeftParenthesis: // "(": begin comment
                        await ReadComment(Reader);
                        break;
                    case (byte)SpecialByte.LeftAngledBracket: // "<": begin id
                        Identifiers.Add(await ReadOneId(Reader));
                        break;
                    default: // alle andere gevallen
                        break;
                }
                nextByte = await Reader.ReadByte();
            }
            return nextByte;
        }
    }
}
