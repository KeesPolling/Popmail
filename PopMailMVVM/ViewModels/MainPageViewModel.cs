using System.Collections.ObjectModel;
using System.Collections.Generic;
using PopMail.Models;
using PopMail.DataAcces;
using Prism.Commands;
using Prism.Windows.Mvvm;
using Prism.Windows.Navigation;

using System.Threading.Tasks;
using Windows.UI.Xaml.Controls;


namespace PopMail.ViewModels
{
    public class MainPageViewModel : ViewModelBase
    {
        private readonly INavigationService _navigationService;
        private DelegateCommand<ItemClickEventArgs> _providerProperties;

        #region FolderItems
        private FoldersList _foldersList;
        private ObservableCollection<FolderViewModel> _folderItems;
        public ObservableCollection<FolderViewModel> FolderItems
        {
            get { return _folderItems; }
            set { this.SetProperty(ref _folderItems, value); }
        }
        #endregion
  
        public MainPageViewModel()
        {
          _providerProperties = new DelegateCommand<ItemClickEventArgs>(providerProperies);
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
