using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Windows.Networking.NetworkOperators;
using PopMail.EmailProxies.IP_helpers;
using System.IO;
using System.Text;

namespace PopMail.EmailProxies.EmailInterpreter
{
    public class Email
    {
        private readonly Header Header = new Header();

        public string Provider { get; set; }
        public string ProviderId { get; set; }
        public DateTime Received { get; set; }
        public ContentTypeFieldValue ContentType => Header.ContentType;
        public List<Body> Bodies { get; private set; }
        public async Task GetOneMail(IByteStreamReader streamReader)
        {
           var reader = new BufferedByteReader(streamReader);

            Received = DateTime.Now;
        }
    }
}
