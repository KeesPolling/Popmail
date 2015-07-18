using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PopMailDemo.EmailProxies.EmailInterpreter
{
    public class Email
    {
        public string Provider { get; set; }
        public string ProviderID { get; set; }
        public string Header { get; set; }
        public AddressList From { get; set; }
        public AddressList.Adress Sender { get; set; }
        public AddressList ReplyTo { get; set; } 
        public AddressList To { get; set; }
        public AddressList Cc { get; set; }
        public AddressList Bcc { get; set; }
        public DateTime OrigDate { get; set; }
        public string MessageId { get; set; }
        public List<string> InReplyTo { get; set; }
        public List<string> References { get; set; }
        public string Subject { get; set; }
        public string Comments { get; set; }
        public List<string> Keywords { get; set; }
        public List<string> BodyParts { get; set; }
    }
}
