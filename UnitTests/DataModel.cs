using Microsoft.VisualStudio.TestPlatform.UnitTestFramework;
using Popmail.UILogic.Models;
using Popmail.UILogic.ViewModels;
using System.Linq;
using System.Threading.Tasks;
using Windows.Networking;
using Popmail.UILogic.DataAcces;

namespace PopMail.UnitTests
{
    [TestClass]
    public class DataModel
    {
        [TestMethod]
        public async Task EmailProvider()
        {
           
            EmailProviderPageViewModel provider = new EmailProviderPageViewModel(null);
            provider.Name = "Caiway";
            provider.ProviderUri = (new HostName("pop.caiway.net").DisplayName);
            provider.ServiceName = "110";
            provider.AccountName = "kpolling@caiway.net";
            provider.Password = "kuif0001";
            provider.User = "kpolling@caiway.net";
            await provider.Save();
            Assert.IsNotNull(provider, "Provider niet aangemaakt");
        }
        [TestMethod]
        public async Task Folder()
        {
            var db = Database.DbConnection;
            var folder = new Folders();
            folder.Name = "Test";
            var i = await db.InsertAsync(folder);
            var result = await db.FindAsync<Folders>(f => f.Name == "Test");
            Assert.IsNotNull(result, "Folder niet aangemaakt");
        }
        [TestMethod]
        public async Task FolderVm1()
        {
            var tree = new FolderTreeViewModel();
            var folder = new FolderViewModel("Test", tree);
            await folder.Save();
          
            var root = tree.Children;
            Assert.AreNotEqual(0, root.Count(), "Folder niet aangemaakt");
        }
        [TestMethod]
        public async Task FolderVmparent1()
        {
            var tree = new FolderTreeViewModel();
            var parentFolder = new FolderViewModel("TestParent", tree);
            await parentFolder.Save();
            var testFolder = await parentFolder.AddChild("Test");
            Assert.IsNotNull(testFolder, "Folder niet aangemaakt");
            Assert.IsNotNull(parentFolder, "parentFolder niet aangemaakt");
            Assert.AreEqual<int>(0, testFolder.Children.Count, "Child ten onrechte aangemaakt");
            Assert.AreEqual("Test", parentFolder.Children[0].Name, false);
        }
        [TestMethod]
        public async Task FolderVmRecusie1()
        {
            var tree = new FolderTreeViewModel();
            var testFolder = new FolderViewModel("TestParent", tree);
            await testFolder.Save();
            var testChild = await testFolder.AddChild(testFolder);
            Assert.IsNotNull(testFolder, "Folder niet aangemaakt");
            Assert.IsNull(testFolder.Parent, "Parent recursie!");
            Assert.AreEqual(testFolder.Children.Count, 0, "Child recursie!");
        }
        [TestMethod]
        public async Task FolderVMaddChild()
        {
            var tree = new FolderTreeViewModel();
            var testFolder = new FolderViewModel("TestParent", tree);
            await testFolder.Save();
            var test = await testFolder.AddChild("testChild");
            await test.Save();
            Assert.IsNotNull(test, "Folder niet aangemaakt");
            Assert.AreEqual("testChild", testFolder.Children[0].Name, false);
        }
    }
}

