using System.Collections.Generic;
using System.Threading.Tasks;
using Windows.Storage.Streams;
using PopMail.EmailProxies.IP_helpers;

namespace PopMail.EmailProxies
{
    public class BufferedByteReader: IByteStreamReader
    {
        private readonly List<byte> _bufferBytes = new List<byte>();
        private readonly IByteStreamReader _byteReader;

        public int BufferSize => _bufferBytes.Count;

        public BufferedByteReader(IByteStreamReader byteReader)
        {
            _byteReader = byteReader;
        }

        public async Task<DataReader> GetStream(string request)
        {
            return await _byteReader.GetStream(request);
        }

        public async Task<byte> ReadByteAhead()
        {
            var byteRead = await _byteReader.ReadByte();
            _bufferBytes.Add(byteRead);
            return byteRead;
        }

        public async Task<byte> ReadByte()
        {
            if (_bufferBytes.Count == 0)
            {
                return await _byteReader.ReadByte();
            }
            var byteRead = _bufferBytes[0];
            _bufferBytes.RemoveAt(0);
            return byteRead;
        }

        public void Clear()
        {
            _bufferBytes.Clear();
        }

        public void RemoveFirst(int length)
        {
            _bufferBytes.RemoveRange(0, length);
        }

        public void Dispose()
        {
            _byteReader.Dispose();
        }

        public void Dispose(bool disposing)
        {
            _byteReader.Dispose(disposing);
        }
    }
}
