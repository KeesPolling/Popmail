using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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
        internal bool UnEncode()
        {
            switch (TransferEncoding)
            {
                case transferEncoding.sevenBit:
                    Content = Encoding.ASCII.GetString(EncodedContent.ToArray());
                    break;
                case transferEncoding.eightBit:
                    Content = Encoding.GetEncoding(Charset).GetString(EncodedContent.ToArray());
                    break;
                case transferEncoding.base64:
                    var base64String = Encoding.ASCII.GetString(EncodedContent.ToArray());
                    Content = Encoding.GetEncoding(Charset).GetString(Convert.FromBase64String(base64String));
                    break;
                case transferEncoding.quotedPrintable:
                    Content = GetStringFromQuotedPrintable(EncodedContent, Encoding.GetEncoding(Charset));
                    break;
            }
            return true;
        }
        internal string GetStringFromQuotedPrintable(MemoryStream stream, Encoding charset)
        {
            var stringBuilder = new StringBuilder();
            stream.Position = 0;
            var nextByte = (byte)stream.ReadByte();
            var twoBytes = new byte[2];
            while (stream.Length > stream.Position)
            {
                if (nextByte == (byte)'=')
                {
                    if (!(stream.Length - stream.Position < 2))
                    {
                        twoBytes[0] = (byte)stream.ReadByte();
                        twoBytes[1] = (byte)stream.ReadByte();
                        var hex = Encoding.ASCII.GetString(twoBytes);
                        if (hex == "\r\n") //soft eol;
                        {
                            nextByte = (byte)stream.ReadByte();
                            continue;
                        }
                        if (!byte.TryParse(
                            hex,
                            System.Globalization.NumberStyles.HexNumber,
                            System.Globalization.CultureInfo.InvariantCulture,
                            out nextByte))
                        {
                            stringBuilder.Append('=');
                            stringBuilder.Append(hex);
                            nextByte = (byte)stream.ReadByte();
                            continue;
                        }
                    }
                }
                stringBuilder.Append(charset.GetString(new byte[1] {nextByte}));
                nextByte = (byte)stream.ReadByte();
            }
            return stringBuilder.ToString();
        }
    }
}
