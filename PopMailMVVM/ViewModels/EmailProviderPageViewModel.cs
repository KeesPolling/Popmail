using Microsoft.Practices.Prism.Commands;
using Microsoft.Practices.Prism.Mvvm;
using PopMail.Models;
using PopMail.DataAcces;
using System;
using System.Threading.Tasks;
using System.Windows.Input;

namespace PopMail.ViewModels
{
    public class EmailProviderPageViewModel : ViewModel
    {
        private EmailProvider _emailProvider;
        private DelegateCommand _saveCommand;
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
                var rootFolder = new FolderViewModel(RootName);
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
        public EmailProviderPageViewModel()
        {
            this._emailProvider = new EmailProvider();
            this._hasChanges = false;
            this._saveCommand = new DelegateCommand( () => this.Save(),() => this.ReadyForSave());
        }
        public EmailProviderPageViewModel(int Id)
        {
            var db = Database.DbConnection;
            var provider = db.FindAsync<EmailProvider>(e => e.Id == Id).Result;
            if (provider == null)
            {
                throw new ArgumentOutOfRangeException("Id", "Id does not exist");
            }
            this._emailProvider = provider;
        }
        [RestorableState]
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
        [RestorableState]
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
        [RestorableState]
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
        [RestorableState]
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
        [RestorableState]
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
        [RestorableState]
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
        [RestorableState]
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
        [RestorableState]
        public bool HasChanges
        {
            get{return _hasChanges;}
            set
            {
                _hasChanges = value;
                _saveCommand.RaiseCanExecuteChanged();
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
        public ICommand SaveCommand
        {
            get
            {
                return this._saveCommand;
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
                        // New provider
                        var i = await db.InsertAsync(this._emailProvider);
                    }
                    else
                    {
                        // Update provider
                        var i = await db.UpdateAsync(this._emailProvider);
                    }
                    HasChanges = false;
                }
            } 
        }
    }
}
