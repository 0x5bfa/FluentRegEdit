using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Navigation;
using RegistryValley.App.Models;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Windows.Foundation;
using Windows.Foundation.Collections;
using System.Collections.Specialized;

namespace RegistryValley.App.UserControls.TreeViewControl
{
    public sealed partial class TreeViewControl : UserControl
    {
        #region propdp
        public static DependencyProperty DataProperty
            = DependencyProperty.RegisterAttached(
                nameof(Data),
                typeof(DataContainer),
                typeof(TreeViewControl),
                new PropertyMetadata(null, OnDataChanged)
            );

        private static void OnDataChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is TreeViewControl c)
            {
                c.OnDataChanged(e);
            }
        }

        public DataContainer Data
        {
            get => (DataContainer)GetValue(DataProperty);
            set => SetValue(DataProperty, value);
        }
        #endregion

        public TreeViewControl()
            => InitializeComponent();

        private void OnDataChanged(DependencyPropertyChangedEventArgs e)
        {
            if (e.NewValue is DataContainer Data)
            {
                if (Data.HiveSource.Count > 0)
                {
                    // Create node from hive items
                    foreach (KeyItem item in Data.HiveSource)
                    {
                        TreeView.RootNode.Add(CreateTreeNode(item));
                    }
                }

                Data.HiveSource.CollectionChanged += DataSource_CollectionChanged;
            }
        }

        private TreeNode CreateTreeNode(KeyItem item)
        {
            TreeNode node = new TreeNode()
            {
                Data = item,
                IsExpanded = item.Expanded
            };

            node.PropertyChanged += Node_PropertyChanged;

            //item.PropertyChanged += (object sender, System.ComponentModel.PropertyChangedEventArgs e) =>
            //{
            //    if (e.PropertyName == nameof(item.Expanded))
            //    {
            //        if (node.IsExpanded)
            //        {
            //            node.IsExpanded = item.Expanded;
            //            TreeView.SelectedItem = node;
            //        }
            //    }
            //};

            // The hive has child objects (== sub keys)
            if (item.Children.Count > 0)
            {
                foreach (KeyItem newItem in item.Children)
                {
                    node.Add(CreateTreeNode(newItem));
                }
            }

            item.Children.CollectionChanged += (object sender, NotifyCollectionChangedEventArgs e) =>
            {
                foreach (KeyItem newItem in e.NewItems)
                {
                    node.Add(CreateTreeNode(newItem));
                }
            };

            return node;
        }

        private void DataSource_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            foreach (KeyItem item in e.NewItems)
            {
                TreeView.RootNode.Add(CreateTreeNode(item));
            }
        }

        private void Node_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            TreeNode node = (TreeNode)sender;
            KeyItem item = (KeyItem)node.Data;

            // Sync expand/collapse state
            if (e.PropertyName == nameof(node.IsExpanded) && node.IsExpanded != item.Expanded)
            {
                if (node.IsExpanded)
                {
                    Data.ItemExpanded(item);
                }
                else
                {
                    Data.ItemCollapsed(item);
                }
            }
        }

        private void TreeView_ItemClick(object sender, ItemClickEventArgs e)
        {
            TreeNode node = (TreeNode)e.ClickedItem;
            KeyItem item = (KeyItem)node.Data;

            Data.ItemInvoked(item);
        }
    }
}
