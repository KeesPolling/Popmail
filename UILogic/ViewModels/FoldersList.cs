using System.Collections.ObjectModel;
using Prism.Windows.AppModel;
using Prism.Windows.Mvvm;
using PopMail.Models;
using PopMail.DataAcces;
//using System;
using System.Threading.Tasks;
//using System.Windows.Input;

namespace PopMail.ViewModels
{
    public class FoldersList : ViewModelBase
    {
        private ObservableCollection<FolderViewModel> _folderItems;
        public FoldersList()
        {
        }
        [RestorableState]
        public ObservableCollection<FolderViewModel> FolderTree
        {
            get { return _folderItems; }
            set { this.SetProperty(ref _folderItems, value); }
        }
        public async Task<ObservableCollection<FolderViewModel>> BuildTree()
        {
            var tree = new ObservableCollection<FolderViewModel>();

            var db = Database.DbConnection;
            var rootFolders = await db.Table<Folder>().Where(f => f.Parent == 0).ToListAsync();
            //var RootFolders = db.FindAsync<Folder>(f => f.Parent == 0).Result;
            foreach (var root in rootFolders)
            {
                tree.Add(new FolderViewModel(root, null));
            }
            return tree;
        }
    }
}
