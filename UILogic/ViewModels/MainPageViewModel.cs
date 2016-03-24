using System;
using System.Collections.ObjectModel;
using System.Collections.Generic;
using System.Globalization;
using PopMail.Models;
using PopMail.DataAcces;
using Prism.Commands;
using Prism.Windows.Mvvm;
using Prism.Windows.Navigation;

using System.Threading.Tasks;
using Windows.UI.Xaml.Controls;
using Prism.Logging;


namespace PopMail.ViewModels
{
    public class MainPageViewModel : ViewModelBase
    {
        private INavigationService _navigationService;
        private DelegateCommand<ItemClickEventArgs> _providerProperties;
        private bool _loadingData;

        #region FolderItems
        private FoldersList _foldersList;

        public ObservableCollection<FolderViewModel> FolderItems
        {
            get { return _foldersList.FolderTree; }
        }
        #endregion
  
        public MainPageViewModel(INavigationService navigationService)
        {
          _navigationService = navigationService;
          _providerProperties = new DelegateCommand<ItemClickEventArgs>(providerProperies);
          _foldersList = new FoldersList();
        }
        public bool LoadingData
        {
            get { return _loadingData; }
            private set { SetProperty(ref _loadingData, value); }
        }

        public override async void OnNavigatedTo(NavigatedToEventArgs e, Dictionary<string, object> viewModelState)
        {
            string errorMessage = string.Empty;
            try
            {
                LoadingData = true;
                _foldersList.FolderTree = await _foldersList.BuildTree();
                if (_foldersList.FolderTree.Count == 0)
                {
                    _navigationService.Navigate("EmailProvider",null);
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

            //if (!string.IsNullOrWhiteSpace(errorMessage))
            //{
            //    await _alertMessageService.ShowAsync(errorMessage, _resourceLoader.GetString("ErrorServiceUnreachable"));
            //    return;
            //}
        }
        public DelegateCommand<ItemClickEventArgs> ToProviderProperties
        {
            get
            {
                return _providerProperties;
            }
        }
        private void providerProperies(ItemClickEventArgs args)
        {
            _navigationService.Navigate("EmailProvider", null);
        }
    }
}
