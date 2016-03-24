﻿using Microsoft.VisualStudio.TestPlatform.UnitTestFramework;
using PopMail.DataAcces;
using PopMail.Models;
using PopMail.ViewModels;
using System.Linq;
using System.Threading.Tasks;
using Windows.Networking;

namespace PopMail.UnitTests
{
    [TestClass]
    public class DataModel
    {
        [TestMethod]
        public async Task EmailProvider()
        {
           
            EmailProviderPageViewModel provider = new EmailProviderPageViewModel();
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
            var folder = new Folder();
            folder.Name = "Test";
            var i = await db.InsertAsync(folder);
            var result = await db.FindAsync<Folder>(f => f.Name == "Test");
            Assert.IsNotNull(result, "Folder niet aangemaakt");
        }
        [TestMethod]
        public async Task FolderVM1()
        {
            var folder = new FolderViewModel("Test");
            await folder.Save();
            var root = await FolderViewModel.GetRootItems();
            Assert.AreNotEqual(0, root.Count(), "Folder niet aangemaakt");
        }
        [TestMethod]
        public async Task FolderVMparent1()
        {
            var parentFolder = new FolderViewModel("TestParent");
            await parentFolder.Save();
            var testFolder = await parentFolder.AddChild("Test");
            await testFolder.Save();
            Assert.IsNotNull(testFolder, "Folder niet aangemaakt");
            Assert.IsNotNull(parentFolder, "parentFolder niet aangemaakt");
            Assert.AreEqual<int>(0, testFolder.Children.Count, "Child ten onrechte aangemaakt");
            Assert.AreEqual("Test", parentFolder.Children[0].Name, false);
        }
        [TestMethod]
        public async Task FolderVMRecusie1()
        {
            var testFolder = new FolderViewModel("TestParent");
            await testFolder.Save();
            var testChild = await testFolder.AddChild(testFolder);
            await testFolder.Save();
            Assert.IsNotNull(testFolder, "Folder niet aangemaakt");
            Assert.IsNull(testFolder.Parent, "Parent recursie!");
        }
        [TestMethod]
        public async Task FolderVMaddChild()
        {
            var testFolder = new FolderViewModel("TestParent");
            await testFolder.Save();
            var test = await testFolder.AddChild("testChild");
            await test.Save();
            Assert.IsNotNull(test, "Folder niet aangemaakt");
            Assert.AreEqual("testChild", testFolder.Children[0].Name, false);
        }
    }
}
