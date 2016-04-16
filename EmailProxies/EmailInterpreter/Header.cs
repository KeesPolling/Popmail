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

        public ContentType  ContentType { get; set; }


        public async Task ReadHeader( BufferedByteReader reader)
        {
            var fieldName = new HeaderFieldName();

            var endType = FieldValue.EndType.None;
            var ignore = new HeaderIgnore();
            while (endType != FieldValue.EndType.EndOfHeader)
            {
                switch (await fieldName.ReadFieldName(reader))
                {
                    case "From":
                        From = new AddressList();
                        endType = await From.ReadFieldValue(reader);
                        break;
                    case "Sender":
                        var sender = new AddressList();
                        endType = await sender.ReadFieldValue(reader);
                        Sender = sender.Adresses[0];
                        break;
                    case "Reply-To":
                        ReplyTo = new AddressList();
                        endType = await ReplyTo.ReadFieldValue(reader);
                        break;
                    case "To":
                        To = new AddressList();
                        endType = await To.ReadFieldValue(reader);
                        break;
                    case "CC":
                        Cc = new AddressList();
                        endType = await Cc.ReadFieldValue(reader);
                        break;
                    case "Date":
                        var dateField = new DateField();
                        endType = await dateField.ReadFieldValue(reader);
                        OrigDate = dateField.Value;
                        break;
                    case "Message-ID":
                        var ids = new IdentificationField();
                        endType = await ids.ReadFieldValue(reader);
                        MessageId = ids.Identifiers[0];
                        break;
                    case "In-Reply-To":
                        InReplyTo = new IdentificationField();
                        endType = await InReplyTo.ReadFieldValue(reader);
                        break;
                    case "References":
                        References = new IdentificationField();
                        endType = await References.ReadFieldValue(reader);
                        break;
                    case "Subject":
                        var subject = new UnstructuredText();
                        endType = await subject.ReadFieldValue(reader);
                        Subject = subject.Value;
                        break;
                    case "Comments":
                        var comments = new UnstructuredText();
                        endType = await comments.ReadFieldValue(reader);
                        Subject = comments.Value;
                        break;
                    case "Content-Type":
                        ContentType = new ContentType();
                        endType = await ContentType.ReadFieldValue(reader);
                        break;

                    default:
                        endType = await ignore.ReadFieldValue(reader);
                        break;
                }
            }
            Received = DateTime.Now;
        }
    }
}
