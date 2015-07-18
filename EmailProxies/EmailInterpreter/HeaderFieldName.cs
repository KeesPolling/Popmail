using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PopMailDemo.EmailProxies.EmailInterpreter
{
    internal class HeaderFieldName
    {
        internal async Task<string> ProcessFieldName(byte Buffer, IpDialog Ip)
        {
            var nameBuilder = new StringBuilder();
            var buffer = Buffer;
            while (buffer != 50)
            {
                nameBuilder.Append(Convert.ToChar(buffer));
                buffer = await Ip.ReadByte();
            }
            return nameBuilder.ToString().Trim();
        }
    }
}
