using System.Text;
using System.IO;
using System;
using System.Threading.Tasks;

namespace PopMail.EmailProxies.EmailInterpreter
{
    public static class MailMethods
    {
        public static MemoryStream FromQuotedPrintable(MemoryStream encodedStream)
        {
            var unEncoded = new MemoryStream();
            var buffer = new MemoryStream();
            encodedStream.Position = 0;
            var nextByte = (byte)encodedStream.ReadByte();
            var twoBytes = new byte[2];
            while (encodedStream.Length > encodedStream.Position)
            {
                if (nextByte == (byte)' ' || nextByte == (byte)'\t')
                {
                    buffer.WriteByte(nextByte);
                    nextByte = (byte)encodedStream.ReadByte();
                    continue;
                }
                if (nextByte == '\r')
                {
                    buffer.Dispose();
                    buffer = new MemoryStream();
                }
                if (buffer.Length > 0)
                {
                    buffer.CopyTo(unEncoded);
                    buffer.Dispose();
                    buffer = new MemoryStream();
                }
                if (nextByte == (byte)'=')
                {
                    if (!(encodedStream.Length - encodedStream.Position < 2))
                    {
                        twoBytes[0] = (byte)encodedStream.ReadByte();
                        twoBytes[1] = (byte)encodedStream.ReadByte();
                        var hex = Encoding.ASCII.GetString(twoBytes);
                        if (hex == "\r\n") //soft eol;
                        {
                            nextByte = (byte)encodedStream.ReadByte();
                            continue;
                        }
                        if (!byte.TryParse(
                            hex,
                            System.Globalization.NumberStyles.HexNumber,
                            System.Globalization.CultureInfo.InvariantCulture,
                            out nextByte))
                        {
                            unEncoded.WriteByte((byte)'=');
                            unEncoded.WriteByte(twoBytes[0]);
                            nextByte = twoBytes[1];
                        }
                    }
                }
                unEncoded.WriteByte(nextByte);
                nextByte = (byte)encodedStream.ReadByte();
            }
            buffer.Dispose();
            return unEncoded;
        }
        public static async Task<MemoryStream> UnEncode(MemoryStream encodedContent, string transferEncoding)
        {
            var unEncodedContent = new MemoryStream();
            try
            {
                switch (transferEncoding.ToLower())
                {
                    case "7bit":
                    case "8bit":
                    case "binary":
                        unEncodedContent = encodedContent;
                        break;
                    case "quoted-printable":
                        unEncodedContent = MailMethods.FromQuotedPrintable(encodedContent);
                        break;
                    case "base64":
                        var base64String = Encoding.ASCII.GetString(encodedContent.ToArray());
                        var bytes = Convert.FromBase64String(base64String);
                        await unEncodedContent.WriteAsync(bytes, 0, bytes.Length);
                        break;
                    default:
                        using (var writer = new StreamWriter(unEncodedContent, Encoding.ASCII))
                        {
                            writer.WriteLine("Unknown 'Content-transfer-encoding' encounterd: {0}", transferEncoding);
                            writer.WriteLine("------Encoded Content-------");
                            await writer.FlushAsync();
                            await encodedContent.CopyToAsync(unEncodedContent);
                        }
                        break;
                }
                return unEncodedContent;
            }
            catch (Exception ex)
            {
                using (var writer = new StreamWriter(unEncodedContent, Encoding.ASCII))
                {
                    writer.WriteLine(ex.Message);
                    writer.WriteLine("------Encoded Content-------");
                    await writer.FlushAsync();
                    await encodedContent.CopyToAsync(unEncodedContent);
                    return unEncodedContent;
                }
            }
        }
    }
}
