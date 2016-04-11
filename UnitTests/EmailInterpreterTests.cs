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
            var FileName = "TestCFWSP.txt";
            var reader = new BufferedByteReader(new FileByteReader());
            await reader.GetStream(FileName);
            var header = new Header();
            await header.ReadHeader(reader);
            reader.Dispose(false);
            Assert.AreEqual(header.From.Adresses[0].Name, "Pete", "This message is not from Pete");
            Assert.AreEqual(header.To.Groups[0].Name, "A Group", "This message is not to 'A Group'");
            Assert.AreEqual(header.To.Groups[0].Members[0].Name, "Chris Jones", "A Group does not include 'Chris Jones'");
        }
        [TestMethod]
        public async Task ReadMessageIDs()
        {
            var FileName = "testReferences.txt";
            var reader = new BufferedByteReader(new FileByteReader());
            await reader.GetStream(FileName);
            var header = new Header();
            await header.ReadHeader(reader);
            reader.Dispose(false);
            Assert.AreEqual(header.MessageId, "abcd.1234@local.machine.test", "This message has thet wrong ID");
            Assert.AreEqual(header.InReplyTo.Identifiers[0], "3456@example.net", "This message replies to the wrong message");
            Assert.AreEqual(header.References.Identifiers.Count, 2, "This message does not reference 2 messages");
        }
        [TestMethod]
        public async Task ReadDate()
        {
            var FileName = "TestCFWSP.txt";
            var reader = new BufferedByteReader(new FileByteReader());
            await reader.GetStream(FileName);
            var header = new Header();
            await header.ReadHeader(reader);
            reader.Dispose(false);
            Assert.AreEqual(Convert.ToDateTime("14-02-1969 4:02", CultureInfo.CurrentCulture), header.OrigDate, "This message has the wrong Date");
        }
    }
}
