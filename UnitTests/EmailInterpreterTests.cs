using System;
using System.Globalization;
using System.Threading.Tasks;
using Windows.ApplicationModel;
using Windows.Storage;
using Microsoft.VisualStudio.TestPlatform.UnitTestFramework;
using PopMail.EmailProxies;
using PopMail.EmailProxies.EmailInterpreter;

namespace PopMail.UnitTests
{
    [TestClass]
    public class EmailInterpreterTests
    {
        [ClassInitialize]
        public static void PrepareData(TestContext T)
        {
            var unzipTask = FileUtils.UnZip(Package.Current.InstalledLocation, "EmailProxiesMock.zip", ApplicationData.Current.LocalFolder);
            unzipTask.Wait();
        }
        [TestMethod]
        public async Task ReadAddressCommentsFws()
        {
            const string fileName = "TestCFWSP.txt";
            var reader = new BufferedByteReader(new FileByteReader());
            await reader.GetStream(fileName);
            var header = new Header();
            await header.ReadHeader(reader);
            reader.Dispose(false);
            Assert.AreEqual("Pete", header.From.Adresses[0].Name, "This message is not from Pete");
            Assert.AreEqual("A Group", header.To.Groups[0].Name, "This message is not to 'A Group'");
            Assert.AreEqual("Chris Jones", header.To.Groups[0].Members[0].Name, "A Group does not include 'Chris Jones'");
        }
        [TestMethod]
        public async Task ReadMessageIDs()
        {
            const string fileName = "testReferences.txt";
            var reader = new BufferedByteReader(new FileByteReader());
            await reader.GetStream(fileName);
            var header = new Header();
            await header.ReadHeader(reader);
            reader.Dispose(false);
            Assert.AreEqual("abcd.1234@local.machine.test", header.MessageId, "This message has thet wrong ID");
            Assert.AreEqual("3456@example.net", header.InReplyTo.Identifiers[0], "This message replies to the wrong message");
            Assert.AreEqual(2, header.References.Identifiers.Count, "This message does not reference 2 messages");
        }
        [TestMethod]
        public async Task ReadDate()
        {
            const string fileName = "TestCFWSP.txt";
            var reader = new BufferedByteReader(new FileByteReader());
            await reader.GetStream(fileName);
            var header = new Header();
            await header.ReadHeader(reader);
            reader.Dispose(false);
            Assert.AreEqual(Convert.ToDateTime("14-02-1969 4:02", CultureInfo.CurrentCulture), header.OrigDate, "This message has the wrong Date");
        }

        [TestMethod]
        public async Task ReadMime2047_1()
        {
            const string fileName = "RFC2047-1.txt";
            var reader = new BufferedByteReader(new FileByteReader());
            await reader.GetStream(fileName);
            var header = new Header();
            await header.ReadHeader(reader);
            reader.Dispose(false);
            Assert.AreEqual("Keith Moore", header.From.Adresses[0].Name, "This message is not from Keith Moore (2047-1)");
            Assert.AreEqual("Keld Jørn Simonsen", header.To.Adresses[0].Name, "This message is not to Keld Jørn Simonsen (2047-1)");
            Assert.AreEqual("André Pirard", header.Cc.Adresses[0].Name, "This message is not Cced to André Pirard");
            Assert.AreEqual("If you can read this you understand the example.", header.Subject,"Subject not correct" );
        }
    }
}
