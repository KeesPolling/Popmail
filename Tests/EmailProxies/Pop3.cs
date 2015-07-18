using Microsoft.VisualStudio.TestPlatform.UnitTestFramework;
using PopMailDemo.EmailProxies;
using System;
using System.Threading.Tasks;

namespace PopMailDemo.Tests.EmailProxies
{
    [TestClass]
    public class Pop3
    {
        [ClassInitialize]
        public static void PrepareData(TestContext T)
        {
            var testunzip = new PrepareMockData();
            var UnzipTask = testunzip.UnZip("EmailProxiesMock.zip");
            UnzipTask.Wait();
        }
        private Pop3Proxy GetTestProxy() 
        {
            var Name = "Caiway";
            var ProviderUri = "pop.caiway.net";
            var ServiceName = "110";
            var AccountName = "kpolling@caiway.net";
            var Password = "kuif0001";
            
            return new Pop3Proxy(Name, ProviderUri, ServiceName, AccountName, Password);
        }
        [TestMethod]
        public async Task ReadFile()
        {
            var FileName = "TestCFWSP.txt";
            var reader = new FileByteReader();
            await reader.GetStream(FileName);
            var nextByte = await reader.ReadByte();
            reader.Dispose(false);
        }
        private async Task<Pop3Proxy> Connect(Pop3Proxy Proxy)
        {
            await Proxy.Connect();
            return Proxy;
        }    

        [TestMethod]
        public async Task TestMethodConnect()
        {
            using (var test = GetTestProxy())
            {
                await this.Connect(test);

                await test.Disconnect();
            }
        }

        [TestMethod]
        public async Task TestMethodGetList()
        {
            using (var test = GetTestProxy())
            {
                try
                {
                    await this.Connect(test);

                    var messages = await test.LIST();
                    await test.Disconnect();

                    Assert.AreEqual(true, (messages.Count > 309), String.Format("Aantal berichten {0}", messages.Count));
                }
                catch (Exception ex)
                {
                    Assert.Fail(ex.Message);
                }
            }
        }

        [TestMethod]
        public async Task TestMethodGetUild()
        {
            using (var test = GetTestProxy())
            {
                try
                {
                    await this.Connect(test);

                    var messages = await test.UILD();
                    await test.Disconnect();

                    Assert.AreEqual(true, (messages.Count > 309), String.Format("Aantal berichten {0}", messages.Count));
                }
                catch (Exception ex)
                {
                    Assert.Fail(ex.Message);
                }
            }
        }

        [TestMethod]
        public async Task TestMethodGetStat()
        {
            using (var test = GetTestProxy())
            {
                try
                {
                    await this.Connect(test);

                    var statistics = await test.STAT();
                    await test.Disconnect();

                    Assert.AreEqual(true, (statistics.NumberOfMessages > 309), String.Format("Aantal berichten {0}", statistics.NumberOfMessages));
                }
                catch (Exception ex)
                {
                    Assert.Fail(ex.Message);
                }
            }
        }

        //[TestMethod]
        //public async Task  MailmessageFromStream()
        //{
        //    var path = Windows.ApplicationModel.Package.Current.InstalledLocation.Path;
        //    var file = await Windows.Storage.StorageFile.GetFileFromPathAsync(path + "\\TestCFWSP.txt");
        //    // Read all bytes in from a file on the disk.
        //    IBuffer buffer = await Windows.Storage.FileIO.ReadBufferAsync(file);
        //    var bytes = WindowsRuntimeBufferExtensions.ToArray(buffer);
        //    // Create a memory stream from those bytes.
        //    using (MemoryStream memory = new MemoryStream(bytes))
        //    {
        //        var Message = new Message.MailMessage(memory);
        //        Assert.AreEqual(Message.From.Count, 1);
        //    }

        //}
    }
}
