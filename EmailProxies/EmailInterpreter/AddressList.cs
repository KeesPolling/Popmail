using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PopMailDemo.EmailProxies.EmailInterpreter
{
    public class AddressList
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
            public Group(string GroupName)
            {
                this.Name = GroupName;
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
            List.Add(Address);
            Address = new AddressList.Adress();
        }
        private void GroupAdd(ref AddressList.Group Group, AddressList.Adress Address, StringBuilder Value)
        {
            // is also an end of address-spec
            AddressAdd(Group.Members, Address, Value);
            this.Groups.Add(Group);
            Group = null;
        }

        internal async Task<byte> ProcessStream(IpDialog Ip)
        {
            var valueBuilder = new StringBuilder();

            var eol = new EOL();
            var nextByte = await Ip.ReadByte();

            var address = new AddressList.Adress();
            AddressList.Group group = null;
            while (!eol.End)
            {
                if (nextByte == (byte)SpecialByte.CarriageReturn)
                {
                    nextByte = await eol.ProcessEOL(Ip);
                    continue;
                }
                switch (nextByte)
                {
                    case (byte)SpecialByte.LeftParenthesis: // "(": begin comment
                        var comment = new Comment();
                        await comment.Process(Ip);
                        break;
                    case (byte)SpecialByte.Backslash: // "\": begin "quoted character"
                        nextByte = await Ip.ReadByte();
                        valueBuilder.Append(Convert.ToChar(nextByte));
                        break;
                    case (byte)SpecialByte.Quote: //  """: begin quoted string
                        // TODO quotedString ;

                        break;
                    case (byte)SpecialByte.Colon: // ":": = end of group name
                        group = new AddressList.Group("_valueBuilder.ToString().Trim()");
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
                                ((group == null) ? this.Adresses : group.Members),
                                address,
                                valueBuilder
                            );
                        break;
                    case (byte)SpecialByte.SemiColon: // ";" End of group
                    default: // alle andere gevallen
                        GroupAdd
                        (
                              ref group
                            , address
                            , valueBuilder
                        );
                        break;
                }
                nextByte = await Ip.ReadByte();
            }
            if (group == null)
            {
                AddressAdd(this.Adresses, address, valueBuilder);
            }
            else
            {
                GroupAdd(ref group, address, valueBuilder);
            }
            return nextByte;
        }
    }
}
