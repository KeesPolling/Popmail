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
    public class EmailProviderVM : Microsoft.Practices.Prism.Mvvm.BindableBase 
    {
        private EmailProvider _emailProvider;
        private RelayCommand saveCommand;
        private Task _createFolder;
        private bool _hasChanges;

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
                this._emailProvider.InFolderId= Infolder.Id;
                var OutFolder = await rootFolder.AddChild("Out");
                await OutFolder.Save();
                this._emailProvider.OutFolderId= OutFolder.Id;
                var SentFolder = await rootFolder.AddChild("Sent");
                await SentFolder.Save();
                this._emailProvider.SentFolderId = SentFolder.Id;
                var ConceptsFolder = await rootFolder.AddChild("Concepts");
                await ConceptsFolder.Save();
                this._emailProvider.ConceptsFolderId = ConceptsFolder.Id;
        }
        public EmailProviderVM()
        {
            this._emailProvider = new EmailProvider();
            this._hasChanges = false;
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
            this._emailProvider = provider;
        }
        public int Id
        {
            get
            {
                if (this._emailProvider == null)
                {
                    return 0;
                }
                else
                {
                    return this._emailProvider.Id;
                }
            }
        }
        public string Name
        {
            get
            {
                if (this._emailProvider == null)
                {
                    return string.Empty;
                }
                return this._emailProvider.Name;
            }
            set
            {
                if (this._emailProvider != null)
                {
                    if (this._emailProvider.InFolderId == 0)
                    {
                        _createFolder = AddAllFolders(value);
                    } 
                    this._emailProvider.Name = value;
                    HasChanges = true;
                }
            }
        }
        public string AccountName
        {
            get
            {
                if (this._emailProvider == null)
                {
                    return string.Empty;
                }
                return this._emailProvider.AccountName;
            }
            set
            {
                if (this._emailProvider != null)
                {
                    this._emailProvider.AccountName = value;
                    HasChanges = true;
                }
            }
        }
        public string ProviderUri
        {
            get
            {
                if (this._emailProvider == null)
                {
                    return string.Empty;
                }
                return this._emailProvider.ProviderUri;
            }
            set
            {
                if (this._emailProvider != null)
                {
                    this._emailProvider.ProviderUri = value;
                    HasChanges = true;
                }
            }
        }
        public string ServiceName
        {
            get
            {
                if (this._emailProvider == null)
                {
                    return string.Empty;
                }
                return this._emailProvider.ServiceName;
            }
            set
            {
                if (this._emailProvider != null)
                {
                    this._emailProvider.ServiceName = value;
                    HasChanges = true;
                }
            }
        }
        public string Password
        {
           get
            {
                if (this._emailProvider == null)
                {
                    return string.Empty;
                }
                return this._emailProvider.Password;
            }
            set
            {
                if (this._emailProvider != null)
                {
                    this._emailProvider.Password = value;
                    HasChanges = true;
                }
            }
        }
        public string User
        {
            get 
            { 
                if (_emailProvider == null)
                {
                    return null;
                }
                return this._emailProvider.User;
            }
            set
            {
                if (this._emailProvider != null)
                {
                    if (value == null)
                    {
                        throw new ArgumentNullException("User", "User is mandatory");
                    }
                    this._emailProvider.User = value;
                    HasChanges = true;
                }
            }
        }
        public bool HasChanges
        {
            get{return _hasChanges;}
            set
            {
                _hasChanges = value;
                saveCommand.RaiseCanExecuteChanged();
            }
        }
        public bool ReadyForSave()
        {
            return
                (
                        (_emailProvider.Name != null)
                    &&  (_emailProvider.AccountName != null)
                    &&  (_emailProvider.ProviderUri != null)
                    &&  (_emailProvider.ServiceName != null)
                    &&  (_emailProvider.User != null)
                    &&  (_hasChanges)
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
            if (this._emailProvider != null)
            {
                if (ReadyForSave())
                {
                    var db = Database.DbConnection;

                    if (this._emailProvider.Id == 0)
                    {
                        // New
                        var i = await db.InsertAsync(this._emailProvider);
                    }
                    else
                    {
                        // Update
                        var i = await db.UpdateAsync(this._emailProvider);
                    }
                    HasChanges = false;
                }
            } 
        }
    }
}
