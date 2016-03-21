using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace PopMailDemo.EmailProxies.EmailInterpreter
{
    class EOL
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
