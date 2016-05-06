using System;
using System.IO;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Windows.Data.Xml.Dom;
using Windows.Networking;
using Windows.Networking.Sockets;
using Windows.Storage;
using Windows.Storage.Streams;
using Windows.ApplicationModel.Core;
using PopMail.EmailProxies;


namespace PopMail.EmailProxies.IP_helpers
{
    /// <summary>
    /// This class is used for sending a request and receiving a response
    /// over the internet.
    /// minBufferSize and maxBufferSize are read from a XML configration file
    /// </summary>
    public class IpDialog: IByteStreamReader, IDisposable 
    {
        private bool _disposed;
        private uint _minBufferSize;
        private uint _maxBufferSize;
        private bool _logResponse;
  //      uint _buffersize;
        private StreamSocket _streamSocket;
        private DataReader _dataReader;
        private DataWriter _dataWriter;
        private IOutputStream _logStream;
        private DataWriter _logWriter;
        //        StreamWriter _memWriter;

        internal DataReader Reader => _dataReader;

        public bool MessageEnd { get; set; }

        private List<byte> _buffer ;
        
        #region EndBytes;
        private byte[] _endBytes;
        public byte[] EndBytes
        {
            get { return _endBytes; }
            set
            {
                _endBytes = value;
                _endString = System.Text.Encoding.ASCII.GetString(value);
            }
        }
        #endregion
        #region EndString
        private string _endString;
        public string EndString
        {
            get { return _endString; }
            set
            {
                _endString = value;
                _endBytes = Encoding.ASCII.GetBytes(value);
            }
        }
        #endregion
        /// <summary>
        /// The constructor sets some parameters to values that are a best
        /// match for a Pop3 service.
        /// </summary>
        #region Constructor
        public IpDialog()
        {
            _streamSocket = new StreamSocket();
            _dataReader = new DataReader(_streamSocket.InputStream);
            _dataReader.UnicodeEncoding = Windows.Storage.Streams.UnicodeEncoding.Utf8;
            _dataReader.InputStreamOptions = InputStreamOptions.Partial;
            _dataWriter = new DataWriter(_streamSocket.OutputStream);
            _dataWriter.UnicodeEncoding = Windows.Storage.Streams.UnicodeEncoding.Utf8;
            // Assure configurable values are set
            if (_minBufferSize == 0)
            {
                LoadConfiguredValues();
            }

        }

        /// <summary>
        /// Reads designtime configurable values from an XML file.
        /// </summary>
        /// <returns></returns>
        public void LoadConfiguredValues()
        {
            var values = (new Settings()).GetIpSettings();
            _minBufferSize = (uint)values["MinBufferSize"];
            _maxBufferSize = (uint)values["MaxBufferSize"];
            _logResponse = (bool)values["LogResponse"];
        }
        #endregion Constructor

        
        #region Start dialog
        /// <summary>
        /// Connects to a Pop3 server
        /// </summary>
        /// <param name="targetAddres">Ip adress of the server</param>
        /// <param name="portName">Port adress of the service</param>
        /// <returns>Welcome text from the service upon establishing a connection</returns>
        public async Task<string> Start(HostName targetAddres, string portName)
        {
            var received = "";
            try
            {
                await _streamSocket.ConnectAsync(targetAddres, portName);

                var count = await _dataReader.LoadAsync(_minBufferSize);
                if (count > 0)
                {
                    received = _dataReader.ReadString(count);
                }
            }
            catch (Exception exception)
            {
                switch (SocketError.GetStatus(exception.HResult))
                {
                    case SocketErrorStatus.AddressAlreadyInUse:
                        // TODO  handle AddressAlreadyInUSe
                        break;
                    case SocketErrorStatus.AddressFamilyNotSupported:
                    // TODO handle AddressFamilyNotSupported
                        break;
                    case SocketErrorStatus.ConnectionRefused:
                        //TODO handle conenctionRefused
                        break;
                    case SocketErrorStatus.HostIsDown:
                        //TODO handle HostIsDown
                        break;
                    default:
                        var message = exception.Message;
                        break;
                }
            }
            return received;
        }
        #endregion Start dialog
        #region Get answers
        private async Task SendRequest(string Request)
        {
        _dataWriter.WriteString(Request);
        await _dataWriter.StoreAsync();
        await _dataWriter.FlushAsync();
        }
        /// <summary>
        /// Sends a request to the service
        /// </summary>
        /// <param name="request">a valid request</param>
        /// <returns>the response from the server</returns>
        public async Task<string> GetSingleLineResponse(string request)
        {
            var received = new StringBuilder();
            var bufferSize = _minBufferSize;
            try
            {
                await SendRequest(request);
                var count = await _dataReader.LoadAsync(bufferSize);
                received.Append(_dataReader.ReadString(count));
                return received.ToString();
            }
            catch (Exception)
            {
                
                throw;
            }
        }

        /// <summary>
        /// Sends a request to the service
        /// </summary>
        /// <param name="request">a valid request</param>
        /// <returns>the response from the server</returns>
        public async Task<string> GetMultiLineResponse(string request)
        {
//            var received = new StringBuilder();
            var bufferSize = _minBufferSize;
            try
            {
                //await SendRequest(request);

                //var count = await _dataReader.LoadAsync(bufferSize);
                //received.Append(_dataReader.ReadString(count));
                //while (!received.ToString().EndsWith(_endString, StringComparison.Ordinal))
                //{
                //    bufferSize = bufferSize * 4;
                //    if (bufferSize > _maxBufferSize)
                //    {
                //        bufferSize = _maxBufferSize;
                //    }
                //    count = await _dataReader.LoadAsync(bufferSize);
                //    received.Append(_dataReader.ReadString(count));
                //}
                //return received.ToString();
                await GetReaderAsync(request);
                var str = await GetMemoryStreamAsync();
                return Encoding.ASCII.GetString(str.ToArray());
            }
            catch (Exception)
            {

                throw;
            }
        }
        
        /// <summary>
        /// Sends a request to the service
        /// </summary>
        /// <param name="request">a valid request</param>
        /// <returns>the response from the server</returns>
        public async Task GetReaderAsync(string request)
        {
            var bufferSize = _minBufferSize;
            try
            {
                if (_logResponse)
                {

                    var logFileName = string.Format("{0}.log", request.Substring(0, request.Length - 2));
                    var logFile = await DownloadsFolder.CreateFileAsync(logFileName, CreationCollisionOption.GenerateUniqueName);
                    var logStream = await logFile.OpenAsync(FileAccessMode.ReadWrite);
                    _logStream = logStream.GetOutputStreamAt(0);
                    _logWriter = new DataWriter(_logStream);
                    await SendRequest(request);
                }
                var count = await _dataReader.LoadAsync(bufferSize);
            }
            catch (Exception)
            {

                throw;
            }
        }

        public async Task<byte> ReadByteAsync()
        {
            var restLength = _dataReader.UnconsumedBufferLength;
            var nextByte = (byte)0;
            var endByteCounter = (_endBytes == null ? 0 :_endBytes.Length);
            if (_endBytes != null)
            {
                while (restLength > 0 && restLength == endByteCounter)
                {
                    nextByte = _dataReader.ReadByte();
                    if (_buffer == null) _buffer = new List<byte>();
                    _buffer.Add(nextByte);
                    restLength -= 1;
                    if (_endBytes[_endBytes.Length - endByteCounter] == nextByte)
                        endByteCounter -= 1;
                }
                if (endByteCounter == 0)
                {
                    MessageEnd = true;
                    if (_logResponse)
                    {
                        _logWriter.WriteBytes (_buffer.ToArray());
                        await _logWriter.StoreAsync();
                        await _logStream.FlushAsync();
                        _logStream.Dispose();
                    }
                    return 0;
                }
                if (_buffer?[0] != null)
                {
                    endByteCounter = _endBytes.Length;
                    var bufferByte = _buffer[0];
                    _buffer.RemoveAt(0);
                    if (_logResponse) _logWriter.WriteByte(bufferByte);
                    return bufferByte;
                }
            }
            if (restLength != 0) return _dataReader.ReadByte();


            var bufferSize = _minBufferSize;
            try
            {
                var count = await _dataReader.LoadAsync(bufferSize);
            }
            catch (Exception)
            {
                throw;
            }
            nextByte = _dataReader.ReadByte();
            if (_logResponse) _logWriter.WriteByte(nextByte);
            return nextByte;
        }
        #region GetMemoryStream
        public async Task<MemoryStream> GetMemoryStreamAsync()
        {
            if (_endBytes == null)
            {
                throw new NullReferenceException("No end of message defined");
            }
            var received = new MemoryStream();
            var bufferSize =_minBufferSize;
            var restLength = _dataReader.UnconsumedBufferLength;
            if  (restLength == 0) restLength = await _dataReader.LoadAsync(bufferSize);
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
                        if (_logResponse) _logWriter.WriteByte(posValue);
                    }
                }
                Array.Copy(value, (nrOfBytesToRead - _endBytes.Length), valueEnd, 0, _endBytes.Length);
                await memStream.WriteAsync(value, 0, nrOfBytesToRead - valueEnd.Length);
                if (_logResponse)
                    Array.Copy(value, bytesToWrite, nrOfBytesToRead - valueEnd.Length);
                    _logWriter.WriteBytes(bytesToWrite);
            }
            else  CopyInEnd(value, valueEnd, null);
            if (CompareArrays(valueEnd, _endBytes))
            {
                if (_logResponse)
                {
                    _logWriter.WriteBytes(valueEnd);
                    await _logWriter.StoreAsync();
                    await _logStream.FlushAsync();
                    _logStream.Dispose();
                }
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
        #endregion

        #region  Dispose
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
                _disposed = true;
                _dataReader?.Dispose();
                _dataWriter?.Dispose();
                _streamSocket?.Dispose();
            }
        }
        #endregion
    }
}
