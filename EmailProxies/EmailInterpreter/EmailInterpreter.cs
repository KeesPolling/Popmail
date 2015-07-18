using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Storage.Streams;


namespace PopMailDemo.EmailProxies.EmailInterpreter
{
    internal enum SpecialByte : byte
    {
        Linefeed = 10 ,
        CarriageReturn = 13,
        Space = 32, 
        Quote = 34, // "
        LeftParenthesis = 40, // (
        RightParernthesis = 41, // )
        Comma = 44,  // ,
        Colon = 58, // :
        SemiColon = 59, //;
        LeftAngledBracket = 60,  // <
        RightAngledBracket = 63, // >
        Backslash = 92 // \
    }
    internal enum EndType
    {
        None = 0,
        EndOfField,
        EndOfHeader
    }
    internal class EOLResult
    {
        internal bool End { get; set; }
        internal byte NextByte { get; set; }
    }
    internal class Comment
    {
        internal async Task Process(IpDialog Ip)
        {
            var nextByte = await Ip.ReadByte();
            while (nextByte != (byte)SpecialByte.RightParernthesis)
            {
                if (nextByte == (byte)SpecialByte.Backslash)
                {
                    nextByte = await Ip.ReadByte();
                }
                else
                {
                    if (nextByte == (byte)SpecialByte.LeftParenthesis)
                    {
                        var nestedComment = new Comment();
                        await nestedComment.Process(Ip);
                    }
                }
                nextByte = await Ip.ReadByte();
            }
        }
    }
    public class EmailInterpreter
    {
        
        Email _email = new Email();
        enum EmailPart {Header, Body, MimeParts}
        enum HeaderElementPart { Name, Value, Complete}
        enum CrLfPart { none, Cr, Lf, Fws, Cr2}
        enum HeaderFieldType { Ignore, AddressList, DateTime, MessageId, Other}
        enum AddressPart { GroupName, DisplayName, MailBox}

        HeaderElementPart _elementPart = HeaderElementPart.Name;
        EmailPart _partStatus = EmailPart.Header;
        CrLfPart _crLfPart = CrLfPart.none;
        HeaderFieldType _fieldType;
        AddressPart _addressPart;
        string _mimeSeparator;
        uint _commentLevel = 0;
        
        bool _quotedChar = false;
        bool _quotedString = false;
        bool _atEnd = false;
        bool _processingMessageId = false;

        StringBuilder _nameBuilder = new StringBuilder();
        StringBuilder _valueBuilder = new StringBuilder();
        StringBuilder _headerBuilder = new StringBuilder();

        string _fieldName;
        string _groupName;
        string _displayName;
        string _mailbox;


        List<string> _messageIds = new List<string>();

        public string EndSequence { get; set; }

        internal EmailInterpreter()
        {
            _partStatus = EmailPart.Header;
            
        }
        public bool AtEnd
        {
            get
            {
                return _atEnd;
            }
        }
        private byte ReadHeaderBuffer(DataReader Dr)
        {
            var buffer = Dr.ReadByte();
            _headerBuilder.Append(buffer);
            return buffer;
        }
        
        private void ProcessComment(byte Buffer)
        {
            if (_commentLevel == 0)
            {
                if (!_valueBuilder.ToString().EndsWith(" "))
                {
                    _valueBuilder.Append(" ");
                }
            }
        }
        private void ProcessQuotedString(byte Buffer)
        {
            if (_crLfPart != CrLfPart.none)
            {
                ProcessCrLf(Buffer);
                if
                (
                    (_elementPart != HeaderElementPart.Value)
                    || (_crLfPart != CrLfPart.none)
                )
                {
                    _elementPart = HeaderElementPart.Value;
                    _crLfPart = CrLfPart.none;
                }
                // If no more white spaces after CrLf then go on
            }
            if (_quotedChar == true)
            {
                _valueBuilder.Append(Convert.ToChar(Buffer));
            }
        }
        private void ProcessCrLf(byte Buffer)
        {
            switch (_crLfPart)
            {
                case CrLfPart.Cr:
                    if (Buffer == 10)
                    {
                        _crLfPart = CrLfPart.Lf;
                    }
                    else
                    {
                        throw new FormatException("carriagereturn must be followed by linefeed");
                    }
                    break;
                case CrLfPart.Lf:
                    if (Buffer == 32) // space = folding white space
                    {
                        _crLfPart = CrLfPart.Fws;
                    }
                    else
                    {
                        _elementPart = HeaderElementPart.Name;
                        if (Buffer == 13)
                        {
                            _crLfPart = CrLfPart.Cr2;
                        }
                        else
                        {
                            // Note: buffer contains first char of next FieldNanme or Cr if last headerfield name
                            _nameBuilder.Append(Convert.ToChar(Buffer));
                        }
                    }
                    break;
                case CrLfPart.Fws:
                    if (Buffer == 32)
                    {
                        break;
                    }
                    if (Buffer == 13)
                    {
                        _crLfPart = CrLfPart.Cr2;
                    }
                    else
                    {
                        if (!_valueBuilder.ToString().EndsWith(" "))
                        {
                            _valueBuilder.Append(" ");
                        }
                        _crLfPart = CrLfPart.none;
                    }
                    break;
                case CrLfPart.Cr2:
                    if (Buffer == 10)
                    {
                        _partStatus = EmailPart.Body;
                        _elementPart = HeaderElementPart.Complete;
                        _crLfPart = CrLfPart.none;
                    }
                    else
                    {
                        throw new FormatException("carriagereturn must be followed by linefeed");
                    }
                    break;
            }
        }
        private void ProcessIgnore(DataReader Dr)
        {
            byte buffer;
            while 
                (
                    (_elementPart == HeaderElementPart.Value)
                    && (_partStatus == EmailPart.Header)
                    && (Dr.UnconsumedBufferLength > 0)
                )
            {
                buffer = ReadHeaderBuffer(Dr);
                if (_crLfPart != CrLfPart.none)
                {
                    ProcessCrLf(buffer);
                    if (_crLfPart != CrLfPart.none)
                    {
                        continue;
                    }
                    // If no more white spaces after CrLf then go on
                }
                if (buffer == 13)
                {
                    _crLfPart = CrLfPart.Cr;
                }
            }
        }
        private void ProcessDateTime(DataReader Dr)
        {
            byte buffer;
            while
                (
                    (_elementPart == HeaderElementPart.Value)
                    && (_partStatus == EmailPart.Header)
                    && (Dr.UnconsumedBufferLength > 0)
                )
            {
                buffer = ReadHeaderBuffer(Dr);
                if (_crLfPart != CrLfPart.none)
                {
                    ProcessCrLf(buffer);
                    if 
                    (
                        (_crLfPart != CrLfPart.none)
                        || (_elementPart != HeaderElementPart.Value)
                    )
                    {
                        continue;
                    }
                    // If no more white spaces after CrLf then go on
                }
                
                if (_commentLevel > 0)
                {
                    ProcessComment(buffer);
                    continue;
                }

                switch (buffer)
                {
                    case 13: // "\r\n": end of line
                        _crLfPart = CrLfPart.Cr;
                        break;
                    case 40: // "(": begin comment
                        _commentLevel = 1;
                        break;
                    default:
                        _valueBuilder.Append(Convert.ToChar(buffer));
                        break;
                }
            }
            if
            (
                (_elementPart == HeaderElementPart.Value)
                && (_partStatus == EmailPart.Header)
            )
            {
                _email.OrigDate = Convert.ToDateTime(_valueBuilder.ToString().Trim());
            }
        }

        private void ProcessMessageId(DataReader Dr)
        {
            byte buffer;
            while
                (
                    (_elementPart == HeaderElementPart.Value)
                    && (_partStatus == EmailPart.Header)
                    && (Dr.UnconsumedBufferLength > 0)
                )
            {
                buffer = ReadHeaderBuffer(Dr);
                if (_crLfPart != CrLfPart.none)
                {
                    ProcessCrLf(buffer);
                    if
                    (
                        (_crLfPart != CrLfPart.none)
                        || (_elementPart != HeaderElementPart.Value)
                    )
                    {
                        continue;
                    }
                    // If no more white spaces after CrLf then go on
                }
                if (_commentLevel > 0)
                {
                    ProcessComment(buffer);
                    continue;
                }

                switch (buffer)
                {
                    case 13: // "\r\n": end of line
                        _crLfPart = CrLfPart.Cr;
                        break;
                    case 40: // "(": begin comment
                        if (_processingMessageId)
                        {
                            _valueBuilder.Append(Convert.ToChar(buffer));
                        }
                        else
                        {
                            _commentLevel = 1;
                        }
                        break;
                    case 34: //  """: begin quoted string
                        if (_processingMessageId)
                        {
                            _valueBuilder.Append(Convert.ToChar(buffer));
                        }
                        else
                        {
                            _quotedString = true;
                        }
                        break;
                    case 60: // "<": begin messageId
                        if (_processingMessageId)
                        {
                            _valueBuilder.Append(Convert.ToChar(buffer));
                        }
                        else
                        {
                            _valueBuilder = new StringBuilder();
                        }
                        break;
                    case 62: // "<": begin messageId
                        if (_processingMessageId)
                        {
                            _messageIds.Add(_valueBuilder.ToString());
                            _processingMessageId = false;
                            _valueBuilder = new StringBuilder();
                        }
                        break;
                    default:
                        if (_processingMessageId)
                        {
                            _valueBuilder.Append(Convert.ToChar(buffer));
                        }
                        break;
                }
            }
            if
            (
                (_elementPart == HeaderElementPart.Value)
                && (_partStatus == EmailPart.Header)
            )
            {
                switch (_fieldName)
                {
                    case "Message-ID": // contains only one Id
                        _email.MessageId = _messageIds[0];
                        break;
                    case "In-Reply-To":
                        _email.InReplyTo = _messageIds;
                        break;
                    case "References":
                        _email.References = _messageIds;
                        break;
                }
            }
        }
        public void ReadStream(DataReader Dr)
        {

        }
    }
}
