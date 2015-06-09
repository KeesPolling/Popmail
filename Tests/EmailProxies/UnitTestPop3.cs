using PopMailDemo.EmailProxies;
using System;
using System.IO;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using Windows.Networking;
using Windows.Storage.Streams;
using Microsoft.VisualStudio.TestPlatform.UnitTestFramework;


namespace PopMailDemo.EmailProxies.Tests
{
    [TestClass]
    public class UnitTest1
    {
        private Pop3Proxy GetTestProxy() 
        {
            var Name = "Caiway";
            var ProviderUri = "pop.caiway.net";
            var ServiceName = "110";
            var AccountName = "kpolling@caiway.net";
            var Password = "kuif0001";
            
            return new Pop3Proxy(Name, ProviderUri, ServiceName, AccountName, Password);
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

                    var messages = await test.IdentifierList();
                    await test.Disconnect();

                    Assert.AreEqual(true, (messages.Count > 309), String.Format("Aantal berichten {0}", messages.Count));
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
