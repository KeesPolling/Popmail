using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestPlatform;
using Microsoft.VisualStudio.TestPlatform.UnitTestFramework;
using Popmail.UILogic.ViewModels;
using Prism.Windows.Navigation;

namespace PopMail.UnitTests
{
    [TestClass]
    public class MainPageViewmodelTest
    {
        [TestMethod]
        public async Task AccountsList()
        {
            var main = new MainPageViewModel(null);
            main.OnNavigatedTo(new NavigatedToEventArgs(), new Dictionary<string, object>());
        }
    }
}
