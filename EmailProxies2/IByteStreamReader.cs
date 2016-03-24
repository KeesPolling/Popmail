using System.Threading.Tasks;
using Windows.Storage.Streams;

namespace PopMail.EmailProxies
{
    public interface IByteStreamReader
    {
        Task<DataReader> GetStream(string Request);
        Task<byte> ReadByte();
    }
}
