using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Prism.Windows.Mvvm;

namespace PopMail.DesignViewModels
{
    public class AccountDesignViewModel:ViewModelBase
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
