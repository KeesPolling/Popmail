using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PopMailDemo.MVVM.DataAcces;
using PopMailDemo.MVVM.Model;
using PopMailDemo.MVVM.Utilities;
using PopMailDemo.Common;
using SQLite;

namespace PopMailDemo.MVVM.ViewModel
{
    public class EmailProviderVM : BindableBase
    {
        private EmailProvider emailProvider;
        private RelayCommand saveCommand;
        private Task createFolder;
        private bool hasChanges;

        private bool FolderIdExists(int FolderId)
        {
            var db = Database.DbConnection;
            var folder = db.FindAsync<Folder>(f => f.Id == FolderId).Result;
            return (folder != null);
        }

        private async Task AddAllFolders(string RootName)
        {
                var rootFolder = new FolderVM(RootName);
                await rootFolder.Save();
                var Infolder = await rootFolder.AddChild("In");
                await Infolder.Save();
                this.emailProvider.InFolderId= Infolder.Id;
                var OutFolder = await rootFolder.AddChild("Out");
                await OutFolder.Save();
                this.emailProvider.OutFolderId= OutFolder.Id;
                var SentFolder = await rootFolder.AddChild("Sent");
                await SentFolder.Save();
                this.emailProvider.SentFolderId = SentFolder.Id;
                var ConceptsFolder = await rootFolder.AddChild("Concepts");
                await ConceptsFolder.Save();
                this.emailProvider.ConceptsFolderId = ConceptsFolder.Id;
        }
        public EmailProviderVM()
        {
            this.emailProvider = new EmailProvider();
            this.hasChanges = false;
            this.saveCommand = new RelayCommand( () => this.Save(),() => this.ReadyForSave());
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
        public int Id
        {
            get
            {
                if (this.emailProvider == null)
                {
                    return 0;
                }
                else
                {
                    return this.emailProvider.Id;
                }
            }
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
                    if (this.emailProvider.InFolderId == 0)
                    {
                        createFolder = AddAllFolders(value);
                    } 
                    this.emailProvider.Name = value;
                    this.OnPropertyChanged();
                    HasChanges = true;
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
                    HasChanges = true;
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
                    HasChanges = true;
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
                    HasChanges = true;
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
                    HasChanges = true;
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
                    this.OnPropertyChanged();
                    HasChanges = true;
                }
            }
        }
        public bool HasChanges
        {
            get{return hasChanges;}
            set
            {
                hasChanges = value;
                saveCommand.RaiseCanExecuteChanged();
            }
        }
        public bool ReadyForSave()
        {
            return
                (
                        (emailProvider.Name != null)
                    &&  (emailProvider.AccountName != null)
                    &&  (emailProvider.ProviderUri != null)
                    &&  (emailProvider.ServiceName != null)
                    &&  (emailProvider.User != null)
                    &&  (hasChanges)
                    );
        }
        public System.Windows.Input.ICommand SaveCommand
        {
            get
            {
                return this.saveCommand;
            }
        }
        public async Task Save()
        {
            if (this.emailProvider != null)
            {
                if (ReadyForSave())
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
                    HasChanges = false;
                }
            } 
        }
    }
}
