
using Prism.Windows.Mvvm;
using Popmail.UILogic.Models;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Popmail.UILogic.DataAcces;

namespace Popmail.UILogic.ViewModels
{
    public class FolderViewModel: ViewModelBase
    {
        private FolderTreeViewModel _visualTree;
        private Folders _folder;
        private FolderViewModel _parent;
        private ObservableCollection<FolderViewModel> _children =  new ObservableCollection<FolderViewModel>();
        public static async Task GetRootItems(FolderTreeViewModel folderTree)
        {
            Debug.Assert(folderTree != null);
            var db = Database.DbConnection;
            var rootFolders = await db.Table<Folders>().Where(f => f.Parent == 0).ToListAsync();
            //  Skip single rootfolders, unless it does not have children
            while (rootFolders.Count == 1)
            {
                var rootId = rootFolders.First().Id;
                var children = await db.Table<Folders>().Where(f => f.Parent == rootId).ToListAsync();
                if (children.Count == 0) break;
                rootFolders = children;
            }
            var root = new ObservableCollection<FolderViewModel>();
            foreach (Folders folder in rootFolders)
            {
                var folderViewModel = new FolderViewModel(folder,null,folderTree);
                await folderViewModel.ReadChildrenFromDb();
                root.Add(folderViewModel);
            }
            folderTree.Children = root;
        }
        #region private Constructors
        internal FolderViewModel(Folders myFolder, FolderViewModel myParent, FolderTreeViewModel visualTree )
        {
            this._folder = myFolder;
            _visualTree = visualTree;
            _parent = myParent;
        }
        #endregion


        #region CheckParent

        private bool CheckParent(List<int> ids)
        {
            ids.Add(this.Id);
            if (this.Parent == null) //rootfolder..
                return  CheckChildren(new List<int>());
            if (ids.Contains(Parent.Id)) return false;
            var checkedOk = Parent.CheckParent(ids);
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
        private async Task ReadChildrenFromDb()
        {
            var db = Database.DbConnection;
            var children = await db.Table<Folders>().Where(f => f.Parent == this._folder.Id).ToListAsync();

            foreach (Folders childfolder in children)
            {
                var child = new FolderViewModel(childfolder, this, _visualTree);
                _children.Add(child);
                await child.ReadChildrenFromDb();
            }
        }
        #endregion
        #region Expand

        public bool Expand(int id, bool select)
        {
            if (_folder.Id == id)
            {
                if(!select) Expand();
                if (select) IsSelected = true;
                return true;
            }
            foreach (var child in Children)
            {
                if (child.Expand(id, select))
                {
                    Expand();
                    return true;
                }
            }
            return false;
        }

        public void Expand()
        {
            if (!IsExpanded)
            {
                IsExpanded = true;
            }
        }
        #endregion

        private async Task<bool> Save(List<int> children)
        {
            var i = 0; //number of records inserted or updated.
            if (_folder == null) return false;
            var checkedOk = CheckParent(children);
            if (!checkedOk) return false;

            var db = Database.DbConnection;
            if ((_parent != null) && (_parent.Id == 0))
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
            if (_parent == null)
            {
                _visualTree?.Children?.Add(this);
            }
            return (i == 1);
        }
        public async Task<bool> SetParent(FolderViewModel parent)
        {
            if (_parent == parent) return true;
            if (_parent != null)
            {
                await _parent.RemoveChild(this);
            }
            _parent = parent;
            if (parent == null)
            {
                if (_visualTree?.Children?[0] == null)
                {
                    _folder.Parent = 0;
                }
                else
                {
                    _folder.Parent = _visualTree.Children[0]._parent.Id;
                }
                _parent = null;
                await Save(new List<int>());
            }
            else
            {
                if (parent.Id == 0) await parent.Save();
                _folder.Parent = parent.Id;
                var saved = await Save(new List<int>());
                if (!saved)
                {
                    _folder.Parent = 0;
                    _parent = null;
                    return false;
                }
                _parent?.OnPropertyChanged("Children");
            }
            OnPropertyChanged("Path");
            return true;
        }
        internal int Id
        {
            get { return _folder.Id; }
        }

        #region publicConstructors
        public FolderViewModel(string name, FolderTreeViewModel folderTree)
        {
            _folder = new Folders();
            Name = name;
            _visualTree = folderTree;
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

        public FolderViewModel Parent => _parent;

        public string Path => GetPath();
        
        #region IsSelected
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
        private string GetPath()
        {
            if (this._parent == null)
            {
                return "\\".Insert(2, Name);
            }
            else
            {
                var concatPath = _parent.GetPath();
                concatPath = string.Concat(concatPath, "\\", Name);
                return concatPath;
            }
        }
        public async Task<bool> AddChild(FolderViewModel Child)
        {
            if (Child.Parent == this) return true; // do nothing
            {
                var hasParent = false;
                if (Child.Parent != null)
                {
                    hasParent = !(await Child.Parent.RemoveChild(Child));
                }
                if (!hasParent)
                {
                    hasParent = await Child.SetParent(this);
                    if (!hasParent) return false;
                    this._children.Add(Child);
                    OnPropertyChanged("Children");
                    return true;
                }
            }
            return false;
        }
        public async Task<bool> RemoveChild(FolderViewModel Child)
        {
            Child._parent = null;
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
