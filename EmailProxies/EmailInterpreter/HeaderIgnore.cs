using System.Threading.Tasks;
using PopMail.EmailProxies.IP_helpers;

namespace PopMail.EmailProxies.EmailInterpreter
{
    public class HeaderIgnore : FieldValue
    {
        public static async Task<EndType> ReadIgnore(BufferedByteReader reader)
        {
            var eol = new Eol();
            var nextByte = await reader.ReadByte();
            var endType = EndType.None;
            while ((endType == EndType.None))
            {
                switch (nextByte)
                {
                    case (byte)SpecialByte.CarriageReturn:
                        endType = await eol.ProcessEol(reader);
                        break;

                    case (byte)SpecialByte.Backslash:
                        nextByte = await reader.ReadByte();
                        break;
                }
                if (endType == EndType.None) nextByte = await reader.ReadByte();

            }
            return endType;
        }
    }
}
