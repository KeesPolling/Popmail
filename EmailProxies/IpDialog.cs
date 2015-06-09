﻿using System;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using Windows.Data.Xml.Dom;
using Windows.Networking;
using Windows.Networking.Sockets;
using Windows.Storage;
using Windows.Storage.Streams;

namespace PopMailDemo.EmailProxies
{
    /// <summary>
    /// This class is used for sending a request and receiving a response
    /// over the internet.
    /// minBufferSize and maxBufferSize are read from a XML configration file
    /// </summary>
    class IpDialog: IDisposable
    {
        bool _disposed;
        uint _minBufferSize;
        uint _maxBufferSize;
        StreamSocket _streamSocket;
        DataReader _dataReader;
        DataWriter _dataWriter;

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
                var FileName = new Uri(String.Format("ms-appx:///Assets/Configuration/{0}", "All.xml"));
                StorageFile file = await StorageFile.GetFileFromApplicationUriAsync
                (
                    FileName    
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
                _maxBufferSize = (node == null) ? (uint)64001 : (uint)node.NodeValue;
            }
            catch (System.IO.FileNotFoundException fnfe) 
            {
                _minBufferSize = 1024;
                _maxBufferSize = 64000;
            }

        }
        /// <summary>
        /// Sends a request to the service
        /// </summary>
        /// <param name="Request">a valid request</param>
        /// <returns>the response from the server</returns>
        public async Task<string> GetSingleLineResponse(string Request)
        {
            var received = new StringBuilder();
            var bufferSize = _minBufferSize;
            try
            {
                await SendRequest(Request);
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
        /// <param name="Request">a valid request</param>
        /// <returns>the response from the server</returns>
        public async Task<string> GetMultiLineResponse(string Request)
        {
            var received = new StringBuilder();
            var bufferSize = _minBufferSize;
            try
            {
                await SendRequest(Request);
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
        /// <param name="Request">a valid request</param>
        /// <returns>the response from the server</returns>
        public async Task<MemoryStream> GetStream(string Request)
        {
            var received = new MemoryStream((int)_minBufferSize); 
            var memWriter = new StreamWriter(received);
            var bufferSize = _minBufferSize;
            try
            {
                await SendRequest(Request);
                var count = await _dataReader.LoadAsync(bufferSize);
                memWriter.Write(_dataReader.ReadString(count));
                while (_dataReader.UnconsumedBufferLength > 0)
                {
                    bufferSize = bufferSize * 4;
                    if (bufferSize > _maxBufferSize)
                    {
                        bufferSize = _maxBufferSize;
                    }
                    
                    count = await _dataReader.LoadAsync(bufferSize);
                }
                if (count > 0)
                {
                    memWriter.Write(_dataReader.ReadString(count));
                }
                memWriter.Flush();
                return received;
            }
            catch (Exception)
            {

                throw;
            }
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
                if (_dataReader != null)
                {
                     _dataReader.Dispose();
                }
                if (_dataWriter != null)
                {
                    _dataWriter.Dispose();
                }
                if (_streamSocket != null)
                {
                    _streamSocket.Dispose();
                }
                _disposed = true;
            }
        }
    }
}
