using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PopMailDemo.MVVM.Model;
using PopMailDemo.MVVM.Utilities;
using PopMailDemo.MVVM.DataAcces;
using SQLite;

namespace PopMailDemo.MVVM.ViewModel
{
    public class FolderVM : BindableBase
    {
        private Folder folder;
        private FolderVM parent;
        private List<FolderVM> children = new List<FolderVM>();

        private bool CheckParent(List<FolderVM> Children)
        {
            if (Children.Contains(this))
            {
                return false;
            }
            if (this.folder.Id != 0)
            {
                return CheckParent(this.folder.Id);
            }
            return true;
        }
        private bool CheckParent(int Id)
        {
            if (this.folder != null)
            {
                if (this.parent != null)
                {
                    if (this.parent.folder.Id == Id)
                    {
                        return false;
                    }
                    else
                    {
                        return this.parent.CheckParent(Id);
                    }
                }
            }
            return true;
        }
        private bool CheckParent()
        {
            if (this.folder != null)
            {
                if (this.folder.Id != 0)
                {
                    return CheckParent(this.folder.Id);
                }
            }
            return true;
        }
        private async Task<List<FolderVM>> ReadChildrenFromDb()
        {
            var db = Database.DbConnection;
            var ChildList = new List<FolderVM>();
            var Children = await db.Table<Folder>().Where(f => f.Parent == this.folder.Id).ToListAsync();

            foreach (Folder Childfolder in Children)
            {
                var Child = new FolderVM(Childfolder.Id);
                ChildList.Add(Child);
            }
            return ChildList;
        }
        public FolderVM(Folder folder)
        {

            this.folder = folder;
            if (this.folder.Parent != 0)
            {
                this.parent = new FolderVM(this.folder.Parent);
            }
            children = ReadChildrenFromDb().Result;
        }
        public FolderVM(int Id)
        {
            var db = Database.DbConnection;
            var ChildList = new List<FolderVM>();

            this.folder = db.FindAsync<Folder>( f => f.Id == Id).Result;
            if (this.folder == null)
            {
                throw new ArgumentException("Folder bestaat niet");
            }
            if (this.folder.Parent != 0)
            {
                this.parent = new FolderVM(this.folder.Parent);
            }
            
            children = ReadChildrenFromDb().Result;
        }
        public FolderVM(string Name, FolderVM Parent)
        {
            this.parent = Parent;
            var db = Database.DbConnection;
            if (Parent != null)
            {
                if (Parent.folder.Id != 0) // Als dit 0 is is de parent nog niet opgeslagen
                {
                    this.folder = db.FindAsync<Folder>(f => (f.Parent == Parent.folder.Id) && (f.Name == Name)).Result;
                }
            }
            else
            {
                this.folder = db.FindAsync<Folder>(f => (f.Parent == 0) && (f.Name == Name)).Result;
            }
            if (this.folder == null)
            {
                this.folder = new Folder();
                this.folder.Name = Name;
            }
            else
            {
                children = ReadChildrenFromDb().Result;
            }
            if (Parent != null)
            {
                Parent.AddChild(this);
                this.folder.Parent = Parent.folder.Id;
            }
        }
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
            set 
            { 
                if (this.folder != null)
                {
                    if (value != null)
                    {
                        if (this.folder.Id != 0)
                        {
                            if (!CheckParent(this.folder.Id))
                            {
                                throw new ArgumentException("recursie in folders is not allowed");
                            }
                        }
                    }
                    parent = value;
                }
            }
        }
        public List<FolderVM> Children
        {
            get
            {
                return children;
            }
        }
        public FolderVM AddChild(string Name)
        {
            var Child = new FolderVM(Name, this);
            return Child;
        }
        public void AddChild(FolderVM Child)
        {
            Child.Parent.RemoveChild(Child);
            Child.Parent = this;
            children.Add(Child);
        }
        public bool RemoveChild(FolderVM Child)
        {
            Child.Parent = null;
            return children.Remove(Child);
        }
        private async Task<bool> Save(List<FolderVM> Children)
        {
            var Saved = false;

            if (this.folder != null)
            {
                if (CheckParent(Children))
                {
                    var db = DataAcces.Database.DbConnection;
                    if (this.Parent != null)
                    {
                        Children.Add(this);
                        await this.Parent.Save(Children);
                        if (Saved)
                        {
                            this.folder.Parent = Parent.folder.Id;
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
        public async Task Save()
        {
            if (children.Count > 0)
            {
                foreach (FolderVM Child in children)
                {
                    await Child.Save();
                }
            }
            else
            {
                if (this.folder != null)
                {
                    if (CheckParent())
                    {
                        var db = DataAcces.Database.DbConnection;
                        if (this.Parent != null)
                        {
                            var parentage = new List<FolderVM>();
                            parentage.Add(this);
                            var Saved = await this.Parent.Save(parentage);
                            if (Saved)
                            {
                                this.folder.Parent = Parent.folder.Id;
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
                    }
                }
            }
        }
    }
}