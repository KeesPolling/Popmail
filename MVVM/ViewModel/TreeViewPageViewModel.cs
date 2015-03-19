using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using WinRTXamlToolkit.Imaging;
using PopMailDemo.MVVM.Utilities;
using PopMailDemo.MVVM.Model;
using WinRTXamlToolkit.Tools;
using Windows.UI;
using Windows.UI.Xaml.Media;
using System.Threading.Tasks;
using PopMailDemo.MVVM.DataAcces;

namespace PopMailDemo.MVVM.ViewModel
{
    public class FolderTree : BindableBase
    {
        #region FolderItems
        private ObservableCollection<FolderVM> _folderItems;
        public ObservableCollection<FolderVM> FolderItems
        {
            get { return _folderItems; }
            set { this.SetProperty(ref _folderItems, value); }
        }
        #endregion

        public FolderTree()
        {
             FolderItems = BuildTree();
        }

        public ObservableCollection<FolderVM> BuildTree()
        {
            var tree = new ObservableCollection<FolderVM>();

            var db = Database.DbConnection;
            var RootFolders = db.Table<Folder>().Where(f => f.Parent == 0).ToListAsync().Result;
            //var RootFolders = db.FindAsync<Folder>(f => f.Parent == 0).Result;
            foreach (var Root in RootFolders)
            {
                tree.Add(new FolderVM(Root, null));
            }
            return tree;
        }
    }
}
