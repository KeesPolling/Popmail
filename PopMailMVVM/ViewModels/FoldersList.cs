using System.Collections.ObjectModel;
//using Microsoft.Practices.Prism.Commands;
using Microsoft.Practices.Prism.Mvvm;
using PopMail.Models;
using PopMail.DataAcces;
//using System;
using System.Threading.Tasks;
//using System.Windows.Input;

namespace PopMail.ViewModels
{
    public class FoldersList : ViewModel
    {
        private ObservableCollection<FolderViewModel> _folderItems;
        public FoldersList()
        {
            _folderItems = BuildTree().GetAwaiter().GetResult();
        }
        [RestorableState]
        public ObservableCollection<FolderViewModel> FolderTree
        {
            get { return _folderItems; }
            set { this.SetProperty(ref _folderItems, value); }
        }
        private async Task<ObservableCollection<FolderViewModel>> BuildTree()
        {
            var tree = new ObservableCollection<FolderViewModel>();

            var db = Database.DbConnection;
            var RootFolders = await db.Table<Folder>().Where(f => f.Parent == 0).ToListAsync();
            //var RootFolders = db.FindAsync<Folder>(f => f.Parent == 0).Result;
            foreach (var Root in RootFolders)
            {
                tree.Add(new FolderViewModel(Root, null));
            }
            return tree;
        }
    }
}
