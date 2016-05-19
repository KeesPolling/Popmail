using System;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Xaml.Documents;

namespace PopMail.EmailProxies.EmailInterpreter
{
    public class ContentTypeFieldValue : MimeFieldFieldValue
    {
        public string Type { get; private set; }
        public string SubType { get; private set; }
        public string Charset { get; private set; }
        public byte[] Boundary { get; private set; }
        public string TYPE { get; private set; }
        public int Padding { get; private set; }
        public int Number { get; private set; }
        public int Total {get; private set;}
        public string Id { get; private set; }
        public string AccesType { get; private set; }
        public DateTime Expiration { get; private set; }
        public int Size { get; private set; }
        public string Permission { get; private set; }
        public string Name { get; private set; }
        public string Site { get; private set; }
        public string Directory { get; private set; }
        public string Mode { get; private set; }
        public string Server { get; private set; }
        public string Subject { get; private set; }

        new internal async Task<EndType>  ReadFieldValue(BufferedByteReader reader)
        {
            var value = await base.ReadFieldValue(reader);
            var result = value.Split('/');
            Type = result[0];
            SubType = result[1];

            // and now the parameters
            string key = null;
            var valueBuilder = new StringBuilder();
            var nextByte = await reader.ReadByteAsync();

            while (base.TypOfEnd == EndType.None)
            {
                switch (nextByte)
                {
                    case (byte)SpecialByte.CarriageReturn:
                        await ProcessEol(reader);
                        break;
                    case (byte)SpecialByte.Equals:
                        key = valueBuilder.ToString().Trim();
                        await ReadParameter(key, reader);
                        valueBuilder = new StringBuilder();
                        break;
                    case (byte)SpecialByte.Space:
                        break;
                    case (byte)SpecialByte.LeftParenthesis: // "(": begin comment
                        await ReadComment(reader);
                        break;
                    case (byte)SpecialByte.SemiColon: // ";" End of parameter
                        break;
                    default: // alle andere gevallen
                        valueBuilder.Append(Convert.ToChar(nextByte));
                        MimeState = PreviousMimeQuoted.NotMime;
                        break;
                }
                if (base.TypOfEnd == EndType.None) nextByte = await reader.ReadByteAsync();
            }
            
            return base.TypOfEnd;
        }

        private async Task ReadParameter(string key, BufferedByteReader reader)
        {
            switch (key)
            {
                case "boundary":
                    Boundary = await ReadBytesParameter(reader).ConfigureAwait(false);
                    break;
                case "charset":
                    Charset = await ReadStringParameter(reader).ConfigureAwait(false);
                    break;
                case "type":
                    TYPE = await ReadStringParameter(reader).ConfigureAwait(false);
                    break;
                case "padding":
                    Padding = await ReadNumberParameter(reader).ConfigureAwait(false);
                    break;
                case "number":
                    Number = await ReadNumberParameter(reader).ConfigureAwait(false);
                    break;
                case "total":
                    Total = await ReadNumberParameter(reader).ConfigureAwait(false);
                    break;
                case "id":
                    Id = await ReadStringParameter(reader).ConfigureAwait(false);
                    break;
                case "acces-type":
                    AccesType = (await ReadStringParameter(reader).ConfigureAwait(false)).ToLower();
                    break;
                case "expiration":
                    Expiration = await ReadDateParameter(reader).ConfigureAwait(false);
                    break;
                case "size":
                    Size = await ReadNumberParameter(reader).ConfigureAwait(false);
                    break;
                case "permission":
                    Permission = (await ReadStringParameter(reader).ConfigureAwait(false)).ToLower();
                    break;
                case "name":
                    Name = await ReadStringParameter(reader).ConfigureAwait(false);
                    break;
                case "site":
                    Site = await ReadStringParameter(reader).ConfigureAwait(false);
                    break;
                case "directory":
                    Directory = await ReadStringParameter(reader).ConfigureAwait(false);
                    break;
                case "mode":
                    Mode = (await ReadStringParameter(reader).ConfigureAwait(false)).ToLower();
                    break;
                case "server":
                    Server = await ReadStringParameter(reader).ConfigureAwait(false);
                    break;
                case "subject":
                    Subject = await ReadStringParameter(reader).ConfigureAwait(false);
                    break;
                default:
                    await ReadBytesParameter(reader);
                    break;
            }
            return;
        }
    }
}
