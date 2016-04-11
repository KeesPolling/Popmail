using System;
using System.Threading.Tasks;
using Windows.Storage.Streams;

namespace PopMail.EmailProxies.IP_helpers
{
    public interface IByteStreamReader: IDisposable
    {
        Task<DataReader> GetStream(string request);
        Task<byte> ReadByte();
        void Dispose(bool disposing);
    }
}
