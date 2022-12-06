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
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;

namespace RegistryValley.App.UserControls.TreeViewControl
{
    public sealed partial class TreeViewControl : UserControl
    {
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

        public TreeViewControl()
        {
            InitializeComponent();
        }


        private void OnDataChanged(DependencyPropertyChangedEventArgs e)
        {
            if (e.NewValue is DataContainer Data)
            {
                if (Data.HiveSource.Count > 0)
                {
                    foreach (HiveItem item in Data.HiveSource)
                    {
                        TreeView.RootNode.Add(CreateTreeNode(item));
                    }
                }

                Data.HiveSource.CollectionChanged += DataSource_CollectionChanged;
            }
        }

        private void DataSource_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            foreach (HiveItem item in e.NewItems)
            {
                TreeView.RootNode.Add(CreateTreeNode(item));
            }
        }

        private TreeNode CreateTreeNode(HiveItem item)
        {
            TreeNode node = new TreeNode()
            {
                Data = item,
                IsExpanded = item.Expanded
            };

            /*item.PropertyChanged += (object sender, System.ComponentModel.PropertyChangedEventArgs e) =>
            {
                if (e.PropertyName == nameof(item.Expanded))
                {
                    if (node.IsExpanded)
                    {
                        node.IsExpanded = item.Expanded;
                        TreeView.SelectedItem = node;
                    }
                }
            };*/

            node.PropertyChanged += Node_PropertyChanged;

            if (item.Children.Count > 0)
            {
                foreach (HiveItem newItem in item.Children)
                {
                    node.Add(CreateTreeNode(newItem));
                }
            }

            item.Children.CollectionChanged += (object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e) =>
            {
                foreach (HiveItem newItem in e.NewItems)
                {
                    node.Add(CreateTreeNode(newItem));
                }
            };

            return node;
        }

        private void Node_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            TreeNode node = (TreeNode)sender;
            HiveItem item = (HiveItem)node.Data;

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
            HiveItem item = (HiveItem)node.Data;

            Data.ItemInvoked(item);
        }
    }
}
