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
        internal async Task<byte> ProcessEOL(IpDialog Ip)
        {
            var rs = await Ip.ReadByte();
            if (rs != (byte)SpecialByte.Linefeed)
            {
                throw new FormatException("carriagereturn must be followed by linefeed");
            }
            rs = await Ip.ReadByte();
            if (rs == (byte)SpecialByte.Space)
            {
                rs = await Ip.ReadByte();
                while (rs == (byte)SpecialByte.Space)
                {
                    rs = await Ip.ReadByte();
                }
                if (rs == (byte)SpecialByte.CarriageReturn)
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
