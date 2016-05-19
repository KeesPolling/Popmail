using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Media.Protection.PlayReady;

namespace PopMail.EmailProxies.EmailInterpreter
{
    public class BodyPart
    {
        public static  Header Header { get; } = new Header();
        public ContentTypeFieldValue ContentType => Header.ContentType;

        public List<BodyPart> BodyParts { get; private set; }
        public byte[] Body { get; private set; }

        internal async Task ReadHeader(BufferedByteReader reader)
        {
            await Header.ReadHeader(reader).ConfigureAwait(false);
        }
    }
}
