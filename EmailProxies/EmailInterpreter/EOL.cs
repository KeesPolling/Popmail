using System;
using System.Threading.Tasks;


namespace PopMail.EmailProxies.EmailInterpreter
{
    internal class EOL
    {
        internal bool End { get; set; }
        internal async Task<byte> ProcessEOL(IByteStreamReader Reader)
        {
            var rs = await Reader.ReadByte();
            if (rs != (byte)FieldValue.SpecialByte.Linefeed)
            {
                throw new FormatException("carriagereturn must be followed by linefeed");
            }
            rs = await Reader.ReadByte();
            if (rs == (byte)FieldValue.SpecialByte.Space)
            {
                rs = await Reader.ReadByte();
                while (rs == (byte)FieldValue.SpecialByte.Space)
                {
                    rs = await Reader.ReadByte();
                }
                if (rs == (byte)FieldValue.SpecialByte.CarriageReturn)
                {
                    this.End = true;
                }
                else
                {
                    this.End = false;
                }
            }
            else
            {
                this.End = true;
            }
            return rs;
        }
    }
}
