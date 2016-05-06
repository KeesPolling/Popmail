using System;
using System.Threading.Tasks;
using Windows.Storage.Streams;

namespace PopMail.EmailProxies.IP_helpers
{
    public interface IByteStreamReader: IDisposable
    {
        bool MessageEnd { get; set; }
        byte[] EndBytes { get; set; }
        Task GetReaderAsync(string request);
        Task<byte> ReadByteAsync();
        void Dispose(bool disposing);
    }
}
