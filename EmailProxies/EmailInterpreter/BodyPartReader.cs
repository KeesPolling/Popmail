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
        internal byte[] Boundary { get; private set; }

        internal async Task<List<BodyPart>> ReadBodyParts(byte[] boundary, BufferedByteReader reader)
        {
            Boundary = boundary;
            var bodyParts = new List<BodyPart>();
            var nextByte = await reader.ReadByteAsync();
            var endBytes = new byte[5] {13,10, 46, 13, 10};
            reader.EndBytes = endBytes;
            while (!reader.MessageEnd)
            {
                if (nextByte == (byte) SpecialByte.CarriageReturn)
                {
                    if (await CheckBytes(reader))
                    {
                        var bodyPart = new BodyPart();
                         bodyPart.ReadHeader(reader, boundary);

                    }
                }
                nextByte = await reader.ReadByteAsync();
            }
            return bodyParts;
        }

        private async Task<bool> CheckBytes(BufferedByteReader reader)
        {
            if (await reader.ReadByteAsync() != (byte) SpecialByte.Linefeed)
                return false;
            if (await reader.ReadByteAsync() != (byte)SpecialByte.Hyphen)
                return false;
            if (await reader.ReadByteAsync() != (byte)SpecialByte.Hyphen)
                return false;
            var i = 0;
            while (i < Boundary.Length)
            {
                if (await reader.ReadByteAsync() != Boundary[i]) return false;
            }
            return true;
        }
    }
}
