using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PopMailDemo.MVVM.Model;
using PopMailDemo.MVVM.Utilities;
using PopMailDemo.MVVM.DataAcces;
using WinRTXamlToolkit.Tools;
using SQLite;
using System.Collections.ObjectModel;

namespace PopMailDemo.MVVM.ViewModel
{
    public class FolderVM :BindableBase
    {
        private Folder folder;
        private FolderVM parent;
        private ObservableCollection<FolderVM> children =  new ObservableCollection<FolderVM>();
        public static async Task<ObservableCollection<FolderVM>> GetRootItems()
        {
            var db = Database.DbConnection;
            var ChildList = new ObservableCollection<FolderVM>();
            var Children = await db.Table<Folder>().Where(f => f.Parent == 0).ToListAsync();
            foreach (Folder Childfolder in Children)
            {
                var Child = new FolderVM(Childfolder, null);
                ChildList.Add(Child);
            }
            return ChildList;
        }
        #region private Constructors
        private FolderVM(Folder MyFolder, FolderVM MyParent)
        {
            this.folder = MyFolder;
            this.parent = MyParent;
            this.children = ReadChildrenFromDb().Result;
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
                    while (x + 1 <= Ids.Count())
                    {
                        if (Ids[x].CompareTo(OldId) == 0)
                        {
                            throw new OverflowException("Folderstructure corrupt");
                        }
                        OldId = Ids[x];

                        if (Ids[x].CompareTo(MyId) < 0)
                        {
                            break;
                        }
                        else
                        {
                            x+=1;
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
        private async Task<ObservableCollection<FolderVM>> ReadChildrenFromDb()
        {
            var db = Database.DbConnection;
            var ChildList = new ObservableCollection<FolderVM>();
            var Children = await db.Table<Folder>().Where(f => f.Parent == this.folder.Id).ToListAsync();

            foreach (Folder Childfolder in Children)
            {
                var Child = new FolderVM(Childfolder, this);
                ChildList.Add(Child);
            }
            return ChildList;
        }
        #endregion

        private async Task<bool> Save(List<int> Children)
        {
            var Saved = false;

            if (this.folder != null)
            {
                if (CheckParent(Children).Result)
                {
                    var db = DataAcces.Database.DbConnection;
                    if (this.Parent != null)
                    {
                        Children.Add(this.Id);
                        if (this.Parent.Save(Children).Result)
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
                        var i = await db.InsertAsync(folder);
                    }
                    else
                    {
                        var i = await db.UpdateAsync(folder);
                    }
                    Saved = true;
                }
            }
            return Saved;
        }
 
        #region publicConstructors
        public FolderVM(string Name)
        {
            this.folder = new Folder();
            this.Name = Name;
            this.Save();
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
                    this.OnPropertyChanged();
                }
            }
        }
        public FolderVM Parent
        {
            get
            {
                return this.parent;
            }
            private set 
            { 
                if (this.Parent != null)
                {
                    Parent.children.Remove(this);
                    Parent.OnPropertyChanged();
                }
                parent = value;
                this.OnPropertyChanged();
                this.Save();
            }
        }
        public ObservableCollection<FolderVM> Children
        {
            get
            {
                return children;
            }
        }
        #endregion
        internal int Id
        {
            get { return folder.Id; }
        }

        #region publicMethods
        public async Task<FolderVM> AddChild(string Name)
        {
            var Child = new FolderVM(Name);
            await this.AddChild(Child);
            this.OnPropertyChanged();
            return Child;
        }
        public async Task<bool> AddChild(FolderVM Child)
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
                    Child.Save();
                    this.OnPropertyChanged();
                    return true;
                }
            }
            return false;
        }
        public async Task<bool> RemoveChild(FolderVM Child)
        {
            Child.Parent = null;
            Child.Save();
            Child.OnPropertyChanged();
            if (this.children.Remove(Child))
            {
                this.OnPropertyChanged();
                return true;
            }
            else
            {
                return false;
            }

        }
        public void Save()
        {
            if (this.folder != null)
            {
                var parentage = new List<int>();
                var Saved = this.Save(parentage).Result;
            }
        }
        #endregion
    }
}
