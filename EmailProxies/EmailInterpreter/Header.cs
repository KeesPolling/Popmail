using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Windows.Networking.NetworkOperators;

namespace PopMail.EmailProxies.EmailInterpreter
{
    public class Header
    {
        public AddressListFieldValue From{ get; set; }
        public AddressListFieldValue.Adress Sender { get; set; }
        public AddressListFieldValue ReplyTo { get; set; }
        public AddressListFieldValue To { get; set; }
        public AddressListFieldValue Cc { get; set; }
        public AddressListFieldValue Bcc { get; set; }
        public DateTime OrigDate { get; set; }
        public string MessageId { get; set; }
        public IdentificationFieldValue InReplyTo { get; set; }
        public IdentificationFieldValue References { get; set; }
        public string Subject { get; set; }
        public string Comments { get; set; }
        public List<string> Keywords { get; set; }

        public ContentTypeFieldValue  ContentType { get; set; }
        public MimeFieldFieldValue ContentTransferEncoding { get; set; } 
        public string ContentDescription { get; set; }

        public async Task ReadHeader( BufferedByteReader reader)
        {
            var fieldName = new HeaderFieldName();

            var endType = FieldValue.EndType.None;
            var ignore = new HeaderIgnoreFieldValue();
            while (endType != FieldValue.EndType.EndOfHeader)
            {
                var name = await fieldName.ReadFieldName(reader);
                switch (name)
                {
                    case "From":
                        From = new AddressListFieldValue();
                        endType = await From.ReadFieldValue(reader);
                        break;
                    case "Sender":
                        var sender = new AddressListFieldValue();
                        endType = await sender.ReadFieldValue(reader);
                        Sender = sender.Adresses[0];
                        break;
                    case "Reply-To":
                        ReplyTo = new AddressListFieldValue();
                        endType = await ReplyTo.ReadFieldValue(reader);
                        break;
                    case "To":
                        To = new AddressListFieldValue();
                        endType = await To.ReadFieldValue(reader);
                        break;
                    case "CC":
                        Cc = new AddressListFieldValue();
                        endType = await Cc.ReadFieldValue(reader);
                        break;
                    case "Date":
                        var dateField = new DateFieldValue();
                        endType = await dateField.ReadFieldValue(reader);
                        OrigDate = dateField.Value;
                        break;
                    case "Message-ID":
                        var ids = new IdentificationFieldValue();
                        endType = await ids.ReadFieldValue(reader);
                        MessageId = ids.Identifiers[0];
                        break;
                    case "In-Reply-To":
                        InReplyTo = new IdentificationFieldValue();
                        endType = await InReplyTo.ReadFieldValue(reader);
                        break;
                    case "References":
                        References = new IdentificationFieldValue();
                        endType = await References.ReadFieldValue(reader);
                        break;
                    case "Subject":
                        var subject = new UnstructuredTextFieldValue();
                        endType = await subject.ReadFieldValue(reader);
                        Subject = subject.Value;
                        break;
                    case "Comments":
                        var comments = new UnstructuredTextFieldValue();
                        endType = await comments.ReadFieldValue(reader);
                        Subject = comments.Value;
                        break;
                    case "Content-type":
                        ContentType = new ContentTypeFieldValue();
                        endType = await ContentType.ReadFieldValue(reader);
                        break;
                    case "Content-rrandfer-encoding":
                        ContentTransferEncoding = new MimeFieldFieldValue();
                        //endType = await ContentTransferEncoding.ReadFieldValue(reader);
                        break;
                    case "Content-description":
                        var description = new UnstructuredTextFieldValue();
                        endType = await description.ReadFieldValue(reader);
                        ContentDescription = description.Value;
                        break;
                    default:
                        endType = await ignore.ReadFieldValue(reader);
                        break;
                }
            }
        }
    }
}
