using System.Collections.ObjectModel;
using System.Collections.Generic;
using PopMail.Models;
using PopMail.DataAcces;
using Microsoft.Practices.Prism.Commands;
using Microsoft.Practices.Prism.Mvvm;
using Microsoft.Practices.Prism.Mvvm.Interfaces;
using Windows.UI.Xaml.Navigation;
using System.Threading.Tasks;
using Windows.UI.Xaml.Controls;


namespace PopMail.ViewModels
{
    public class MainPageViewModel : ViewModel
    {
        private readonly INavigationService _navigationService;
        private DelegateCommand<ItemClickEventArgs> _providerProperties;

        #region FolderItems
        private ObservableCollection<FolderViewModel> _folderItems;
        public ObservableCollection<FolderViewModel> FolderItems
        {
            get { return _folderItems; }
            set { this.SetProperty(ref _folderItems, value); }
        }
        #endregion

        public MainPageViewModel(INavigationService navigationService, ObservableCollection<FolderViewModel> folderItems)
        {
            _navigationService = navigationService;
            _providerProperties = new DelegateCommand<ItemClickEventArgs>(providerProperies);
            _folderItems = FolderItems;
        }
   
        public override async void OnNavigatedTo(object navigationParameter, NavigationMode navigationMode, Dictionary<string, object> viewModelState)
        {
            if (_folderItems.Count == 0)
            {
                _navigationService.Navigate("EmailProvider", null);
            }
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
