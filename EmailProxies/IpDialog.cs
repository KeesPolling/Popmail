using System;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using Windows.Data.Xml.Dom;
using Windows.Networking;
using Windows.Networking.Sockets;
using Windows.Storage;
using Windows.Storage.Streams;

namespace EmailProxies
{
    /// <summary>
    /// This class is used for sending a request and receiving a response
    /// over the internet.
    /// minBufferSize and maxBufferSize are read from a XML configration file
    /// </summary>
    class IpDialog: IDisposable
    {
        private uint _minBufferSize;
        private uint _maxBufferSize;
        private StreamSocket _streamSocket;
        private DataReader _dataReader;
        private DataWriter _dataWriter;

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

        public void Dispose()
        {
            _dataReader.DetachStream();
            _dataReader.Dispose();
            _dataWriter.DetachStream();
            _dataWriter.Dispose();
            _streamSocket.Dispose();
        }
        /// <summary>
        /// Connects to a Pop3 server
        /// </summary>
        /// <param name="TargetAddres">Ip adress of the server</param>
        /// <param name="PortName">Port adress of the service</param>
        /// <returns>Welcome text from the service upon establishing a connection</returns>
        public async Task<string> Start(HostName TargetAddres, string PortName)
        {
            var received = "";
            try
            {
                // Assure configurable values are set
                if (_minBufferSize == 0)
                {
                    await LoadConfiguredValues();
                }

                await _streamSocket.ConnectAsync(TargetAddres, PortName);

                var count = await _dataReader.LoadAsync(_minBufferSize);
                if (count > 0)
                {
                    received = _dataReader.ReadString(count);
                }
                return received;
            }
            catch (Exception)
            {
                throw;
            }
        }
        /// <summary>
        /// Reads designtime configurable values from an XML file.
        /// </summary>
        /// <returns></returns>
        public async Task LoadConfiguredValues()
        {
            StorageFile file = await StorageFile.GetFileFromApplicationUriAsync
            (
                new Uri(String.Format("ms-appx:///Assets/Configuration/{0}", "All"))
            );
            XmlDocument xmlConfiguration = await XmlDocument.LoadFromFileAsync(file);
            
            // Set _minbuffersize
            IXmlNode node = xmlConfiguration.DocumentElement.SelectSingleNode
            (
                "./appSettings/add[@key='minBufferSize']/@value"
            );
            _minBufferSize = (node == null) ? (uint)1023 : (uint)node.NodeValue;

            // Set _maxbuffersize
            node = xmlConfiguration.DocumentElement.SelectSingleNode
                (
                    "./appSettings/add[@key='maxBufferSize']/@value"
                );
            _maxBufferSize = (node == null) ? (uint)64001: (uint)node.NodeValue;
     
        }
        /// <summary>
        /// Sends a request to the service
        /// </summary>
        /// <param name="request">a valid request</param>
        /// <returns>the response from the server</returns>
        public async Task<string> GetResponse(string request)
        {
            var received = new StringBuilder();
            var bufferSize = _minBufferSize;
            try
            {
                _dataWriter.WriteString(request);
                await _dataWriter.StoreAsync();
                await _dataWriter.FlushAsync();

                var count = await _dataReader.LoadAsync(bufferSize);
                while (count == bufferSize)
                {
                    bufferSize = bufferSize * 4;
                    if (bufferSize > _maxBufferSize)
                    {
                        bufferSize = _maxBufferSize;
                    }
                    received.Append(_dataReader.ReadString(count));
                    count = await _dataReader.LoadAsync(bufferSize);
                }
                if (count > 0)
                {
                    received.Append(_dataReader.ReadString(count));
                }
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
        public async Task<MemoryStream> GetStream(string request)
        {
            var received = new MemoryStream((int)_minBufferSize);
            var memWriter = new StreamWriter(received);
            var bufferSize = _minBufferSize;
            try
            {
                _dataWriter.WriteString(request);
                await _dataWriter.StoreAsync();
                await _dataWriter.FlushAsync();

                var count = await _dataReader.LoadAsync(bufferSize);
                while (count == bufferSize)
                {
                    bufferSize = bufferSize * 4;
                    if (bufferSize > _maxBufferSize)
                    {
                        bufferSize = _maxBufferSize;
                    }
                    memWriter.Write(_dataReader.ReadString(count));
                    count = await _dataReader.LoadAsync(bufferSize);
                }
                if (count > 0)
                {
                    memWriter.Write(_dataReader.ReadString(count));
                }
                return received;
            }
            catch (Exception)
            {

                throw;
            }
        }
    }
}
