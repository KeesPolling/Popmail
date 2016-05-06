using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Windows.Networking.NetworkOperators;
using PopMail.EmailProxies.IP_helpers;

namespace PopMail.EmailProxies.EmailInterpreter
{
    public class Email
    {
        private static readonly Header Header = new Header();

        public string Provider { get; set; }
        public string ProviderId { get; set; }
        public DateTime Received { get; set; }
        public ContentTypeFieldValue ContentType => Header.ContentType;
        public List<BodyPart> BodyParts { get; private set; }
        public byte[] Body { get; private set; }

        public async Task GetMail(IByteStreamReader streamReader)
        {
            var reader = new BufferedByteReader(streamReader);
            await Header.ReadHeader(reader);
            if (ContentType.Type == "multipart")
            {
                var bodyPartReader = new BodyPartReader();
                BodyParts = await bodyPartReader.ReadBodyParts(ContentType.Parameters["boundary"], reader);
            }


            Received = DateTime.Now;
        }
    }
}
