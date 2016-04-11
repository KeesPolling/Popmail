using Windows.Storage.Streams;

namespace PopMail.EmailProxies.IP_helpers
{
    internal interface IDataStreamInterpreter
    {
        void ReadStream(DataReader dr);
        string EndSequence { get; set; }
        bool AtEnd { get; }
    }
}
