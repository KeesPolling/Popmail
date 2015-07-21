using Microsoft.VisualStudio.TestPlatform.UnitTestFramework;
using Windows.ApplicationModel;
using PopMailDemo.EmailProxies;
using System;
using System.Threading.Tasks;
using Windows.Storage;
using PopMailDemo.EmailProxies.EmailInterpreter;

namespace PopMailDemo.Tests.EmailProxies
{
    [TestClass]
    public class Pop3
    {
        [ClassInitialize]
        public static void PrepareData(TestContext T)
        {
            var UnzipTask = FileUtils.UnZip(Package.Current.InstalledLocation ,"EmailProxiesMock.zip", ApplicationData.Current.LocalFolder);
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
        public async Task ReadHeaderCommentsFWS()
        {
            var FileName = "TestCFWSP.txt";
            var reader = new FileByteReader();
            await reader.GetStream(FileName);
            var header = new Header();
            await header.ReadHeader(reader);
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

        [TestMethod]
        public async Task MailmessageFromStream()
        {

        }
    }
}
