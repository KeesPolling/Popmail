using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Popmail.UILogic.Models;
using Popmail.UILogic.ViewModels;
using Prism.Windows.Navigation;

namespace PopMail.DesignViewModels
{
    public class MainPageDesignViewModel
    {
        #region FolderItems
        private ObservableCollection<FolderDesignViewModel> _folderItems = new ObservableCollection<FolderDesignViewModel>();

        public ObservableCollection<FolderDesignViewModel> FolderItems
        {
            get { return _folderItems; }
        }
        #endregion
        #region FolderTree
        private FolderTreeDesignViewModel _folderTree = new FolderTreeDesignViewModel();

        public FolderTreeDesignViewModel FolderTree
        {
            get { return _folderTree; }
        }
        #endregion

        private ObservableCollection<AccountDesignViewModel> _accountsList;
        private AccountDesignViewModel _selectedAccount;

        public ObservableCollection<AccountDesignViewModel> AccountsList
        {
            get { return _accountsList; }
            private set { _accountsList = value; }
        }

        public bool AccountsListVisibility
        {
            get { return true;}
        }

        private int _selectedAccountIndex;

        public int SelectedAccountIndex
        {
            get { return _selectedAccountIndex; }
            set { _selectedAccountIndex= value; }
        }
        public MainPageDesignViewModel()
        {
            var root1Folder = new Folders{Name = "Root1", Id = 1,Parent = 0};
            var root1Dvm = new FolderDesignViewModel(root1Folder, null, _folderTree);
            _folderItems.Add(root1Dvm);
            var root2Folder = new Folders{Name = "Root2",Id = 2,Parent = 0};
            var root2Dvm = new FolderDesignViewModel(root2Folder, null, _folderTree);
            _folderItems.Add(root2Dvm);
            var root3Folder = new Folders{Name = "Root3",Id = 3,Parent = 0};
            var root3Dvm = new FolderDesignViewModel(root3Folder, null, _folderTree);
            _folderItems.Add(root3Dvm);
            root2Dvm.AddChild("Child1");
            root2Dvm.AddChild("Child2");
            root2Dvm.AddChild("Child3");
            _folderTree.Children = _folderItems;
            AccountsList = new ObservableCollection<AccountDesignViewModel>()
            {
                new AccountDesignViewModel()
                {
                    Id = 1, Name = "My account"
                },
                new AccountDesignViewModel()
                {
                    Id = 2, Name = "My other account"
                }   
            };

        }
    }
}
