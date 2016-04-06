
using Prism.Windows.Mvvm;
using PopMail.DataAcces;
using Popmail.UILogic.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices.ComTypes;
using System.Threading.Tasks;
using Windows.UI.Notifications;
using SQLite.Net.Interop;

namespace Popmail.UILogic.ViewModels
{
    public class FolderViewModel: ViewModelBase
    {
        private FolderTreeViewModel _visualTree;
        private Folder _folder;
        private FolderViewModel _parent;
        private string _path;
        private ObservableCollection<FolderViewModel> _children =  new ObservableCollection<FolderViewModel>();
        public static async Task GetRootItems(FolderTreeViewModel folderTree)
        {
            Debug.Assert(folderTree != null);
            var db = Database.DbConnection;
            var rootFolders = await db.Table<Folder>().Where(f => f.Parent == 0).ToListAsync();
            var firstFolder = rootFolders.First();
            //  Skip single rootfolders, unless it does not have children
            while (rootFolders.Count == 1)
            {
                var id = firstFolder.Id;
                var children = await db.Table<Folder>().Where(f => f.Parent == id).ToListAsync();
                if (children.Count == 0) break;
                rootFolders = children;
                firstFolder = rootFolders.First();
            }
            var root = new ObservableCollection<FolderViewModel>();
            foreach (Folder folder in rootFolders)
            {
                var folderViewModel = new FolderViewModel(folder,null,folderTree);
                await folderViewModel.ReadChildrenFromDb(folderTree);
                root.Add(folderViewModel);
            }
            folderTree.Children = root;
        }
        #region private Constructors
        internal FolderViewModel(Folder myFolder, FolderViewModel myParent, FolderTreeViewModel visualTree )
        {
            this._folder = myFolder;
            _visualTree = visualTree;
            Parent = myParent;
        }
        #endregion


        #region CheckParent

        private bool CheckParent(List<int> ids)
        {
            ids.Add(this.Id);
            if (this.Parent == null) //rootfolder..
                return  CheckChildren(new List<int>());
            if (ids.Contains(Parent.Id)) return false;
            var checkedOk = CheckParent(ids);
            return checkedOk;
        }

        private bool CheckChildren(List<int> myIds)
        {
            // myIds contains the Id of the root and all of its descendants
            // In a valid tree all ids should occur only once.
            myIds.Add(this.Id);
            foreach (var child in _children)
            {
                if (myIds.Contains(child.Id))
                {
                    return false;
                }
                var checkChild = child.CheckChildren(myIds);
                if (!checkChild) return false;
            }
            return true;
        }

        //private async Task<List<int>> GetIdTree()
        //{
        // }
        #endregion
        #region ReadChildren
        private async Task ReadChildrenFromDb(FolderTreeViewModel folderTree)
        {
            var tree = _visualTree.Children;
            var db = Database.DbConnection;
            var children = await db.Table<Folder>().Where(f => f.Parent == this._folder.Id).ToListAsync();

            foreach (Folder childfolder in children)
            {
                var child = new FolderViewModel(childfolder, this, _visualTree);
                await AddChild(child);
                await child.ReadChildrenFromDb(folderTree);
            }
            _visualTree.Children = tree;
        }
        #endregion

        private async Task<bool> Save(List<int> children)
        {
            var i = 0; //number of records inserted or updated.
            if (_folder == null) return false;
            var checkedOk = CheckParent(children);
            if (!checkedOk) return false;

            var db = Database.DbConnection;
            if (this.Parent != null)
            {
                children.Add(this.Id);
                if (await this.Parent.Save(children))
                {
                    _folder.Parent = Parent.Id;
                }
                else
                {
                    _parent = null;
                }
            }
            if (_folder.Id == 0)
            {
                i = await db.InsertAsync(_folder);
            }
            else
            {
                i = await db.UpdateAsync(_folder);
            }
            // a folder with no parent should be added to the FolderTree.Children
            // single folders in a root are not visible if it has children (see this.GetRootItems)
            // a new rootfolder should be added to the invisible rootfolder.
            if (Parent == null)
            {
                if (_visualTree.Children == null) _visualTree.Children = new ObservableCollection<FolderViewModel>();
                if (_visualTree.Children.Count == 0)
                {
                    _visualTree.Children.Add(this);
                }
                else
                {
                    _folder.Parent = _visualTree.Children[0]._folder.Parent;
                }
            }
            return (i == 1);
        }
        private async Task SetParent(FolderViewModel parent)
        {
            if (_parent == parent) return;
            if (_parent != null)
            {
                await _parent.RemoveChild(this);
            }
            if (_parent != null)
            {
                if (parent.Id == 0)
                {
                    await parent.Save();
                }
                if (_folder.Parent != Parent.Id)
                {
                    _folder.Parent = Parent.Id;
                    await this.Save();
                    _parent.OnPropertyChanged("Children");
                }
            }
            _path = await this.GetPath();
            OnPropertyChanged("Path");
        }
        internal int Id
        {
            get { return _folder.Id; }
        }

        #region publicConstructors
        public FolderViewModel(string name, FolderTreeViewModel folderTreeTree)
        {
            _folder = new Folder();
            Name = name;
            _visualTree = folderTreeTree;
        }
        #endregion
        #region publicProperties
        public string Name
        {
            get
            {
                if (this._folder == null)
                {
                    return string.Empty;
                }
                return this._folder.Name;
            }
             set
            {
                if (this._folder != null)
                {
                    _folder.Name = value;
                }
            }
        }
        public FolderViewModel Parent
        {
            get
            {
                return this._parent;
            }
            private set 
            { 
                SetParent(value);
            }
        }
        public string Path
        {
            get
            {
                return _path;
            }
        }
#   region IsSelected
        public bool IsSelected
        {
            get { return _isSelected; }
            set
            {
                if (this.SetProperty(ref _isSelected, value) && value)
                {
                    if (!_everSelected)
                    {
                        _everSelected = true;
                    }

                    this._visualTree.SelectedItem = this;
                }
            }
        }
        #endregion

        #region IsExpanded
        private bool _isExpanded;
        private bool _isSelected;
        private bool _everSelected;
        private bool _everExpanded;

        public bool IsExpanded
        {
            get { return _isExpanded; }
            set
            {
                if (this.SetProperty(ref _isExpanded, value) &&
                    value &&
                    !_everExpanded)
                {
                    _everExpanded = true;
                }
            }
        }
        #endregion

        public ObservableCollection<FolderViewModel> Children
        {
            get
            {
                return _children;
            }
        }
#endregion


        #region publicMethods
        public async Task <FolderViewModel> AddChild(string name)
        {
            var child = new FolderViewModel(name, _visualTree);
            var a = await this.AddChild(child);
            return child;
        }
        public async Task<string> GetPath()
        {
            if (this._parent == null)
            {
                return "\\".Insert(2, Name);
            }
            else
            {
                var concatPath = await _parent.GetPath();
                concatPath = string.Concat(concatPath, "\\", Name);
                return concatPath;
            }
        }
        public async Task<bool> AddChild(FolderViewModel Child)
        {
            if (this.CheckParent(new List<int>()))
            {
                var hasParent = false;
                if (Child.Parent != null)
                {
                    hasParent = !(await Child.Parent.RemoveChild(Child));
                }
                if (!hasParent)
                {
                    Child.Parent = this;
                    this._children.Add(Child);
                    this.OnPropertyChanged("Children");
                    return true;
                }
            }
            return false;
        }
        public async Task<bool> RemoveChild(FolderViewModel Child)
        {
            Child.Parent = null;
            await Child.Save();
            if (this._children.Remove(Child))
            {
                this.OnPropertyChanged("Children");
                return true;
            }
            else
            {
                return false;
            }

        }
        public async Task Save()
        {
            if (this._folder != null)
            {
                var parentage = new List<int>();
                var Saved = await this.Save(parentage);
            }
        }
        #endregion
 

    }
}
