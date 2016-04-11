﻿using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using System.Threading.Tasks;
using Windows.Foundation.Metadata;
using PopMail.EmailProxies.IP_helpers;

namespace PopMail.EmailProxies.EmailInterpreter
{
    public class DateField : FieldValue
    {
        private string _stringRepresentation;
        public override string ToString()
        {
            return _stringRepresentation;
        }
        public DateTime Value { get; private set; }
        public DateField()
        {
            Value = new DateTime();
        }
        private async Task<string> ReadOneId(IByteStreamReader reader)
        {
            var valueBuilder = new StringBuilder();
            var nextByte = await reader.ReadByte();
            while (nextByte != (byte)SpecialByte.RightAngledBracket)
            {
                valueBuilder.Append(Convert.ToChar(nextByte));
                nextByte = await reader.ReadByte();
            }
            return valueBuilder.ToString();
        }
        internal async Task<EndType> ReadDateTime(BufferedByteReader reader)
        {

            var eol = new Eol();
            var nextByte = await reader.ReadByte();
            var valueBuilder = new StringBuilder();
            var endType = EndType.None;
            while ((endType == EndType.None))
            {
                switch (nextByte)
                {
                    case (byte)SpecialByte.CarriageReturn:
                        endType = await eol.ProcessEol(reader);
                        break;
                    case (byte)SpecialByte.LeftParenthesis: // "(": begin comment
                        await ReadComment(reader);
                        break;
                    case (byte)SpecialByte.Space:
                        if
                            (
                            (valueBuilder.Length == 0) ||
                            (valueBuilder[valueBuilder.Length - 1] != ' ')
                            ) valueBuilder.Append(' ');
                        break;
                    default: // alle andere gevallen
                        valueBuilder.Append(Convert.ToChar(nextByte));
                        break;
                }
                if (endType == EndType.None) nextByte = await reader.ReadByte();
            }
            _stringRepresentation = valueBuilder.ToString();
            Value = Convert.ToDateTime(_stringRepresentation, CultureInfo.InvariantCulture);
            return endType;
        }
    }
}
