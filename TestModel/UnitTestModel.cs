using PopMailDemo;
using PopMailDemo.MVVM.DataAcces;
using PopMailDemo.MVVM.Model;
using PopMailDemo.MVVM.ViewModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Networking;
using SQLite;
using Microsoft.VisualStudio.TestPlatform.UnitTestFramework;

namespace TestModel
{
    [TestClass]
    public class DataModel
    {
        [TestMethod]
        public async Task EmailProvider()
        {
           
            EmailProviderVM provider = new EmailProviderVM();
            provider.Name = "Caiway";
            provider.ProviderUri = (new HostName("pop.caiway.net").DisplayName);
            provider.ServiceName = "110";
            provider.AccountName = "kpolling@caiway.net";
            provider.Password = "kuif0001";
            await provider.Save();
            Assert.IsNotNull(provider, "Provider niet aangemaakt");
        }
        [TestMethod]
        public async Task Folder()
        {
            var db = Database.DbConnection;
            var folder = new Folder();
            folder.Name = "Test";
            var i = await db.InsertAsync(folder);
            var result = await db.FindAsync<Folder>(f => f.Name == "Test");
            Assert.IsNotNull(result, "Folder niet aangemaakt");
        }
        [TestMethod]
        public async Task FolderVM()
        {
            var folder = new FolderVM("Test", null);
            await folder.Save();
            Assert.IsNotNull(folder, "Folder niet aangemaakt");
        }
        [TestMethod]
        public async Task FolderVMparent1()
        {
            var parentFolder = new FolderVM("TestParent", null);
            var testFolder = new FolderVM("Test", parentFolder);
            await testFolder.Save();
            Assert.IsNotNull(testFolder, "Folder niet aangemaakt");
            Assert.IsNotNull(parentFolder, "parentFolder niet aangemaakt");
            Assert.AreEqual<int>(0, testFolder.Children.Count, "Child ten onrechte aangemaakt");
            Assert.AreEqual("Test", parentFolder.Children[0].Name, false);
        }
        [TestMethod]
        public async Task FolderVMRecusie1()
        {
            var testFolder = new FolderVM("TestParent", null);
            testFolder.Parent = testFolder;
            await testFolder.Save();
            Assert.IsNotNull(testFolder, "Folder niet aangemaakt");
            Assert.IsNull(testFolder.Parent, "Parent recursie!");
        }
    }
}

