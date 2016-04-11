using System;
using System.Text;
using System.Threading.Tasks;
using Windows.Data.Xml.Dom;
using Windows.Networking;
using Windows.Networking.Sockets;
using Windows.Storage;
using Windows.Storage.Streams;

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
  //      uint _buffersize;
        private StreamSocket _streamSocket;
        private DataReader _dataReader;
        private DataWriter _dataWriter;
//        StreamWriter _memWriter;

        internal DataReader Reader => _dataReader;
            
   
        private async Task SendRequest(string Request)
        {
            _dataWriter.WriteString(Request);
            await _dataWriter.StoreAsync();
            await _dataWriter.FlushAsync();
        }
        /// <summary>
        /// The constructor sets some parameters to values that are a best
        /// match for a Pop3 service.
        /// </summary>
        public IpDialog()
        {
            _streamSocket = new StreamSocket();
            _dataReader = new DataReader(_streamSocket.InputStream);
            _dataReader.UnicodeEncoding = Windows.Storage.Streams.UnicodeEncoding.Utf8;
            _dataReader.InputStreamOptions = InputStreamOptions.Partial;
            _dataWriter = new DataWriter(_streamSocket.OutputStream);
            _dataWriter.UnicodeEncoding = Windows.Storage.Streams.UnicodeEncoding.Utf8;
        }
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
                // Assure configurable values are set
                if (_minBufferSize == 0)
                {
                    await LoadConfiguredValues();
                }

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
        /// <summary>
        /// Reads designtime configurable values from an XML file.
        /// </summary>
        /// <returns></returns>
        public async Task LoadConfiguredValues()
        {
            try
            {
                var fileName = new Uri(String.Format("ms-appx:///Assets/Configuration/{0}", "All.xml"));
                StorageFile file = await StorageFile.GetFileFromApplicationUriAsync
                (
                    fileName    
                );
                XmlDocument xmlConfiguration = await XmlDocument.LoadFromFileAsync(file);

                // Set _minbuffersize
                IXmlNode node = xmlConfiguration.DocumentElement.SelectSingleNode
                (
                    "./appSettings/add[@key='minBufferSize']/@value"
                );
                _minBufferSize =  Convert.ToUInt32(node?.NodeValue ?? "1023");

                // Set _maxbuffersize
                node = xmlConfiguration.DocumentElement.SelectSingleNode
                    (
                        "./appSettings/add[@key='maxBufferSize']/@value"
                    );
                _maxBufferSize = Convert.ToUInt32(node?.NodeValue ?? "64001"); 
            }
            catch (System.IO.FileNotFoundException) 
            {
                _minBufferSize = 1024;
                _maxBufferSize = 64000;
            }

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
            var received = new StringBuilder();
            var bufferSize = _minBufferSize;
            try
            {
                await SendRequest(request);
                var count = await _dataReader.LoadAsync(bufferSize);
                received.Append(_dataReader.ReadString(count));
                while (!received.ToString().EndsWith("\r\n.\r\n", StringComparison.Ordinal))
                {
                    bufferSize = bufferSize * 4;
                    if (bufferSize > _maxBufferSize)
                    {
                        bufferSize = _maxBufferSize;
                    }
                    count = await _dataReader.LoadAsync(bufferSize);
                    received.Append(_dataReader.ReadString(count));
                }
                return received.ToString();
            }
            catch (Exception)
            {

                throw;
            }
        }        /// <summary>
        /// Sends a request to the service
        /// </summary>
        /// <param name="request">a valid request</param>
        /// <returns>the response from the server</returns>
        public async Task<DataReader> GetStream(string request)
        {
            var bufferSize = _minBufferSize;
            try
            {
                await SendRequest(request);
                var count = await _dataReader.LoadAsync(bufferSize);
                return _dataReader;
            }
            catch (Exception)
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
                    var count = await _dataReader.LoadAsync(bufferSize);
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
                _disposed = true;
                _dataReader?.Dispose();
                _dataWriter?.Dispose();
                _streamSocket?.Dispose();
            }
        }
    }
}
