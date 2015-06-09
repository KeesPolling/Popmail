//using PopMailDemo.MVVM.Model;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.ApplicationModel.Core;
using Windows.Networking.Sockets;
//using Windows.Networking;
//using Windows.Networking.Sockets;


namespace PopMailDemo.EmailProxies
{
    public class Pop3Proxy: IDisposable
    {
        //private StreamSocket socket;
        //private string providerName;
        //private string serviceName;
        //private HostName hostName;
        //private string accountName;
        //private string password;
        bool _disposed = false;
        IpDialog _socketDialog;
        Pop3Service _serviceProperties = new Pop3Service();
   
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

        public Pop3Proxy(string Name, string Uri, string Port, string AccountName, string Password)
        {

            _serviceProperties.Address = Uri;
            _serviceProperties.Name = Name;
            _serviceProperties.ServiceName = Port;
            _serviceProperties.AccountName = AccountName;
            _serviceProperties.Password = Password;
            if (!CoreApplication.Properties.ContainsKey(_serviceProperties.Name))
            {
                CoreApplication.Properties.Add(_serviceProperties.Name, null);
            }
        }

        public async Task Connect()
        {
            try
            {
                _socketDialog = new IpDialog();
                //connect
                var received = await _socketDialog.Start(_serviceProperties.AddressName, _serviceProperties.ServiceName);
                if(received.StartsWith("+OK"))
                {
                    CoreApplication.Properties[_serviceProperties.Name] = "connected";
                }
                else
                {
                    CoreApplication.Properties[_serviceProperties.Name] = null;
                    return;
                }
                //Login: username
                var sendstring = String.Format("USER {0}\r\n", _serviceProperties.AccountName);

               var answer = await _socketDialog.GetSingleLineResponse(sendstring);

                if (answer.StartsWith("+OK"))
                {
                    // login password
                    sendstring = String.Format("PASS {0}\r\n", _serviceProperties.Password);
                    answer = await _socketDialog.GetSingleLineResponse(sendstring);
                }
                if (!answer.StartsWith("+OK"))
                {
                    CoreApplication.Properties[_serviceProperties.Name] = "loginFailed";
                }
                else
                {
                    CoreApplication.Properties[_serviceProperties.Name] = "loggedIn";
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
        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public async Task<Dictionary<uint, uint>> LIST()
        {
            var mailItems = new Dictionary<uint, uint>();
            var sendstring = "LIST\r\n";

            var answer = await _socketDialog.GetMultiLineResponse(sendstring);
            string[] splitstrings = { "\r\n" };
            var items = answer.Split(splitstrings, StringSplitOptions.None);
            var separator = new char[] {' '};
            foreach (var item in items)
            {
                if (!(item.StartsWith("+OK") || (item == ".") || (item == "")))
                {
                    try
                    {
                        var Numbers = item.Split(separator, 2);
                        mailItems.Add
                        (
                            Convert.ToUInt32(Numbers[0], 10)
                            , Convert.ToUInt32(Numbers[1])
                        );
                    }
                    catch (System.FormatException)
                    { }
                }
            }
            return mailItems;
        }
        //private string ReadLine(MemoryStream MemStream)
        //{
        //    var lineBuilder = new StringBuilder();
        //    var bytesread = new Byte[2];
        //    var byteread = new Byte[1];
        //    byteread[0] = (byte)MemStream.ReadByte();
        //    while (MemStream.Position < MemStream.Length)
        //    {
        //        if (byteread[0] == 13)
        //        {
        //            bytesread[0] = byteread[0];
        //            bytesread[1] = (byte)MemStream.ReadByte();
        //            if (byteread[1] == 10)
        //            {
        //                break;
        //            }
        //            else
        //            {
        //                lineBuilder.Append(System.Text.Encoding.UTF8.GetChars(bytesread));
        //            }
        //        }
        //        else lineBuilder.Append(System.Text.Encoding.UTF8.GetChars((byte[])byteread));
        //    }
        //    return lineBuilder.ToString();
        //}
        public async Task<Dictionary<uint, string>> IdentifierList()
        {
            var  mailItems= new Dictionary<uint, string>();
            var sendstring = "UIDL\r\n";

            var answer = await _socketDialog.GetMultiLineResponse(sendstring);
            string[] splitstrings = { "\r\n" };
            var items = answer.Split(splitstrings, StringSplitOptions.None);
            var separator = new char[] { ' ' };
            if (items[0].StartsWith("-ERR"))
            {
                throw new Exception(items[0].Substring(5));
            }
            foreach (var item in items)
            {
                if (!(item.StartsWith("+OK") || (item == ".") || (item == "")))
                {
                    try
                    {
                        var Numbers = item.Split(separator, 2);
                        mailItems.Add
                        (
                            Convert.ToUInt32(Numbers[0], 10)
                            , Numbers[1]
                        );
                    }
                    catch (System.FormatException)
                    { }
                }
            }
            return mailItems;
        }

        public async Task Disconnect()
        {
            var sendstring = new StringBuilder("QUIT");
            sendstring.Append("\r\n");
            CoreApplication.Properties[_serviceProperties.Name] = null;
            var answer = await _socketDialog.GetSingleLineResponse(sendstring.ToString());
            _socketDialog.Dispose();
        }
        public virtual void Dispose(bool disposing)
        {
            if (_disposed) return;

            if (disposing)
            { 
                if (_socketDialog != null) _socketDialog.Dispose();
            }
            _disposed = true;
        }
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
    }
}

