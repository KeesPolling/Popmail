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
        private ObservableCollection<FolderViewModel> _folderItems = new ObservableCollection<FolderViewModel>();

        public ObservableCollection<FolderViewModel> FolderItems
        {
            get { return _folderItems; }
            private set { SetProperty(ref _folderItems, value); } 
        }
        #endregion
  
        public MainPageViewModel(INavigationService navigationService)
        {
          _navigationService = navigationService;
          _providerProperties = new DelegateCommand<ItemClickEventArgs>(providerProperies);
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
                FolderItems = await FolderViewModel.GetRootItems();

                if (FolderItems.Count == 0)
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
                // Use the ViewModel to store the SelectedIndex of the FlipView so that the value can be set

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
