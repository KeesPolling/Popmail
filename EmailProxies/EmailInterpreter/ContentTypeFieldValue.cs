using System;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Xaml.Documents;

namespace PopMail.EmailProxies.EmailInterpreter
{
    public class ContentTypeFieldValue: MimeFieldFieldValue
    {
        public string Type { get; set; }
        public string SubType { get; set; }
        public string Charset { get; set; }
        public byte[] Boundary { get; set; }
          
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

            while (EndType == EndType.None)
            {
                switch (nextByte)
                {
                    case (byte)SpecialByte.CarriageReturn:
                        EndType = await ProcessEol(reader);
                        break;
                    case (byte)SpecialByte.Equals:
                        key = valueBuilder.ToString().Trim();
                        EndType = await ReadParameter(key, reader);
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
                if (EndType == EndType.None) nextByte = await reader.ReadByteAsync();
            }
            
            return EndType;
        }

        private async Task<EndType> ReadParameter(string key, BufferedByteReader reader)
        {
            switch (key)
            {
                case "boundary":
                    Boundary = await ReadBytesParameter(reader);
                    break;
                case "charset":
                    Charset = await ReadStringParameter(reader);
                    break;
                case "TYPE":
                case "PADDING":
                case "ACCES-TYPE":
                case "EXPIRATION":
                case "SIZE":
                case "PERMISSION":
                case "NAME":
                case "SITE":
                case "DIRECTORY":
                case "MODE":
                case "SERVER":
                case "SUBJECT":
                default:
                    await ReadBytesParameter(reader);
                    break;
            }
            return base.EndType;
        }
    }
}
