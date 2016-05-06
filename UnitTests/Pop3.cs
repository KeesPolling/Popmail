using Microsoft.VisualStudio.TestPlatform.UnitTestFramework;
using PopMail.EmailProxies;
using System;
using System.Threading.Tasks;
namespace PopMail.UnitTests
{
    [TestClass]
    public class Pop3
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

        private async Task<Pop3Proxy> Connect(Pop3Proxy proxy)
        {
            await proxy.Connect();
            return proxy;
        }

        [TestMethod]
        public async Task TestMethodConnect()
        {
            using (var test = GetTestProxy())
            {
                await Connect(test);

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

                    Assert.AreEqual(true, (statistics.NumberOfMessages > 309),
                        String.Format("Aantal berichten {0}", statistics.NumberOfMessages));
                }
                catch (Exception ex)
                {
                    Assert.Fail(ex.Message);
                }
            }
        }
    }
}
