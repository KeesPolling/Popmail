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
