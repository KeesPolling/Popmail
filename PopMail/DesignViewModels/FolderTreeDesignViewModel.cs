using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using Windows.UI.Xaml;
using Prism.Windows.Mvvm;

namespace PopMail.DesignViewModels
{
    public class FolderTreeDesignViewModel : ViewModelBase
    {
        #region RootElements

        private ObservableCollection<FolderDesignViewModel> _children = new ObservableCollection<FolderDesignViewModel>();

        /// <summary>
        /// Gets or sets the root elements in the visual tree.
        /// </summary>
        public ObservableCollection<FolderDesignViewModel> Children
        {
            get { return _children; }
            set { _children = value;  }
        }

        #endregion

        #region SelectedItem

        private FolderDesignViewModel _selectedItem;

        public FolderDesignViewModel SelectedItem
        {
            get { return _selectedItem; }
            set
            {
                SetProperty(ref _selectedItem, value);
           }
        }


        #endregion

        #region HighlightMargin

        private Thickness _highlightMargin;

        public Thickness HighlightMargin
        {
            get { return _highlightMargin; }
            set { this.SetProperty(ref _highlightMargin, value); }
        }

        #endregion

        #region HighlightTextMargin

        private Thickness _highlightTextMargin;

        public Thickness HighlightTextMargin
        {
            get { return _highlightTextMargin; }
            set { this.SetProperty(ref _highlightTextMargin, value); }
        }

        #endregion

        #region HighlightText

        private string _highlightText;

        public string HighlightText
        {
            get { return _highlightText; }
            set { this.SetProperty(ref _highlightText, value); }
        }

        #endregion

        #region SelectItem()

        internal async Task<bool> SelectItem(FolderDesignViewModel element, bool refreshOnFail = false)
        {
            var ancestors = new List<FolderDesignViewModel>();
            var current = element;
            while (current.Parent != null)
            {
                ancestors.Add(current.Parent);
                current = current.Parent;
            }
            for (int i = ancestors.Count - 1; i <= 0; i--)
            {
                if (!ancestors[i].IsExpanded)
                {
                    ancestors[i].IsExpanded = true;
                }
            }
            element.IsSelected = true;
            return true;
        }

        #endregion
    }
}
