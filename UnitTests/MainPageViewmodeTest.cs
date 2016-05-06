using System.Collections.Generic;
using Microsoft.VisualStudio.TestPlatform.UnitTestFramework;
using Popmail.UILogic.ViewModels;
using Prism.Windows.Navigation;

namespace PopMail.UnitTests
{
    [TestClass]
    public class MainPageViewmodelTest
    {
        [TestMethod]
        public void  AccountsList()
        {
            var main = new MainPageViewModel(null);
            main.OnNavigatedTo(new NavigatedToEventArgs(), new Dictionary<string, object>());
        }
    }
}
