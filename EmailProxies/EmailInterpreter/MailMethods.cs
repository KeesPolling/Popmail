using System.Text;
using System.IO;

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
            return unEncoded;
        }
    }
}
