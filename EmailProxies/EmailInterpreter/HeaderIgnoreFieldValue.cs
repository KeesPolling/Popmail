using System.Threading.Tasks;
using PopMail.EmailProxies.IP_helpers;

namespace PopMail.EmailProxies.EmailInterpreter
{
    public class HeaderIgnoreFieldValue : FieldValue
    {
        internal async Task<EndType> ReadFieldValue(BufferedByteReader reader)
        {
            var nextByte = await reader.ReadByteAsync();
            var endType = EndType.None;
            while ((endType == EndType.None))
            {
                switch (nextByte)
                {
                    case (byte)SpecialByte.CarriageReturn:
                        endType = await ProcessEol(reader);
                        break;

                    case (byte)SpecialByte.BackSlash:
                        nextByte = await reader.ReadByteAsync();
                        break;
                }
                if (endType == EndType.None) nextByte = await reader.ReadByteAsync();

            }
            return endType;
        }
    }
}
