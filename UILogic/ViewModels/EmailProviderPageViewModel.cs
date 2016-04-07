using Prism.Commands;
using Prism.Windows.AppModel;
using Prism.Windows.Mvvm;
using Popmail.UILogic.Models;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Threading.Tasks;
using System.Windows.Input;
using Windows.UI.Xaml.Navigation;
using Popmail.UILogic.DataAcces;
using Prism.Windows.Navigation;

namespace Popmail.UILogic.ViewModels
{
    public class EmailProviderPageViewModel : ViewModelBase
    {

        private Accounts _emailProvider;
        private Task _createFolder;
        private bool _hasChanges;
        private INavigationService _navigationService;
        private DelegateCommand _backCommand;
        private FolderTreeViewModel _folderTree;

        private bool FolderIdExists(int folderId)
        {
            var db = Database.DbConnection;
            var folder = db.FindAsync<Folders>(f => f.Id == folderId).Result;
            return (folder != null);
        }

        private async Task AddAllFolders(string rootName)
        {
            var rootFolder = new FolderViewModel(rootName, _folderTree);
            await rootFolder.Save();
            var infolder = await rootFolder.AddChild("In");
            this._emailProvider.InFolderId= infolder.Id;
            var outFolder = await rootFolder.AddChild("Out");
            this._emailProvider.OutFolderId= outFolder.Id;
            var sentFolder = await rootFolder.AddChild("Sent");
            this._emailProvider.SentFolderId = sentFolder.Id;
            var conceptsFolder = await rootFolder.AddChild("Concepts");
            this._emailProvider.ConceptsFolderId = conceptsFolder.Id;
    }
        public EmailProviderPageViewModel(INavigationService navigationService)
        {
            _emailProvider = new Accounts();
            _hasChanges = false;
            _navigationService = navigationService;
            SaveCommand = DelegateCommand.FromAsyncHandler(Save ,() => this.ReadyForSave());
        }

        public override async void OnNavigatedTo(NavigatedToEventArgs e, Dictionary<string, object> viewModelState)
        {
            string errorMessage = string.Empty;
            try
           {
                LoadingData = true;
                var parameter = (AccountPageParameters)e.Parameter;
                if (e.NavigationMode == NavigationMode.New)
                {
                    if ((parameter != null) && (parameter.Account != 0))
                    {
                        _emailProvider = new Accounts();
                    }
                    else
                    {
                        var db = Database.DbConnection;
                       _emailProvider = await db.FindAsync<Accounts>(f => f.Id == parameter.Account);
                        if (_emailProvider == null)
                        {
                            _emailProvider = new Accounts();
                        } 
                    }
                }
               _folderTree = parameter?.FolderTree;
           }
            catch (Exception ex)
            {
                errorMessage = string.Format(CultureInfo.CurrentCulture,
                "GeneralServiceErrorMessage",
                Environment.NewLine,
                ex.Message);
            }
            finally
            {
                LoadingData = false;
            }

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
                SaveCommand.RaiseCanExecuteChanged();
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
        public DelegateCommand GoBackCommand
        {
            get
            {
                if (_backCommand == null)
                {
                    _backCommand = new DelegateCommand(
                        () => _navigationService.GoBack(), () => { return true; });
                }

                return _backCommand;
            }
            set
            {
                _backCommand = value;
            }
        }
        public DelegateCommand SaveCommand { get; private set; }
        public bool LoadingData { get; private set; }

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
                    if (_navigationService != null) //unittest!
                    {
                        if (_navigationService.CanGoBack())
                        {
                            _navigationService.GoBack();
                        }
                    }
                }
            } 
        }
    }
}
