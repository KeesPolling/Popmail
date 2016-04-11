using System;
using System.Threading.Tasks;
using PopMail.EmailProxies.IP_helpers;


namespace PopMail.EmailProxies.EmailInterpreter
{
    internal class Eol
    {
        private FieldValue.EndType _endType; 

        internal async Task<FieldValue.EndType> ProcessEol(BufferedByteReader reader)
        {
            var rs = await reader.ReadByte();
            if (rs != (byte)FieldValue.SpecialByte.Linefeed)
            {
                throw new FormatException("carriagereturn must be followed by linefeed");
            }
            _endType = FieldValue.EndType.EndOfField; // unless folowed by a space ( a folding white space = FWS)
            rs = await reader.ReadByteAhead();
            if (rs == (byte) FieldValue.SpecialByte.Space)
            {
                rs = await reader.ReadByteAhead();
                while (rs == (byte) FieldValue.SpecialByte.Space)
                {
                    rs = await reader.ReadByteAhead();
                }
                _endType = FieldValue.EndType.None; // unless folowed by a crlf (End of header)
                // leave 1 space and the not space character on the buffer
                if (reader.BufferSize > 2) reader.RemoveFirst(reader.BufferSize - 2);
            }
            // Two crlf's with or without spaces in between signify te end of the Header 
            if (rs != (byte) FieldValue.SpecialByte.CarriageReturn) return _endType;

            reader.Clear();
            rs = await reader.ReadByte();
            if (rs != (byte)FieldValue.SpecialByte.Linefeed)
            {
                throw new FormatException("carriagereturn must be followed by linefeed");
            }

            return  FieldValue.EndType.EndOfHeader;
        }
    }
}
