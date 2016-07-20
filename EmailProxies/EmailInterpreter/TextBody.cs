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
        internal MemoryStream EncodedContent { get; set; }
        internal transferEncoding TransferEncoding { get; private set; }
        internal string Charset { get; private set; }
        internal string Content { get; set; }

        internal TextBody(string transferEncoding, string charset)
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
            Charset = charset;
        }
    }
}
