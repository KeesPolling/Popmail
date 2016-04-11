using System;
using System.Text;
using System.Threading.Tasks;
using PopMail.EmailProxies.IP_helpers;

namespace PopMail.EmailProxies.EmailInterpreter
{
    internal class HeaderFieldName
    {
        internal async Task<string> ReadFieldName(BufferedByteReader reader)
        {
            var nameBuilder = new StringBuilder();
            var buffer = await reader.ReadByte();
            while (buffer != (byte)FieldValue.SpecialByte.Colon)
            {
                nameBuilder.Append(Convert.ToChar(buffer));
                buffer = await reader.ReadByte();
            }
            return nameBuilder.ToString().Trim();
        }
    }
}
