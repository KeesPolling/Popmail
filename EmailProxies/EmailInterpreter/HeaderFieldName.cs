using System;
using System.Text;
using System.Threading.Tasks;

namespace PopMail.EmailProxies.EmailInterpreter
{
    internal class HeaderFieldName
    {
        internal async Task<string> ReadFieldName(BufferedByteReader reader)
        {
            var nameBuilder = new StringBuilder();
            var buffer = await reader.ReadByteAsync();
            while (buffer != (byte)FieldValue.SpecialByte.Colon)
            {
                nameBuilder.Append(Convert.ToChar(buffer));
                buffer = await reader.ReadByteAsync();
            }
            return nameBuilder.ToString().Trim();
        }
    }
}
