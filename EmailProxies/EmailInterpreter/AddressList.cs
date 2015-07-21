using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PopMailDemo.EmailProxies.EmailInterpreter
{
    public class AddressList : FieldValue
    {
        public AddressList()
        {
            this.Groups = new List<Group>();
            this.Adresses = new List<Adress>();
        }
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

        private void AddressAdd(List<AddressList.Adress> List, AddressList.Adress Address, StringBuilder Value)
        {
            var value = Value.ToString().Trim();
            if (String.IsNullOrEmpty(Address.MailBox))
            {
                Address.MailBox = value;
            }
            else
            {
                if (String.IsNullOrEmpty(Address.Name))
                {
                    Address.Name = value;
                }
                else
                {
                    Address.Name = Address.Name + value;
                }
            }
            if (!String.IsNullOrEmpty(Address.MailBox))
            { 
                List.Add(Address);
            }
        }
        private void GroupAdd(AddressList.Group Group, AddressList.Adress Address, StringBuilder Value)
        {
            // is also an end of address-spec
            AddressAdd(Group.Members, Address, Value);
            this.Groups.Add(Group);
        }

        internal async Task<byte> ReadAddressList(IByteStreamReader Reader)
        {
            var valueBuilder = new StringBuilder();

            var eol = new EOL();
            var nextByte = await Reader.ReadByte();

            var address = new AddressList.Adress();
            Group group = new Group();
            while (!eol.End)
            {
                if (nextByte == (byte)SpecialByte.CarriageReturn)
                {
                    nextByte = await eol.ProcessEOL(Reader);
                    continue;
                }
                switch (nextByte)
                {
                    case (byte)SpecialByte.LeftParenthesis: // "(": begin comment
                        await ReadComment(Reader);
                        break;
                    case (byte)SpecialByte.Backslash: // "\": begin "quoted character"
                        nextByte = await Reader.ReadByte();
                        valueBuilder.Append(Convert.ToChar(nextByte));
                        break;
                    case (byte)SpecialByte.Quote: //  """: begin quoted string
                        // TODO quotedString ;
                        valueBuilder.Append(await ReadQuotedString(Reader));
                        break;
                    case (byte)SpecialByte.Colon: // ":": = end of group name
                        group.Name = valueBuilder.ToString().Trim();
                        valueBuilder = new StringBuilder();
                        break;
                    case (byte)SpecialByte.LeftAngledBracket: // "<": begin email address  (mailbox)
                        address.Name = valueBuilder.ToString().Trim();
                        valueBuilder = new StringBuilder();
                        break;
                    case (byte)SpecialByte.RightAngledBracket: // ">" : end of email address (mailbox)
                        address.MailBox = valueBuilder.ToString().Trim();
                        valueBuilder = new StringBuilder();
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
                        break;
                    case (byte)SpecialByte.SemiColon: // ";" End of group
                        GroupAdd(group, address, valueBuilder);
                        group = new Group();
                        address = new Adress();
                        valueBuilder = new StringBuilder();
                        break;
                    default: // alle andere gevallen
                        valueBuilder.Append(Convert.ToChar(nextByte));
                        break;
                }
                nextByte = await Reader.ReadByte();
            }
            if (group.Name == null)
            {
                AddressAdd(this.Adresses, address, valueBuilder);
            }
            else
            {
                GroupAdd(group, address, valueBuilder);
            }
            return nextByte;
        }
    }
}
