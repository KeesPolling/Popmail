﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Globalization;
using System.Threading.Tasks;

namespace PopMail.EmailProxies.EmailInterpreter
{
    public class MimeFieldFieldValue : FieldValue
    {
        

        internal async Task<byte[]> ReadBytesParameter(BufferedByteReader reader)
        {
            var nextByte = await reader.ReadByteAsync();
            var memStream = new MemoryStream();
            if (nextByte == (byte)SpecialByte.Quote)
                memStream = await GetQuotedParameterValue(reader);
            else
            {
                while (MimeCheckByte(nextByte))
                {
                    memStream.WriteByte(nextByte);
                    nextByte = await reader.ReadByteAsync();
                }
                if (nextByte == (byte)SpecialByte.CarriageReturn) await ProcessEol(reader);
            }
            var bytes = memStream.ToArray();
            memStream.Dispose();
            return bytes;
        }
        internal async Task<DateTime> ReadDateParameter(BufferedByteReader reader)
        {
            var stringValue = await ReadStringParameter(reader);
            var dateTimeValue = Convert.ToDateTime(stringValue, CultureInfo.InvariantCulture);
            return dateTimeValue;
        }
        internal async Task<Int32> ReadNumberParameter(BufferedByteReader reader)
        {
            var stringValue = await ReadStringParameter(reader);
            var numberValue = Convert.ToInt32(stringValue);
            return numberValue;
        }
        internal async Task<string> ReadStringParameter(BufferedByteReader reader)
        {
            var bytes = await ReadBytesParameter(reader);
            var stringValue = Encoding.ASCII.GetString(bytes);
            return stringValue;
        }
        private async Task<MemoryStream> GetQuotedParameterValue(BufferedByteReader reader)
        {
            var nextByte = await reader.ReadByteAsync();
            var memStream = new MemoryStream();
            while (nextByte != (byte)SpecialByte.Quote)
            {
                memStream.WriteByte(nextByte);
                nextByte = await reader.ReadByteAsync();
            }
            return memStream;
        }
        internal async Task<string> ReadFieldValue(BufferedByteReader reader)
        { 
            var nextByte = await reader.ReadByteAsync();
            var memStream = new MemoryStream();
            while ((TypOfEnd == EndType.None) && (nextByte == (byte)SpecialByte.Space || nextByte == (byte)SpecialByte.CarriageReturn))
            {
                switch (nextByte)
                {
                    case (byte)SpecialByte.CarriageReturn:
                        await ProcessEol(reader);
                        break;
                    case (byte)SpecialByte.Space:
                        break;
                    case (byte)SpecialByte.LeftParenthesis: // "(": begin comment
                        await ReadComment(reader);
                        break;
                   default: // alle andere gevallen
                       break;
                }
                if (TypOfEnd == EndType.None) nextByte = await reader.ReadByteAsync();
            }
            while (MimeCheckByte(nextByte))
            {
                 memStream.WriteByte(nextByte);
                 nextByte = await reader.ReadByteAsync();
             }
            if (nextByte == (byte)SpecialByte.CarriageReturn) await ProcessEol(reader);
            
            var bytes = memStream.ToArray();
            memStream.Dispose();
            return Encoding.ASCII.GetString(bytes);
        }
    }
}

