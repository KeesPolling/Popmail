using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PopMailDemo.MVVM.DataAcces;
using PopMailDemo.MVVM.Model;
using PopMailDemo.MVVM.Utilities;
using SQLite;

namespace PopMailDemo.MVVM.ViewModel
{
    public class EmailProviderVM : BindableBase
    {
        private EmailProvider emailProvider;

        private async Task<bool> FolderIdExists(int FolderId)
        {
            var db = Database.DbConnection;
            var folder = await db.FindAsync<Folder>(f => f.Id == FolderId);
            return (folder != null);
        }
        
        public EmailProviderVM()
        {
            this.emailProvider = new EmailProvider();
        }
        public EmailProviderVM(int Id)
        {
            var db = Database.DbConnection;
            var provider = db.FindAsync<EmailProvider>(e => e.Id == Id).Result;
            if (provider == null)
            {
                throw new ArgumentOutOfRangeException("Id", "Id does not exist");
            }
            this.emailProvider = provider;
        }

        public string Name
        {
            get
            {
                if (this.emailProvider == null)
                {
                    return string.Empty;
                }
                return this.emailProvider.Name;
            }
            set
            {
                if (this.emailProvider != null)
                {
                    this.emailProvider.Name = value;
                    if (this.emailProvider.InFolderId == 0)
                    {
                        FolderVM rootFolder = new FolderVM(value);
                        this.InfolderId = (rootFolder.AddChild("In").Result).Id;
                        this.OutfolderId = (rootFolder.AddChild("Out").Result).Id;
                        this.SentfolderId = (rootFolder.AddChild("Sent").Result).Id;
                        this.ConseptsfolderId = (rootFolder.AddChild("Concepts").Result).Id;
                    }
                    this.OnPropertyChanged();
                }
            }
        }
        public string AccountName
        {
            get
            {
                if (this.emailProvider == null)
                {
                    return string.Empty;
                }
                return this.emailProvider.AccountName;
            }
            set
            {
                if (this.emailProvider != null)
                {
                    this.emailProvider.AccountName = value;
                    this.OnPropertyChanged();
                }
            }
        }
        public string ProviderUri
        {
            get
            {
                if (this.emailProvider == null)
                {
                    return string.Empty;
                }
                return this.emailProvider.ProviderUri;
            }
            set
            {
                if (this.emailProvider != null)
                {
                    this.emailProvider.ProviderUri = value;
                    this.OnPropertyChanged();
                }
            }
        }
        public string ServiceName
        {
            get
            {
                if (this.emailProvider == null)
                {
                    return string.Empty;
                }
                return this.emailProvider.ServiceName;
            }
            set
            {
                if (this.emailProvider != null)
                {
                    this.emailProvider.ServiceName = value;
                    this.OnPropertyChanged();
                }
            }
        }
        public string Password
        {
            get
            {
                if (this.emailProvider == null)
                {
                    return string.Empty;
                }
                return this.emailProvider.Password;
            }
            set
            {
                if (this.emailProvider != null)
                {
                    this.emailProvider.Password = value;
                    this.OnPropertyChanged();
                }
            }
        }
        public string User
        {
            get 
            { 
                if (emailProvider == null)
                {
                    return null;
                }
                return this.emailProvider.User;
            }
            set
            {
                if (this.emailProvider != null)
                {
                    if (value == null)
                    {
                        throw new ArgumentNullException("User", "User is mandatory");
                    }
                    this.emailProvider.User = value;
                }
            }
        }
        public Nullable<int> InfolderId
        {
            get
            {
                if (this.emailProvider == null)
                {
                    return null;
                }
                return this.emailProvider.InFolderId;
            }
            set
            {
                if (this.emailProvider != null)
                {
                    if (value == null)
                    {
                        throw new ArgumentNullException("InFolder", "In folder is mandatory");
                    }
                    if (FolderIdExists((int)value).Result)
                    {
                        this.emailProvider.InFolderId = (int)value;
                        this.OnPropertyChanged();
                    }
                    else
                    {
                        throw new ArgumentOutOfRangeException("InFolder", "In folder does not exist");
                    }
                }
            }
        }
        public Nullable<int> OutfolderId
        {
            get
            {
                if (this.emailProvider == null)
                {
                    return null;
                }
                return this.emailProvider.OutFolderId;
            }
            set
            {
                if (this.emailProvider != null)
                {
                    if (value == null)
                    {
                        throw new ArgumentNullException("OutFolder", "Out folder is mandatory");
                    }
                    if (FolderIdExists((int)value).Result)
                    {
                        this.emailProvider.OutFolderId = (int)value;
                        this.OnPropertyChanged();
                    }
                    else
                    {
                        throw new ArgumentOutOfRangeException("OutFolder", "Out folder does not exist");
                    }
                }
            }
        }
        public Nullable<int> SentfolderId
        {
            get
            {
                if (this.emailProvider == null)
                {
                    return null;
                }
                return this.emailProvider.SentFolderId;
            }
            set
            {
                if (this.emailProvider != null)
                {
                    if (value == null)
                    {
                        throw new ArgumentNullException("SentFolder", "Sent folder is mandatory");
                    }
                    if (FolderIdExists((int)value).Result)
                    {
                        this.emailProvider.SentFolderId = (int)value;
                        this.OnPropertyChanged();
                    }
                    else
                    {
                        throw new ArgumentOutOfRangeException("SentFolder", "Sent folder does not exist");
                    }
                }
            }
        }

        public Nullable<int> ConseptsfolderId
        {
            get
            {
                if (this.emailProvider == null)
                {
                    return null;
                }
                return this.emailProvider.ConceptsFolderId;
            }
            set
            {
                if (this.emailProvider != null)
                {
                    if (value == null)
                    {
                        throw new ArgumentNullException("ConceptsFolder", "Concepts folder is mandatory");
                    }
                    if (FolderIdExists((int)value).Result)
                    {
                        this.emailProvider.ConceptsFolderId = (int)value;
                        this.OnPropertyChanged();
                    }
                    else
                    {
                        throw new ArgumentOutOfRangeException("InFolder", "In folder does not exist");
                    }
                }
            }
        }
        public async Task Save()
        {
            if (this.emailProvider != null)
            {
                if (this.emailProvider.Name != null)
                {
                    var db = Database.DbConnection;

                    if (this.emailProvider.Id == 0)
                    {
                        // New
                        var i = await db.InsertAsync(this.emailProvider);
                    }
                    else
                    {
                        // Update
                        var i = await db.UpdateAsync(this.emailProvider);
                    }
                }
            } 
         }
    }
}
