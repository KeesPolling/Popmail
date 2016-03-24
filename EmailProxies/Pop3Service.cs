
using Windows.Networking;

namespace PopMail.EmailProxies
{
    public class Pop3Service
    {   
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
                AddressName = new HostName(value);
            }
        }
        public string ServiceName { get; set; }
    }
}
