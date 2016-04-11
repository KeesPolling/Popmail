using System;
using System.Threading.Tasks;
using Windows.Storage.Streams;
using PopMail.EmailProxies;
using Windows.Storage;
using PopMail.EmailProxies.IP_helpers;

namespace PopMail.UnitTests
{
    public class FileByteReader : IByteStreamReader, IDisposable
    {
        bool _disposed;
        uint _minBufferSize = 1042;
        DataReader _dataReader;
        public async Task<DataReader> GetStream(string Request)
        {
            var storageFolder =  ApplicationData.Current.LocalFolder;
            StorageFile testFile = await storageFolder.GetFileAsync(Request);

            var bufferSize = (UInt32)(await testFile.GetBasicPropertiesAsync()).Size;
             try
            {
                var buffer = await FileIO.ReadBufferAsync(testFile);
                _dataReader = DataReader.FromBuffer(buffer);
                return _dataReader;
            }
            catch (Exception ex)
            {
                
                throw;
            }
        }
        public async Task<byte> ReadByte()
        {
            if (_dataReader.UnconsumedBufferLength == 0)
            {
                var bufferSize = _minBufferSize;
                try
                {
                    await _dataReader.LoadAsync(bufferSize);
                }
                catch (Exception)
                {
                    throw;
                }
            }
            return _dataReader.ReadByte();
        }
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
        public virtual void Dispose(bool disposing)
        {
            if (_disposed) return;
            if (disposing)
            { 
                    _dataReader?.Dispose();
            }
            _disposed = true;
        }
    }
}
