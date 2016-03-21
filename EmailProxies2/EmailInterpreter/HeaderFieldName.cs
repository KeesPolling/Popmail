using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PopMailDemo.EmailProxies.EmailInterpreter
{
    internal class HeaderFieldName
    {
        internal async Task<string> ReadFieldName(byte Buffer, IByteStreamReader Ip)
        {
            var nameBuilder = new StringBuilder();
            var buffer = Buffer;
            while (buffer != (byte)FieldValue.SpecialByte.Colon)
            {
                nameBuilder.Append(Convert.ToChar(buffer));
                buffer = await Ip.ReadByte();
            }
            return nameBuilder.ToString().Trim();
        }
    }
}
