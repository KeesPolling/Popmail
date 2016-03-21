using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PopMailDemo.EmailProxies.EmailInterpreter
{
    public class Email
    {
        public static Header _header;
        public string Provider { get; set; }
        public string ProviderID { get; set; }

        public List<string> BodyParts { get; set; }
    }
}
