using System.Collections.Generic;
using System.Threading.Tasks;
using PopMail.EmailProxies.IP_helpers;

namespace PopMail.EmailProxies.EmailInterpreter
{
    #region SpecialByte
    internal enum SpecialByte : byte
    {
        Linefeed = 10,
        CarriageReturn = 13,
        Hyphen = 45
    }
    #endregion SpecialByte

    internal class BodyReader
    {
        internal ContentTypeFieldValue _contentType { get; private set; }
        internal List<byte[]> Boundaries { get; set; }
        private readonly  byte[] _boundaryStart = new byte[4] { 13, 10, 45, 45 }; // "\r\n--"

        internal BodyReader(ContentTypeFieldValue contentType, string encoding, List<byte[]> boundaries)
        {
            Boundaries = boundaries;
        }
        internal async Task ReadToStart(BufferedByteReader reader)
        {
            var nextByte = await reader.ReadByteAsync();
            var boundaryeByteIndex = 0;
            while (boundaryeByteIndex < _boundary.Length)
            {
                if (nextByte == _boundary[0])
                {
                    nextByte = await reader.ReadByteAsync();
                    boundaryeByteIndex = 1;
                    while (boundaryeByteIndex < _boundary.Length && nextByte == _boundary[boundaryeByteIndex])
                    {
                        nextByte = await reader.ReadByteAsync();
                        boundaryeByteIndex += 1;
                    }
                }
                else nextByte = await reader.ReadByteAsync();
            }
        }
        internal async Task<Body> ReadBody( BufferedByteReader reader)
        {
            var body = new Body();
            
            var nextByte = await reader.ReadByteAsync();
            {
                if (nextByte == _boundary[0])
                {
                    if (await CheckBytes(reader))
                    {
                        var bodyPart = new Body();
                        await bodyPart.Header.ReadHeader(reader);
                    }
                }
                nextByte = await reader.ReadByteAsync();
            }
            return body;
        }

        private async Task<bool> CheckBytes(BufferedByteReader reader)
        {
            var i = 1;
            while (i < _boundary.Length)
            {
                if (await reader.ReadByteAsync() != _boundary[i]) return false;
                i += 1;
            }
            return true;
        }
        public async Task<List<Body>> GetBodies(IByteStreamReader streamReader, ContentTypeFieldValue contentType, string transportEncoding)
        {
            var reader = new BufferedByteReader(streamReader);
           
            switch (contentType.Type)
            {
                case "multipart":
                    var bodyPartReader = new BodyReader(ContentType, Header.ContentTransferEncoding);

                    var bodies = await bodyPartReader.ReadBody(reader);
                    break;
                case "text":
                    var textString = "";
                    using (var memstream = await streamReader.GetMemoryStreamAsync())
                    {
                        using (var unEncoded = await MailMethods.UnEncode(memstream, Header.ContentTransferEncoding))
                        {
                            using (var textReader = new StreamReader(unEncoded, Encoding.GetEncoding(Header.ContentType.Charset)))
                            {
                                textString = await textReader.ReadToEndAsync();
                            }
                        }
                    }
                    var textBody = new Body();
                    textBody.Content = (new MemoryStream(Encoding.Unicode.GetBytes(textString)));
                    break;
                case "image":
                case "audio":
                case "video":
                case "application":
                    var binaryBody = new Body();
                    binaryBody.Content = await streamReader.GetMemoryStreamAsync());
                    break;
            }
        }
        }
}
}
