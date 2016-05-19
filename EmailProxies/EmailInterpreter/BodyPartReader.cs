using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PopMail.EmailProxies.EmailInterpreter
{
    #region SpecialByte
    internal enum SpecialByte : byte
    {
        Linefeed = 10,
        CarriageReturn = 13,
        Hyphen = 45
    }
    #endregion SpecialByte

    internal class BodyPartReader
    {
        internal ContentTypeFieldValue _contentType { get; private set; }
        private byte[] _boundary;
        private  byte[] _boundaryStart = new byte[4] { 13, 10, 45, 45 };

        internal BodyPartReader(ContentTypeFieldValue contentType, string encoding)
        {
            _boundary = new byte[contentType.Boundary.Length + 4];
            _boundaryStart.CopyTo(_boundary, 0);
            contentType.Boundary.CopyTo(_boundary, 4);
        }
        internal async Task ReadToStart(BufferedByteReader reader)
        {
            var nextByte = await reader.ReadByteAsync();
            var boundaryeByteIndex = 0;
            while (boundaryeByteIndex < _boundary.Length)
            {
                if (nextByte == _boundary[0])
                {
                    nextByte = await reader.ReadByteAsync();
                    boundaryeByteIndex = 1;
                    while (boundaryeByteIndex < _boundary.Length && nextByte == _boundary[boundaryeByteIndex])
                    {
                        nextByte = await reader.ReadByteAsync();
                        boundaryeByteIndex += 1;
                    }
                }
                else nextByte = await reader.ReadByteAsync();
            }
        }
        internal async Task<List<BodyPart>> ReadBodyPart( BufferedByteReader reader)
        {
            var bodyParts = new List<BodyPart>();
            var nextByte = await reader.ReadByteAsync();
            {
                if (nextByte == _boundary[0])
                {
                    if (await CheckBytes(reader))
                    {
                        var bodyPart = new BodyPart();
                        await bodyPart.ReadHeader(reader).ConfigureAwait(false);
                    }
                }
                nextByte = await reader.ReadByteAsync();
            }
            return bodyParts;
        }

        private async Task<bool> CheckBytes(BufferedByteReader reader)
        {
            var i = 1;
            while (i < _boundary.Length)
            {
                if (await reader.ReadByteAsync() != _boundary[i]) return false;
                i += 1;
            }
            return true;
        }
    }
}
