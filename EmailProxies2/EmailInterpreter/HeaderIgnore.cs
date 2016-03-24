using System.Threading.Tasks;

namespace PopMail.EmailProxies.EmailInterpreter
{
    public class HeaderIgnore : FieldValue
    {
        public static async Task<byte> ReadIgnore(IByteStreamReader Reader)
        {
            var eol = new EOL();
            var nextByte = await Reader.ReadByte();

            while (!eol.End)
            {
                if (nextByte == (byte)SpecialByte.CarriageReturn)
                {
                    nextByte = await eol.ProcessEOL(Reader);
                    continue;
                }
                if (nextByte == (byte)SpecialByte.Backslash) // "\": begin "quoted character"
                {
                    await Reader.ReadByte();
                }
                nextByte = await Reader.ReadByte();
            }
            return nextByte;
        }
    }
}
