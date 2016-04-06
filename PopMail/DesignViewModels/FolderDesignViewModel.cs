
using Prism.Windows.Mvvm;
using PopMail.DataAcces;
using Popmail.UILogic.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices.ComTypes;
using System.Threading.Tasks;
using Windows.UI.Notifications;
using SQLite.Net.Interop;

namespace PopMail.DesignViewModels
{
    public class FolderDesignViewModel: ViewModelBase
    {
        private FolderTreeDesignViewModel _visualTree;
        private Folder _folder;
        private FolderDesignViewModel _parent;
        private string _path;
        private ObservableCollection<FolderDesignViewModel> _children =  new ObservableCollection<FolderDesignViewModel>();

        #region private Constructors
        internal FolderDesignViewModel(Folder myFolder, FolderDesignViewModel myParent, FolderTreeDesignViewModel visualTree )
        {
            this._folder = myFolder;
            _visualTree = visualTree;
            Parent = myParent;
        }
        #endregion
        

         internal int Id
        {
            get { return _folder.Id; }
        }

        #region publicConstructors
        public FolderDesignViewModel(string name, FolderTreeDesignViewModel folderTree)
        {
            _folder = new Folder();
            Name = name;
            _visualTree = folderTree;
        }
        #endregion
        #region publicProperties
        public string Name
        {
            get
            {
                if (this._folder == null)
                {
                    return string.Empty;
                }
                return this._folder.Name;
            }
             set
            {
                if (this._folder != null)
                {
                    _folder.Name = value;
                }
            }
        }
        public FolderDesignViewModel Parent
        {
            get
            {
                return this._parent;
            }
            private set 
            { 
                _parent  = (value);
            }
        }
        public string Path
        {
            get
            {
                return _path;
            }
        }
#   region IsSelected
        public bool IsSelected
        {
            get { return _isSelected; }
            set
            {
                if (this.SetProperty(ref _isSelected, value) && value)
                {
                    if (!_everSelected)
                    {
                        _everSelected = true;
                    }

                    this._visualTree.SelectedItem = this;
                }
            }
        }
        #endregion

        #region IsExpanded
        private bool _isExpanded;
        private bool _isSelected;
        private bool _everSelected;
        private bool _everExpanded;

        public bool IsExpanded
        {
            get { return _isExpanded; }
            set
            {
                if (this.SetProperty(ref _isExpanded, value) &&
                    value &&
                    !_everExpanded)
                {
                    _everExpanded = true;
                }
            }
        }
        #endregion

        public ObservableCollection<FolderDesignViewModel> Children
        {
            get
            {
                return _children;
            }
        }
#endregion


        #region publicMethods
        public FolderDesignViewModel AddChild(string name)
        {
            var child = new FolderDesignViewModel(name, _visualTree);
            AddChild(child);
            return child;
        }
        public async Task<string> GetPath()
        {
            if (this._parent == null)
            {
                return "\\".Insert(2, Name);
            }
            else
            {
                var concatPath = await _parent.GetPath();
                concatPath = string.Concat(concatPath, "\\", Name);
                return concatPath;
            }
        }
        public bool AddChild(FolderDesignViewModel Child)
        {
            Child.Parent = this;
            this._children.Add(Child);
            this.OnPropertyChanged("Children");
            return true;
        }
        #endregion
 

    }
}
