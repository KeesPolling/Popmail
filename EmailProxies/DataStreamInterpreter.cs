using Windows.Storage.Streams;

namespace PopMail.EmailProxies
{
    internal interface IDataStreamInterpreter
    {
        void ReadStream(DataReader Dr);
        string EndSequence { get; set; }
        bool AtEnd { get; }
    }
}
