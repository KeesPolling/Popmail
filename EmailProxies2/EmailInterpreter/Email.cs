using System.Collections.Generic;

namespace PopMail.EmailProxies.EmailInterpreter
{
    public class Email
    {
        public static Header Header;
        public string Provider { get; set; }
        public string ProviderID { get; set; }

        public List<string> BodyParts { get; set; }
    }
}
