using Prism.Windows.Mvvm;
using PopMail.DataAcces;
using PopMail.Models;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using WinRTXamlToolkit.Controls.Extensions;
using Windows.Foundation;
using Windows.System;
using Windows.UI.Core;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using PopMail.ViewModels;

namespace PopMail.ViewModels
{
    public class FolderTreeViewModel : ViewModelBase
    {

        #region RootElements

        private ObservableCollection<FolderViewModel> _children = new ObservableCollection<FolderViewModel>();

        /// <summary>
        /// Gets or sets the root elements in the visual tree.
        /// </summary>
        public ObservableCollection<FolderViewModel> Children
        {
            get { return _children; }
            set { this.SetProperty(ref _children, value); }
        }

        #endregion

        #region SelectedItem

        private FolderViewModel _selectedItem;

        public FolderViewModel SelectedItem
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

        internal async Task<bool> SelectItem(FolderViewModel element, bool refreshOnFail = false)
        {
            var ancestors = new List<FolderViewModel>();
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

        internal async Task Refresh()
        {
            //if (this.SelectedItem != null)
            //{
            //    await this.SelectedItem.Refresh();
            //}
            //else 
            //if (this.RootElements.Count == 1 &&
            //    this.RootElements[0] is DependencyObjectViewModel &&
            //    ((DependencyObjectViewModel)this.RootElements[0]).Model == Window.Current.Content)
            //{
            //    await this.RootElements[0].RefreshAsync();
            //}
            //else
            //{
            this.Children.Clear();
            await FolderViewModel.GetRootItems(this);
            //}
        }
    }
}
