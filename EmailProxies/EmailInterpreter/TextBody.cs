using System;
using System.Text;
using System.IO;
using Windows.ApplicationModel.Resources;

namespace PopMail.EmailProxies.EmailInterpreter
{
    public class TextBody 
    {
        internal enum transferEncoding
        {
            sevenBit = 0,
            eightBit,
            binary,
            quotedPrintable,
            base64
       }
        internal enum textType
        {
            plain = 0,
            enriched,
            html
        }
        internal MemoryStream EncodedContent { get; set; }
        internal transferEncoding TransferEncoding { get; private set; }
        internal textType TextType { get; private set; }
        internal string Charset { get; private set; }
        internal string Content { get; set; }

        internal TextBody(string transferEncoding, string textType, string charset)
        {
            switch (transferEncoding.ToLower())
            {
                case null:
                case "7bit":
                    TransferEncoding = TextBody.transferEncoding.sevenBit;
                    break;
                case "8bit":
                    TransferEncoding = TextBody.transferEncoding.eightBit;
                    break;
                case "binary":
                    TransferEncoding = TextBody.transferEncoding.binary;
                    break;
                case "quoted-printable":
                    TransferEncoding = TextBody.transferEncoding.quotedPrintable;
                    break;
                case "base64":
                    TransferEncoding = TextBody.transferEncoding.base64;
                    break;
                default:
                    throw new ArgumentException("Unknown type of Content-transfer-encoding", transferEncoding);
            }
            switch (textType.ToLower())
            {
                case "plain":
                    TextType = TextBody.textType.plain;
                    break;
                case "enriched":
                    TextType = TextBody.textType.enriched;
                    break;
                case "html":
                    TextType = TextBody.textType.html;
                    break;
            }
            Charset = charset;
        }
        internal bool UnEncode(MemoryStream encodedContent)
        {
            EncodedContent = encodedContent;
            try
            {
                switch (TransferEncoding)
                {
                    case transferEncoding.sevenBit:
                        Content = Encoding.ASCII.GetString(EncodedContent.ToArray());
                        break;
                    case transferEncoding.eightBit:
                    case transferEncoding.binary:
                        Content = Encoding.GetEncoding(Charset).GetString(EncodedContent.ToArray());
                        break;
                    case transferEncoding.quotedPrintable:
                        Content = Encoding.GetEncoding(Charset).GetString
                            (MailMethods.FromQuotedPrintable(EncodedContent).ToArray());
                        break;
                    case transferEncoding.base64:
                        var base64String = Encoding.ASCII.GetString(EncodedContent.ToArray());
                        Content = Encoding.GetEncoding(Charset).GetString(Convert.FromBase64String(base64String));
                        break;
                }
            }
            catch(Exception ex)
            {
                var stringBuilder = new StringBuilder(ex.Message);
                stringBuilder.Append("\r\n\r\n");
                var reader = new StreamReader(EncodedContent);
                stringBuilder.Append(reader.ReadToEnd());
                reader.Dispose();
                return false;
            }
            EncodedContent.Dispose();
            return true;
        }
    }
}
