using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace PopMail.EmailProxies.EmailInterpreter
{
    public class Body
    {
        private readonly byte[] _boundaryStart = new byte[4] { 13, 10, 45, 45 }; // "\r\n--"
        public Header Header { get; } = new Header();
        public ContentTypeFieldValue ContentType { get; private set; }

        public List<Body> Bodies{ get; set; } 
        public MemoryStream Content { get; set; }
        public Body Parent { get; }
        private bool _boundaryPossible;
        private bool BoundaryPossible
        {
            get { return _boundaryPossible; }
            set
            {
                _boundaryPossible = value;
                if (value == true)
                {
                    if (Parent != null) Parent.BoundaryPossible = true;
                }
            }
        }
        private bool NoBoundaryPossible
        {
            get
            {
                if (_boundaryPossible) return false;
                if (Parent == null) return !_boundaryPossible;
                return Parent.NoBoundaryPossible;
            }
        }
        private bool _completed;
        private string _charset;
        private int _maxBoundaryLength; 
        internal Body (Body parent, int maxBoundaryLength)
        {
            Parent = parent;
            _completed = false;
            _maxBoundaryLength = maxBoundaryLength;
        }
        internal async Task ReadBody(BufferedByteReader reader, string transferEncoding)
        {
            switch (Header.ContentType.Type)
            {
                case "multipart":
                    await ReadMultiPart(reader);
                    break;
                case "text":
                  
                    Content = (new MemoryStream(Encoding.Unicode.GetBytes(textString)));
                    break;
                case "image":
                case "audio":
                case "video":
                case "application":
                    break;
            }
        }
        private async Task ReadSinglePart(BufferedByteReader reader)
        {
            byte nextByte;
            while (!_completed && !reader.MessageEnd)
            {
                nextByte = await reader.ReadByteAsync();
                if nextByte

            }
        }
        private async Task ReadMultiPart(BufferedByteReader reader)
        {
            if (ContentType.Boundary.Length > _maxBoundaryLength) _maxBoundaryLength = ContentType.Boundary.Length;
            await ReadToStart(reader);
            while (!_completed)
            {
                var body = new Body(this, _maxBoundaryLength);
                await body.Header.ReadHeader(reader);
                await body.ReadBody(reader, Header.ContentTransferEncoding);
                Bodies.Add(body);
            }
            if (!_completed) Complete();
            return;
        }
        private async Task<bool> CheckForEnd(BufferedByteReader reader)
        {
            if ((await reader.ReadByteAhead()) == 45)
            {
                if ((await reader.ReadByteAhead()) == 45)
                {
                    reader.Clear();
                    Complete();
                    return true;
                }
            }
            return false;
        }

        private async Task ReadToStart(BufferedByteReader reader)
        {
            var nextByte = await reader.ReadByteAsync();
            while (!(await CheckBytes(reader, nextByte, null))) await reader.ReadByteAsync();
        }
        private async Task<bool> CheckBytes(BufferedByteReader reader,  byte newByte, Body body)
        {
            if (newByte != _boundaryStart[0]) return false;
            if ((await reader.ReadByteAhead()) != _boundaryStart[1]) return false;
            if ((await reader.ReadByteAhead()) != _boundaryStart[2]) return false;
            if ((await reader.ReadByteAhead()) != _boundaryStart[3]) return false;
            _boundaryPossible = true;
            var i = 0;
            while (!_completed &&  !NoBoundaryPossible && i < _maxBoundaryLength)
            {
                var oneByte = await reader.ReadByteAhead();
                CheckOneByte(oneByte, i, this);
                i += 1;
            }
            if 
            return true;
        }
        private void Complete()
        {
            if (_completed) return;
            foreach (Body child in Bodies)
            {
                child.Complete();
            }
            if (Content != null && Content.Length > 0)
            {
                      
            }
            _completed = true;
        }
        private bool CheckOneByte(byte oneByte, int index, Body body)
        {
            Parent?.CheckOneByte(oneByte, index, this);
            if (_completed) return true; 
            if (!_boundaryPossible) return false;
            if (index < ContentType.Boundary.Length)
            {
                if (ContentType.Boundary[index] != oneByte)
                {
                    _boundaryPossible = false;
                    return false;
                }
                return true;
            }
            if ((index + 1)==ContentType.Boundary.Length) body?.Complete();
            return true;
        }
    }
}
