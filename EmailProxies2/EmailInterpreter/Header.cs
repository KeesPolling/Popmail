using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Storage.Streams;

namespace PopMailDemo.EmailProxies.EmailInterpreter
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
        public string MessageId { get; set; }
        public IdentificationField InReplyTo { get; set; }
        public IdentificationField References { get; set; }
        public string Subject { get; set; }
        public string Comments { get; set; }
        public List<string> Keywords { get; set; }
        private byte[] headerEnd = new byte[]{(byte)FieldValue.SpecialByte.CarriageReturn, (byte)FieldValue.SpecialByte.Linefeed};
        public async Task ReadHeader( IByteStreamReader Reader)
        {
            var fieldName = new HeaderFieldName();
            var buffer = await Reader.ReadByte(); // vooruitlees omdat processFieldName 
                                                //een byte nodig heeft.
            while (buffer != headerEnd[0])
            {
                switch (await fieldName.ReadFieldName(buffer, Reader))
                {
                    case "From":
                        this.From = new AddressList();
                        buffer = await From.ReadAddressList(Reader);
                        break;
                    case "Sender":
                        var sender = new AddressList();
                        buffer = await sender.ReadAddressList(Reader);
                        this.Sender = sender.Adresses[0];
                        break;
                    case "Reply-To":
                        this.ReplyTo = new AddressList();
                        buffer = await ReplyTo.ReadAddressList(Reader);
                        break;
                    case "To":
                        this.To = new AddressList();
                        buffer = await To.ReadAddressList(Reader);
                        break;
                    case "Cc":
                        this.Cc = new AddressList();
                        buffer = await Cc.ReadAddressList(Reader);
                        break;
                    case "Date":
                        buffer = await HeaderIgnore.ReadIgnore(Reader);
                    //    ProcessDateTime(Dr);
                        break;
                    case "Message-ID":
                        var ids = new IdentificationField();
                        buffer = await ids.ReadIdentifiers(Reader);
                        this.MessageId = ids.Identifiers[0];
                        break;
                    case "In-Reply-To":
                        InReplyTo = new IdentificationField();
                        buffer = await InReplyTo.ReadIdentifiers(Reader);
                        break;
                    case "References":
                        this.References = new IdentificationField();
                        buffer = await References.ReadIdentifiers(Reader);
                        break;
                    default:
                        buffer = await HeaderIgnore.ReadIgnore(Reader);
                        break;
                }
            }
        }
    }
}
