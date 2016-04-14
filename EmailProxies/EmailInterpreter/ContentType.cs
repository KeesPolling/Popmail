using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PopMail.EmailProxies.EmailInterpreter
{
    public class ContentType : FieldValue
    {
        public string Type { get; set; }
        public string SubType { get; set; }
        public Dictionary<string, string> Parameters { get; set; }

        public async Task<EndType> ReadContentType(BufferedByteReader reader)
        {
            var valueBuilder = new StringBuilder();

            var eol = new Eol();
            var endType = EndType.None;
            var nextByte = await reader.ReadByteAhead();
            endType = await ProcessBetweenWords(reader, nextByte);
            if (endType != EndType.None) return endType;
            nextByte = await reader.ReadByte();

            // First word: Type
            while (nextByte != (byte)SpecialByte.Slash)
            {
                valueBuilder.Append(Convert.ToChar(nextByte));
                nextByte = await reader.ReadByte();
            }
            Type = valueBuilder.ToString().ToLower();
            nextByte = await reader.ReadByte();
            //SubType
            while (nextByte != (byte)SpecialByte.SemiColon)
            {
                valueBuilder.Append(Convert.ToChar(nextByte));
                nextByte = await reader.ReadByte();
            }
            SubType = valueBuilder.ToString().ToLower();


            return endType;
        }
        private async Task<EndType>ProcessBetweenWords(BufferedByteReader reader, byte nextByte)
        {
            var eol = new Eol();
            var endType = EndType.None;
            while
            (
                (endType == EndType.None) &&
                (
                    (nextByte == (byte)SpecialByte.CarriageReturn) ||
                    (nextByte == (byte)SpecialByte.LeftParenthesis) ||
                    (nextByte == (byte)SpecialByte.Space)
                )
            )
            {
                reader.Clear();
                switch (nextByte)
                {
                    case (byte)SpecialByte.CarriageReturn:
                        endType = await eol.ProcessEol(reader);
                        break;
                    case (byte)SpecialByte.LeftParenthesis: // "(": begin comment
                        await ReadComment(reader);
                        break;
                }
                if (endType == EndType.None) nextByte = await reader.ReadByteAhead();
            }
            return endType;
        }
    }
}
