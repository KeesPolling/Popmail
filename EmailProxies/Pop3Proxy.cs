//using PopMailDemo.MVVM.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.ApplicationModel.Core;
//using Windows.Networking;
//using Windows.Networking.Sockets;


namespace EmailProxies
{
    public class Pop3Proxy
    {
        //private StreamSocket socket;
        //private string providerName;
        //private string serviceName;
        //private HostName hostName;
        //private string accountName;
        //private string password;
        private IpDialog _socketDialog;
        private Pop3Service _pop3Service;

        public class Pop3Exception : System.Exception
        {
            public Pop3Exception(string str) : base(str)
            {
            }
        };

        public class Pop3Message
        {
            public long Number {get;set;}
            public long Bytes{get; set;}
            public bool Retrieved{get; set;}
            public string Message{get; set;}
        }

        public Pop3Proxy(Pop3Service Pop3Service)
        {
            if (!CoreApplication.Properties.ContainsKey(Pop3Service.Name))
            {
                CoreApplication.Properties.Add(Pop3Service.Name, null);
                Po
            }
        }

        public async Task Connect()
        {
            try
            {
                _socketDialog = new IpDialog();
                //connect
                var received = await _socketDialog.Start(_provider., serviceName);
                if(received.StartsWith("+OK"))
                {
                    CoreApplication.Properties[providerName] = "connected";
                }
                else
                {
                    CoreApplication.Properties[providerName] = null;
                    return;
                }
                //Login: username
                var sendstring = new StringBuilder("USER ");
                sendstring.Append(accountName);
                sendstring.Append("\r\n");

               var answer = await _socketDialog.GetResponse(sendstring.ToString());

                if (answer.StartsWith("+OK"))
                {
                    // login password
                    sendstring = new StringBuilder("PASS ");
                    sendstring.Append(password);
                    sendstring.Append("\r\n");
                    answer = await _socketDialog.GetResponse(sendstring.ToString());
                }
                if (!answer.StartsWith("+OK"))
                {
                    CoreApplication.Properties[providerName] = null;
                }
                else
                {
                    CoreApplication.Properties[providerName] = "loggedin";
                }
            }
            catch (Exception exception)
            {
                // If this is an unknown status it means that the error is fatal and retry will likely fail.
                if (SocketError.GetStatus(exception.HResult) == SocketErrorStatus.Unknown)
                {
                    throw;
                }

                //rootPage.NotifyUser("Connect failed with error: " + exception.Message, NotifyType.ErrorMessage);
            }
        }

        public async Task<Dictionary<uint, uint>> MessageList()
        {
            var mailItems = new Dictionary<uint, uint>();
            var sendstring = new StringBuilder("LIST");
            sendstring.Append("\r\n");

            var answer = await _socketDialog.GetResponse(sendstring.ToString());
            string[] splitstrings = { "\r\n" };
            var items = answer.Split(splitstrings, StringSplitOptions.None);
            var separator = 0;
            foreach (var item in items)
            {
                if (!(item.StartsWith("+OK") || (item == ".") || (item == "")))
                {
                    separator = item.IndexOf(" ", StringComparison.Ordinal);
                    mailItems.Add
                        (
                            Convert.ToUInt32(item.Substring(0, separator), 10)
                            , Convert.ToUInt32(item.Substring(separator + 1))
                        );
                }
            }
            return mailItems;
        }

        public async Task<Dictionary<uint, string>> IdentifierList()
        {
            var  mailItems= new Dictionary<uint, string>();
            var sendstring = new StringBuilder("UIDL");
            sendstring.Append("\r\n");

            var answer = await _socketDialog.GetResponse(sendstring.ToString());
            string[] splitstrings = {"\r\n"};
            var items = answer.Split(splitstrings, StringSplitOptions.None);
            var separator = 0;
            foreach (var item in items.Where(item => !(item.StartsWith("+OK") || (item == ".") || (item == ""))))
            {
                separator = item.IndexOf(" ", StringComparison.Ordinal);
                mailItems.Add(Convert.ToUInt32(item.Substring(0,  separator), 10), item.Substring(separator + 1));
            }
            return mailItems;
        }

        public async Task Disconnect()
        {
            var sendstring = new StringBuilder("QUIT");
            sendstring.Append("\r\n");
            CoreApplication.Properties[providerName] = null;
            var answer = await _socketDialog.GetResponse(sendstring.ToString());
            _socketDialog.Dispose();
        }
    }
}

