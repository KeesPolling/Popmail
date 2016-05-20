using System;
using System.IO;
using System.Collections.Generic;
using System.Threading.Tasks;
using Windows.Storage.Streams;
using PopMail.EmailProxies;
using Windows.Storage;
using PopMail.EmailProxies.IP_helpers;
using System.Threading;

namespace PopMail.UnitTests
{
    public class FileByteReader : IByteStreamReader, IDisposable
    {
        private bool _disposed;
        private uint _minBufferSize = 1042;
        private uint _maxBufferSize = 65000;
        private DataReader _dataReader;


        private List<byte> _buffer;
        private string _endString;
        private byte[] _endBytes;
        private int _endByteCounter;
        public bool MessageEnd { get; set; }

        public byte[] EndBytes
        {
            get { return _endBytes; }
            set
            {
                _endBytes = value;
                _endString = System.Text.Encoding.ASCII.GetString(value);
                _endByteCounter = _endBytes.Length;
            }
        }
        public string Endstring
        {
            get { return _endString; }
            set
            {
                _endString = value;
                _endBytes = System.Text.Encoding.ASCII.GetBytes(value);
            }
        }

        public async Task GetReaderAsync(string Request)
        {
            var storageFolder =  ApplicationData.Current.LocalFolder;
            StorageFile testFile = await storageFolder.GetFileAsync(Request);

            var bufferSize = (UInt32)(await testFile.GetBasicPropertiesAsync()).Size;
             try
            {
                var buffer = await FileIO.ReadBufferAsync(testFile);
                _dataReader = DataReader.FromBuffer(buffer);
            }
            catch (Exception ex)
            {
                
                throw;
            }
        }
        public async Task<byte> ReadByteAsync()
        {
            var restLength = _dataReader.UnconsumedBufferLength;

            if (_endBytes != null)
            {
                while (restLength > 0 && restLength == _endByteCounter)
                {
                    var nextByte = _dataReader.ReadByte();
                    if (_buffer == null) _buffer = new List<byte>();
                    _buffer.Add(nextByte);
                    restLength -= 1;
                    if (_endBytes[_endBytes.Length - _endByteCounter] == nextByte)
                        _endByteCounter -= 1;
                }
                if (_endByteCounter == 0)
                {
                    MessageEnd = true;
                    return 0;
                }
                if (_buffer?[0] != null)
                {
                    _endByteCounter = _endBytes.Length;
                    var bufferByte = _buffer[0];
                    _buffer.RemoveAt(0);
                    return bufferByte;
                }
            }

            if (restLength != 0) return _dataReader.ReadByte();

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
        #region GetMemoryStream
        public async Task<MemoryStream> GetMemoryStreamAsync()
        {
            if (_endBytes == null)
            {
                throw new NullReferenceException("No end of message defined");
            }
            var received = new MemoryStream();
            var bufferSize = _minBufferSize;
            var restLength = _dataReader.UnconsumedBufferLength;
            if (restLength == 0)
                restLength = await _dataReader.LoadAsync(bufferSize);
            var valueEnd = new byte[_endBytes.Length];
            var value = new byte[restLength];
            try
            {

                var atEnd = await FillReceived((int)restLength, valueEnd, received);
                while (!atEnd)
                {
                    bufferSize = bufferSize * 4;
                    if (bufferSize > _maxBufferSize)
                    {
                        bufferSize = _maxBufferSize;
                    }
                    restLength = await _dataReader.LoadAsync(bufferSize);
                    atEnd = await FillReceived((int)restLength, valueEnd, received);
                }
                return received;
            }
            catch (Exception)
            {

                throw;
            }
        }
        private async Task<bool> FillReceived(int nrOfBytesToRead, byte[] valueEnd, MemoryStream memStream)
        {
            var value = new byte[nrOfBytesToRead];
            var bytesToWrite = new byte[nrOfBytesToRead - valueEnd.Length];
            _dataReader.ReadBytes(value);
            if (nrOfBytesToRead >= _endBytes.Length)
            {
                foreach (var posValue in valueEnd)
                {
                    if (posValue > 0)
                    {
                        memStream.WriteByte(posValue);
                    }
                }
                Array.Copy(value, (nrOfBytesToRead - _endBytes.Length), valueEnd, 0, _endBytes.Length);
                await memStream.WriteAsync(value, 0, nrOfBytesToRead - valueEnd.Length);
            }
            else CopyInEnd(value, valueEnd, null);
            if (CompareArrays(valueEnd, _endBytes))
            {
                return true;  // end of message
            }
            return false;
        }
        /// <summary>
        /// An ordinal comparison of two byte arrays
        /// </summary>
        /// <param name="oneArray"></param>
        /// <param name="otherArray"></param>
        /// <returns>true if the same, otherwise false</returns>
        private bool CompareArrays(byte[] oneArray, byte[] otherArray)
        {
            if (oneArray.Length > otherArray.Length)
                throw new ArgumentOutOfRangeException("otherArray", "Arrays must have same length");

            for (var i = 0; i < oneArray.Length; i++)
            {
                if (oneArray[i] != otherArray[i]) return false;
            }
            return true;
        }
        /// <summary>
        ///  shifts the values of the target array to make place for the
        ///  values of the source array at the end.
        ///  values that fall out of the target array are written to the end of
        ///  the stream if the stream is not null
        /// </summary>
        /// <param name="source"></param>
        /// <param name="target"></param>
        /// <param name="stream"></param>
        /// <returns>the modified target array</returns>
        private byte[] CopyInEnd(byte[] source, byte[] target, MemoryStream stream)
        {
            if (source.Length > target.Length) throw new ArgumentOutOfRangeException("source", "source cannot be longer than target");
            var d = target.Length - source.Length;
            var s = source.Length;
            for (var i = 0; i < target.Length; i++)
            {
                if (i >= s) target[i - s] = target[i];
                else stream?.WriteByte(target[i]);
                if (i >= d) target[i] = source[i - d];
            }
            return target;
        }

        #endregion


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
