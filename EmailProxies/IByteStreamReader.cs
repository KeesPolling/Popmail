using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Storage.Streams;

namespace PopMailDemo.EmailProxies
{
     public interface IByteStreamReader
    {
        Task<DataReader> GetStream(string Request);
        Task<byte> ReadByte();
    }
}
