using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using PopMail.EmailProxies.IP_helpers;

namespace PopMail.EmailProxies.EmailInterpreter
{
    public class Header
    {
        public AddressList From{ get; set; }
        public AddressList.Adress Sender { get; set; }
        public AddressList ReplyTo { get; set; }
        public AddressList To { get; set; }
        public AddressList Cc { get; set; }
        public AddressList Bcc { get; set; }
        public DateTime OrigDate { get; set; }
        public DateTime Received { get; set; }
        public string MessageId { get; set; }
        public IdentificationField InReplyTo { get; set; }
        public IdentificationField References { get; set; }
        public string Subject { get; set; }
        public string Comments { get; set; }
        public List<string> Keywords { get; set; }
        public async Task ReadHeader( BufferedByteReader reader)
        {
            var fieldName = new HeaderFieldName();

            var endType = FieldValue.EndType.None;
            while (endType != FieldValue.EndType.EndOfHeader)
            {
                switch (await fieldName.ReadFieldName(reader))
                {
                    case "From":
                        this.From = new AddressList();
                        endType = await From.ReadAddressList(reader);
                        break;
                    case "Sender":
                        var sender = new AddressList();
                        endType = await sender.ReadAddressList(reader);
                        this.Sender = sender.Adresses[0];
                        break;
                    case "Reply-To":
                        this.ReplyTo = new AddressList();
                        endType = await ReplyTo.ReadAddressList(reader);
                        break;
                    case "To":
                        this.To = new AddressList();
                        endType = await To.ReadAddressList(reader);
                        break;
                    case "Cc":
                        this.Cc = new AddressList();
                        endType = await Cc.ReadAddressList(reader);
                        break;
                    case "Date":
                        var dateField = new DateField();
                        endType = await dateField.ReadDateTime(reader);
                        OrigDate = dateField.Value;
                        break;
                    case "Message-ID":
                        var ids = new IdentificationField();
                        endType = await ids.ReadIdentifiers(reader);
                        this.MessageId = ids.Identifiers[0];
                        break;
                    case "In-Reply-To":
                        InReplyTo = new IdentificationField();
                        endType = await InReplyTo.ReadIdentifiers(reader);
                        break;
                    case "References":
                        this.References = new IdentificationField();
                        endType = await References.ReadIdentifiers(reader);
                        break;
                    default:
                        endType = await HeaderIgnore.ReadIgnore(reader);
                        break;
                }
            }
            Received = DateTime.Now;
        }
    }
}
