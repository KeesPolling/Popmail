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
        public List<string> InReplyTo { get; set; }
        public List<string> References { get; set; }
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
                        var from = new AddressList();
                        buffer = await from.ReadAddressList(Reader);
                        this.From = from;
                        break;
                    case "Sender":
                        var sender = new AddressList();
                        buffer = await sender.ReadAddressList(Reader);
                        this.Sender = sender.Adresses[0];
                        break;
                    case "Reply-To":
                        var replyTo = new AddressList();
                        buffer = await replyTo.ReadAddressList(Reader);
                        this.ReplyTo = replyTo;
                        break;
                    case "To":
                        var to = new AddressList();
                        buffer = await to.ReadAddressList(Reader);
                        this.To = to;
                        break;
                    case "Cc":
                        var cc = new AddressList();
                        buffer = await cc.ReadAddressList(Reader);
                        this.Cc = cc;
                        break;
                    case "Date":
                        buffer = await HeaderIgnore.ReadIgnore(Reader);
                    //    ProcessDateTime(Dr);
                    //    break;
                    //case HeaderFieldType.MessageId:
                    //    ProcessMessageId(Dr);
                    //    break;
                    //case HeaderFieldType.Other:
                        break;
                    default:
                        buffer = await HeaderIgnore.ReadIgnore(Reader);
                        break;
                }
            }
        }
    }
}
