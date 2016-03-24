
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
        private Folder folder;
        private FolderViewModel parent;
        private string path;
        private ObservableCollection<FolderViewModel> children =  new ObservableCollection<FolderViewModel>();
        public static async Task<ObservableCollection<FolderViewModel>> GetRootItems()
        {
            var db = Database.DbConnection;
            var ChildList = new ObservableCollection<FolderViewModel>();
            var Children = await db.Table<Folder>().Where(f => f.Parent == 0).ToListAsync();
            foreach (Folder Childfolder in Children)
            {
                var Child = new FolderViewModel(Childfolder, null);
                ChildList.Add(Child);
            }
            return ChildList;
        }
        #region private Constructors
        internal FolderViewModel(Folder MyFolder, FolderViewModel MyParent)
        {
            this.folder = MyFolder;
            SetParent(MyParent);
            ReadChildrenFromDb();
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
            foreach (var Child in children)
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
            var Children = await db.Table<Folder>().Where(f => f.Parent == this.folder.Id).ToListAsync();

            foreach (Folder Childfolder in Children)
            {
                var Child = new FolderViewModel(Childfolder, this);
                ChildList.Add(Child);
            }
            children = ChildList;
            OnPropertyChanged("Children");
        }
        #endregion

        private async Task<bool> Save(List<int> Children)
        {
            var Saved = false;
            var i = 0; //number of records inserted or updated.
            if (this.folder != null)
            {
                if (CheckParent(Children).Result)
                {
                    var db = Database.DbConnection;
                    if (this.Parent != null)
                    {
                        Children.Add(this.Id);
                        if (await this.Parent.Save(Children))
                        {
                            this.folder.Parent = Parent.Id;
                        }
                        else
                        {
                            this.parent = null;
                        }
                    }
                    if (this.folder.Id == 0)
                    {
                        i = await db.InsertAsync(folder);
                    }
                    else
                    {
                        i = await db.UpdateAsync(folder);
                    }
                    return (i == 1);
                }
            }
            return Saved;
        }
        private async Task SetParent(FolderViewModel Parent)
        {
            if (parent  != null)
            {
                await parent.RemoveChild(this);
            }
            parent = Parent;
            if (parent != null)
            {
                if (Parent.Id == 0)
                {
                    await Parent.Save();
                }
                folder.Parent = Parent.Id;
                await this.Save();
                parent.OnPropertyChanged("Children");
            }
            path = await this.GetPath();
            OnPropertyChanged("Path");
        }
        internal int Id
        {
            get { return folder.Id; }
        }

        #region publicConstructors
        public FolderViewModel(string Name)
        {
            this.folder = new Folder();
            this.Name = Name;
        }
        #endregion
        #region publicProperties
        public string Name
        {
            get
            {
                if (this.folder == null)
                {
                    return string.Empty;
                }
                return this.folder.Name;
            }
             set
            {
                if (this.folder != null)
                {
                    this.folder.Name = value;
                }
            }
        }
        public FolderViewModel Parent
        {
            get
            {
                return this.parent;
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
                return path;
            }
        }
        public ObservableCollection<FolderViewModel> Children
        {
            get
            {
                return children;
            }
        }
        #endregion

        #region publicMethods
        public async Task <FolderViewModel> AddChild(string Name)
        {
            var Child = new FolderViewModel(Name);
            var a = await this.AddChild(Child);
            return Child;
        }
        public async Task<string> GetPath()
        {
            if (this.parent == null)
            {
                return "\\".Insert(2, Name);
            }
            else
            {
                var concatPath = await parent.GetPath();
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
                    this.children.Add(Child);
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
            if (this.children.Remove(Child))
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
            if (this.folder != null)
            {
                var parentage = new List<int>();
                var Saved = await this.Save(parentage);
            }
        }
        #endregion
    }
}
