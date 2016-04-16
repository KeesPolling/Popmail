using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using PopMail.EmailProxies;
using PopMail.EmailProxies.IP_helpers;

namespace PopMail.EmailProxies.EmailInterpreter
{
    public class AddressList : FieldValue
    {
        public class Adress
        {
            public string Name { get; set; }
            public string MailBox { get; set; }
        }
        public class Group
        {
            public Group()
            {
                this.Members = new List<Adress>();
            }
            public string Name { get; set; }
            public List<Adress> Members { get; set; }
        }
        public List<Group> Groups { get; set; }
        public List<Adress> Adresses { get; set; }

        public AddressList()
        {
            this.Groups = new List<Group>();
            this.Adresses = new List<Adress>();
        }

        private void AddressAdd(List<AddressList.Adress> list, AddressList.Adress address, StringBuilder builderValue)
        {
            var value = builderValue.ToString().Trim();
            if (String.IsNullOrEmpty(address.MailBox))
            {
                address.MailBox = value;
            }
            else
            {
                if (String.IsNullOrEmpty(address.Name))
                {
                    address.Name = value;
                }
                else
                {
                    address.Name = address.Name + value;
                }
            }
            if (!String.IsNullOrEmpty(address.MailBox))
            { 
                list.Add(address);
            }
        }
        private void GroupAdd(AddressList.Group group, AddressList.Adress address, StringBuilder builderValue)
        {
            // is also an end of address-spec
            AddressAdd(group.Members, address, builderValue);
            this.Groups.Add(group);
        }

        internal override async Task<EndType> ReadFieldValue(BufferedByteReader reader)
        {
            var valueBuilder = new StringBuilder();

            var eol = new Eol();
            var nextByte = await reader.ReadByte();

            var address = new AddressList.Adress();
            Group group = new Group();
            var endType = EndType.None;
            while ((endType == EndType.None))
            {
                switch (nextByte)
                {
                    case (byte)SpecialByte.CarriageReturn:
                        endType = await eol.ProcessEol(reader);
                        break;
                    case (byte)SpecialByte.Equals:
                        var resultString = await MimeQuotedString(reader);
                        if (resultString == "=")
                        {
                            MimeState = PreviousMimeQuoted.NotMime;
                            valueBuilder.Append('=');
                        }
                        else
                        {
                            if (MimeState == PreviousMimeQuoted.MimeQuoted)
                            {
                                if (valueBuilder.Length > 0 && valueBuilder[valueBuilder.Length - 1] == ' ')
                                {
                                    valueBuilder.Remove(valueBuilder.Length - 1, 1);
                                }
                            }
                            MimeState = PreviousMimeQuoted.MimeQuoted;
                            valueBuilder.Append(resultString).Append(' ');
                        }
                        break;
                    case (byte)SpecialByte.Space:
                        if (valueBuilder.Length > 0 && valueBuilder[valueBuilder.Length - 1] != ' ')
                        {
                            valueBuilder.Append(' ');
                        }
                        break;
                    case (byte)SpecialByte.LeftParenthesis: // "(": begin comment
                        await ReadComment(reader);
                        break;
                    case (byte)SpecialByte.BackSlash: // "\": begin "quoted character"
                        valueBuilder.Append(Convert.ToChar(nextByte));
                        MimeState = PreviousMimeQuoted.NotMime;
                        break;
                    case (byte)SpecialByte.Quote: //  """: begin quoted string
                        valueBuilder.Append(await ReadQuotedString(reader));
                        MimeState = PreviousMimeQuoted.NotMime;
                        break;
                    case (byte)SpecialByte.Colon: // ":": = end of group name
                        group.Name = valueBuilder.ToString().Trim();
                        valueBuilder = new StringBuilder();
                        MimeState = PreviousMimeQuoted.NotMime;
                        break;
                    case (byte)SpecialByte.LeftAngledBracket: // "<": begin email address  (mailbox)
                        address.Name = valueBuilder.ToString().Trim();
                        valueBuilder = new StringBuilder();
                        MimeState = PreviousMimeQuoted.NotMime;
                        break;
                    case (byte)SpecialByte.RightAngledBracket: // ">" : end of email address (mailbox)
                        address.MailBox = valueBuilder.ToString().Trim();
                        valueBuilder = new StringBuilder();
                        MimeState = PreviousMimeQuoted.NotMime;
                        break;
                    case (byte)SpecialByte.Comma: // "," : end of name-adress spec
                        AddressAdd
                            (
                                ((group.Name == null) ? this.Adresses : group.Members),
                                address,
                                valueBuilder
                            );
                        address = new Adress();
                        valueBuilder = new StringBuilder();
                        MimeState = PreviousMimeQuoted.NotMime;
                        break;
                    case (byte)SpecialByte.SemiColon: // ";" End of group
                        GroupAdd(group, address, valueBuilder);
                        group = new Group();
                        address = new Adress();
                        valueBuilder = new StringBuilder();
                        MimeState = PreviousMimeQuoted.NotMime;
                        break;
                    default: // alle andere gevallen
                        valueBuilder.Append(Convert.ToChar(nextByte));
                        MimeState = PreviousMimeQuoted.NotMime;
                        break;
                }
                if (endType == EndType.None) nextByte = await reader.ReadByte();
            }
            if (group.Name == null)
            {
                AddressAdd(this.Adresses, address, valueBuilder);
            }
            else
            {
                GroupAdd(group, address, valueBuilder);
            }
            return endType;
        }
    }
}
