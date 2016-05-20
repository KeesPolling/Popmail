using System.Collections.Generic;
using System.Threading.Tasks;
using Windows.Storage.Streams;
using PopMail.EmailProxies.IP_helpers;
using System.IO;

namespace PopMail.EmailProxies
{
    public class BufferedByteReader: IByteStreamReader
    {
        private readonly List<byte> _bufferBytes = new List<byte>();
        private readonly IByteStreamReader _byteReader;

        public int BufferSize => _bufferBytes.Count;

        public bool MessageEnd
        {
            get { return _byteReader.MessageEnd; }
            set { _byteReader.MessageEnd = value; }
        }
        public byte[] EndBytes
        {
            get { return _byteReader.EndBytes; }
            set { _byteReader.EndBytes = value; }
        } 

        public BufferedByteReader(IByteStreamReader byteReader)
        {
            _byteReader = byteReader;
        }

        public async Task GetReaderAsync(string request)
        {
           await _byteReader.GetReaderAsync(request);
        }

        public async Task<byte> ReadByteAhead()
        {
            var byteRead = await _byteReader.ReadByteAsync();
            _bufferBytes.Add(byteRead);
            return byteRead;
        }

        public async Task<byte> ReadByteAsync()
        {
            if (_bufferBytes.Count == 0)
            {
                return await _byteReader.ReadByteAsync();
            }
            var byteRead = _bufferBytes[0];
            _bufferBytes.RemoveAt(0);
            return byteRead;
        }
        public async Task<MemoryStream> GetMemoryStreamAsync()
        {
            return await _byteReader.GetMemoryStreamAsync();
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
