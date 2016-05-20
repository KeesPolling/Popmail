using System;
using System.Threading.Tasks;
using System.IO;

namespace PopMail.EmailProxies.IP_helpers
{
    public interface IByteStreamReader: IDisposable
    {
        bool MessageEnd { get; set; }
        byte[] EndBytes { get; set; }
        Task GetReaderAsync(string request);
        Task<byte> ReadByteAsync();
        Task<MemoryStream> GetMemoryStreamAsync();
        void Dispose(bool disposing);
    }
}
