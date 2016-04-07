using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using Windows.UI.Xaml.Controls;
using Popmail.UILogic.DataAcces;
using Popmail.UILogic.Models;
using Prism.Commands;
using Prism.Windows.Mvvm;
using Prism.Windows.Navigation;

namespace Popmail.UILogic.ViewModels
{
    public class MainPageViewModel : ViewModelBase
    {
        private INavigationService _navigationService;

        public MainPageViewModel(INavigationService navigationService)
        {
          _navigationService = navigationService;
          _providerProperties = new DelegateCommand<ItemClickEventArgs>(ProviderProperties);
        }
        #region AccountsList

        private ObservableCollection<AccountViewModel> _accountsList;

        public ObservableCollection<AccountViewModel> AccountsList
        {
            get { return _accountsList; }
            private set { SetProperty(ref _accountsList,  value); }
        }

        public bool AccountsListVisibility
        {
            get { return true; }
        }

        private int _selectedAccountIndex;
        public int SelectedAccountIndex
        {
            get { return _selectedAccountIndex; }
            set
            {
                SetProperty(ref _selectedAccountIndex, value);
                SettingsMethods.SetSetting("last used account", value.ToString());
            }
        }

        #endregion

        #region FolderItems
        private ObservableCollection<FolderViewModel> _folderItems = new ObservableCollection<FolderViewModel>();

        public ObservableCollection<FolderViewModel> FolderItems
        {
            get { return _folderItems; }
            private set { SetProperty(ref _folderItems, value); }
        }
        #endregion
        #region FolderTree
        private FolderTreeViewModel _folderTree;

        public FolderTreeViewModel FolderTree
        {
            get { return _folderTree;}
            private set { SetProperty(ref _folderTree, value); }
        }
        #endregion
        #region LoadingData
        private bool _loadingData;

        public bool LoadingData
        {
            get { return _loadingData; }
            private set { SetProperty(ref _loadingData, value); }
        }
        #endregion

        public override async void OnNavigatedTo(NavigatedToEventArgs e, Dictionary<string, object> viewModelState)
        {
            string errorMessage = string.Empty;
            try
            {
                LoadingData = true;
                var db = Database.DbConnection;

                var accountsList = new ObservableCollection<AccountViewModel>();
                {
                    var result = await db.QueryAsync<AccountViewModel>("select Id, Name from Accounts");
                    AccountsList = new ObservableCollection<AccountViewModel>();
                    foreach (var accountViewModel in result)
                    {
                        AccountsList.Add(accountViewModel);
                    }
                }

                FolderTree = new FolderTreeViewModel();
                await FolderTree.Refresh();
                FolderItems = FolderTree.Children;

                if (FolderItems.Count == 0)
                {
                    _navigationService.Navigate("EmailProvider",new AccountPageParameters() {FolderTree = _folderTree});
                    return;
                }

                var currentAccount = await SettingsMethods.GetSetting("default account") ??
                                     await SettingsMethods.GetSetting("last used account") ??
                                     "0";

                SelectedAccountIndex = Convert.ToInt32(currentAccount);
                var account = await db.FindAsync<Accounts>(AccountsList[SelectedAccountIndex].Id);
                foreach (var folder in FolderItems)
                {
                    if (folder.Expand(account.InFolderId, true)) break;
                }
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

        #region Commands
        private DelegateCommand<ItemClickEventArgs> _providerProperties;

        public DelegateCommand<ItemClickEventArgs> ToProviderProperties
        {
            get
            {
                return _providerProperties;
            }
        }
        private void ProviderProperties(ItemClickEventArgs args)
        {
            _navigationService.Navigate("EmailProvider",
                new AccountPageParameters() { Account = 0, FolderTree = _folderTree });
        }
        #endregion
    }
}
