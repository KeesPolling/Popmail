using Prism.Windows.Mvvm;

namespace Popmail.UILogic.ViewModels
{
    public class AccountViewModel:ViewModelBase
    {
        private int _id;
        private string _name;

        public int Id
        {
            get { return _id; }
            set { SetProperty(ref _id, value); }
        }

        public string Name
        {
            get { return _name; }
            set { SetProperty(ref _name, value); }
        }
    }
}
