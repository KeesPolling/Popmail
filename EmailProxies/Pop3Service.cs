using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Networking.Sockets;
using Windows.Networking;

namespace EmailProxies
{
    public class Pop3Service
    {   
        private StreamSocket _socket;
        internal HostName AddressName{ get; set;}
        public string Name{ get; set; }
        public string AccountName { get; set; }
        public string Password { get; set; }
        public string Address
        {
            get
            {
                return AddressName.RawName;
            }
            set
            {
                AddressName = new HostName(Address);
            }
        }
        public string ServiceName { get; set; }
    }
}
