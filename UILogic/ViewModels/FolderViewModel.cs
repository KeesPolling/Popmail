
using Prism.Windows.Mvvm;
using PopMail.DataAcces;
using PopMail.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Threading.Tasks;

namespace PopMail.ViewModels
{
    public class FolderViewModel: ViewModelBase
    {
        private ObservableCollection<FolderViewModel> _visualTree;
        private Folder _folder;
        private FolderViewModel _parent;
        private string _path;
        private ObservableCollection<FolderViewModel> _children =  new ObservableCollection<FolderViewModel>();
        public static async Task<ObservableCollection<FolderViewModel>> GetRootItems()
        {
            var db = Database.DbConnection;
            var childList = new ObservableCollection<FolderViewModel>();
            var children = await db.Table<Folder>().Where(f => f.Parent == 0).ToListAsync();
            foreach (Folder childfolder in children)
            {
                var child = new FolderViewModel(childfolder, null, childList );
                childList.Add(child);
                await child.ReadChildrenFromDb();
            }
            return childList;
        }
        #region private Constructors
        internal FolderViewModel(Folder myFolder, FolderViewModel myParent, ObservableCollection<FolderViewModel> visualTree )
        {
            this._folder = myFolder;
            _visualTree = visualTree;
            Parent = myParent;
        }
        #endregion


        #region CheckParent
        private async Task<bool> CheckParent(List<int> Ids)
        {
            if (this.Parent == null)
            {
                var MyIds = await this.GetIdTree();
                MyIds.Sort();
                Ids.Sort();
                var OldMyId = -1;
                var OldId = -1;
                var x = 0;
                foreach (var MyId in MyIds)
                {
                    if(MyId== OldMyId)
                    {
                        throw new OverflowException("Folderstructure corrupt");
                    }
                    OldMyId = MyId;
                    while (x + 1 <= Ids.Count)
                    {
                        if (Ids[x].CompareTo(OldId) == 0)
                        {
                            throw new OverflowException("Folderstructure corrupt");
                        }

                        if (Ids[x].CompareTo(MyId) < 0)
                        {
                            break;
                        }
                        else
                        {
                            OldId = Ids[x];
                            x += 1;
                        }
                    }

                    if (MyId == OldId)
                    {
                        return false;
                    }
                }
                return true;
            }
            else
            { 
                return await Parent.CheckParent(Ids);
            }
        }
        private async Task<List<int>> GetIdTree()
        {
            var MyIds = new List<int>();
            MyIds.Add(this.Id);
            foreach (var Child in _children)
            {
                MyIds.AddRange(await Child.GetIdTree());
            }
            
            return MyIds;
        }
        #endregion

        #region ReadChildren
        private async Task ReadChildrenFromDb()
        {
            var db = Database.DbConnection;
            var ChildList = new ObservableCollection<FolderViewModel>();
            var Children = await db.Table<Folder>().Where(f => f.Parent == this._folder.Id).ToListAsync();

            foreach (Folder Childfolder in Children)
            {
                var Child = new FolderViewModel(Childfolder, this, _visualTree);
                ChildList.Add(Child);
            }
            _children = ChildList;
            OnPropertyChanged("Children");
        }
        #endregion

        private async Task<bool> Save(List<int> Children)
        {
            var Saved = false;
            var i = 0; //number of records inserted or updated.
            if (this._folder != null)
            {
                if (CheckParent(Children).Result)
                {
                    var db = Database.DbConnection;
                    if (this.Parent != null)
                    {
                        Children.Add(this.Id);
                        if (await this.Parent.Save(Children))
                        {
                            this._folder.Parent = Parent.Id;
                        }
                        else
                        {
                            this._parent = null;
                        }
                    }
                    if (this._folder.Id == 0)
                    {
                        i = await db.InsertAsync(_folder);
                    }
                    else
                    {
                        i = await db.UpdateAsync(_folder);
                    }
                    return (i == 1);
                }
            }
            return Saved;
        }
        private async Task SetParent(FolderViewModel parent)
        {
            if (_parent  != null)
            {
                await _parent.RemoveChild(this);
            }
            _parent = parent;
            if (_parent != null)
            {
                if (parent.Id == 0)
                {
                    await parent.Save();
                }
                _folder.Parent = parent.Id;
                await this.Save();
                _parent.OnPropertyChanged("Children");
            }
            _path = await this.GetPath();
            OnPropertyChanged("Path");
        }
        internal int Id
        {
            get { return _folder.Id; }
        }

        #region publicConstructors
        public FolderViewModel(string name, ObservableCollection<FolderViewModel> visualTree)
        {
            _folder = new Folder();
            Name = name;
            _visualTree = visualTree;
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
                    this._folder.Name = value;
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
//        public bool IsSelected
//        {
//            get { return _isSelected; }
//            set
//            {
//                if (this.SetProperty(ref _isSelected, value) && value)
//                {
//                    if (!_everSelected)
//                    {
//                        _everSelected = true;
//                    }

//                    this.TreeModel.SelectedItem = this;
//                }
//            }
//        }
//        #endregion

//        #region IsExpanded
//        private bool _isExpanded;
//        private bool _isSelected;
//        private bool _everSelected;

//        public bool IsExpanded
//        {
//            get { return _isExpanded; }
//            set
//            {
//                if (this.SetProperty(ref _isExpanded, value) &&
//                    value &&
//                    !_everExpanded)
//                {
//                    _everExpanded = true;
//#pragma warning disable 4014
//                    LoadChildrenAsync();
//#pragma warning restore 4014
//                }
//            }
//        }
//        #endregion


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
            var Ids = await Child.GetIdTree();
            if (this.CheckParent(Ids).Result)
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
        #region virtualMethods
#pragma warning disable 1998
        internal virtual async Task LoadPropertiesAsync()
        {
        }

        internal virtual async Task LoadChildrenAsync()
        {
        }

        internal virtual async Task RefreshAsync()
        {
        }
        #endregion virtualMethods

#pragma warning restore 1998

    }
}
