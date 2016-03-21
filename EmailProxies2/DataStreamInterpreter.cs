using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Storage.Streams;

namespace PopMailDemo.EmailProxies
{
    internal interface DataStreamInterpreter
    {
        void ReadStream(DataReader Dr);
        string EndSequence { get; set; }
        bool AtEnd { get; }
    }
}
