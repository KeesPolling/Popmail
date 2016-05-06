using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Prism.Commands;
using Prism.Windows.Navigation;

namespace Popmail.UILogic.ViewModels
{
    class EmailProviderIPageViewModel
    {
        private INavigationService _navigationService;

        public EmailProviderIPageViewModel(INavigationService navigationService)
        {
            _navigationService = navigationService;
            GoBackCommand = new DelegateCommand(GoBack, () =>  true);
        }
        public DelegateCommand GoBackCommand {  get; private set; }
     
        public void GoBack()
        {
            _navigationService.GoBack();
        }
    }
}
