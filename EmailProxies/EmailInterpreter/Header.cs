using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Storage.Streams;

namespace PopMailDemo.EmailProxies.EmailInterpreter
{
    class Header
    {
        byte[] headerEnd = new byte[]{(byte)SpecialByte.CarriageReturn, (byte)SpecialByte.Linefeed};
        internal protected async Task ReadHeader(Email Mail, List<byte[]> EndStrings, IpDialog Ip)
        {
            var NameInt = new HeaderFieldName();
            var buffer = await Ip.ReadByte(); // vooruitlees omdat processFieldName 
                                                //een byte nodig heeft.
            while (buffer != headerEnd[0])
            {
                var fieldName = await NameInt.ProcessFieldName(buffer, Ip);
                switch (fieldName)
                {
                    case "From":
                        var from = new AddressList();
                        buffer = await from.ProcessStream(Ip);
                        Mail.From = from;
                        break;
                    case "Sender":
                        var sender = new AddressList();
                        buffer = await sender.ProcessStream(Ip);
                        Mail.Sender = sender.Adresses[0];
                        break;
                    //case "Reply-To":
                    //    ProcessAddressList(Dr);
                    //    break;
                    //case "To":
                    //    ProcessAddressList(Dr);
                    //    break;
                    //case "Cc":
                    //    ProcessAddressList(Dr);
                    //    break;

                    //case HeaderFieldType.Ignore:
                    //    ProcessIgnore(Dr);
                    //    break;
                    //case HeaderFieldType.DateTime:
                    //    ProcessDateTime(Dr);
                    //    break;
                    //case HeaderFieldType.MessageId:
                    //    ProcessMessageId(Dr);
                    //    break;
                    //case HeaderFieldType.Other:

                    default:

                        break;
                }
            }
        }
    }
}
