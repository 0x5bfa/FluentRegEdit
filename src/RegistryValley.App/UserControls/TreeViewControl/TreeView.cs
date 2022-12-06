using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using System;
using System.Linq;
using Windows.ApplicationModel.DataTransfer;

namespace RegistryValley.App.UserControls.TreeViewControl
{
    public sealed class TreeViewItemClickEventArgs
    {
        public TreeViewItemClickEventArgs()
        {
        }

        private object clickedItem = null;
        public object ClickedItem
        {
            get => clickedItem;
            set => clickedItem = value;
        }

        private bool isHandled = false;
        public bool IsHandled
        {
            get => isHandled;
            set => isHandled = value;
        }
    }

    public delegate void TreeViewItemClickHandler(TreeView sender, TreeViewItemClickEventArgs args);

    public sealed class TreeView : ListView
    {
        public TreeView()
        {
            flatViewModel = new ViewModel();
            rootNode = new TreeNode();

            flatViewModel.ExpandNode(rootNode);

            CanReorderItems = true;
            AllowDrop = true;
            CanDragItems = true;

            rootNode.VectorChanged += flatViewModel.TreeNodeVectorChanged;
            ItemClick += TreeView_OnItemClick;
            DragItemsStarting += TreeView_DragItemsStarting;
            DragItemsCompleted += TreeView_DragItemsCompleted;

            ItemsSource = flatViewModel;
        }

        #region properties
        //This event is used to expose an alternative to itemclick to developers.
        public event TreeViewItemClickHandler TreeViewItemClick;

        //This RootNode property is used by the TreeView to handle additions into the TreeView and
        //accurate VectorChange with multiple 'root level nodes'. This node will not be placed
        //in the flatViewModel, but has it's vectorchanged event hooked up to flatViewModel's
        //handler.
        private TreeNode rootNode;
        public TreeNode RootNode { get => rootNode; }

        private ViewModel flatViewModel;

        internal TreeViewItem draggedTreeViewItem;
        #endregion

        public void TreeView_OnItemClick(object sender, ItemClickEventArgs args)
        {
            TreeViewItemClickEventArgs treeViewArgs = new();
            treeViewArgs.ClickedItem = args.ClickedItem;

            TreeViewItemClick(this, treeViewArgs);

            if (!treeViewArgs.IsHandled)
            {
                TreeNode  targetNode = (TreeNode)args.ClickedItem;

                if (targetNode.IsExpanded)
                {
                    flatViewModel.CollapseNode(targetNode);
                }
                else
                {
                    flatViewModel.ExpandNode(targetNode);
                }
            }
        }

        public void TreeView_DragItemsStarting(object sender, DragItemsStartingEventArgs e)
        {
            draggedTreeViewItem = (TreeViewItem)this.ContainerFromItem(e.Items.ElementAt(0));
        }

        public void TreeView_DragItemsCompleted(ListViewBase sender, DragItemsCompletedEventArgs args)
        {
            draggedTreeViewItem = null;
        }

        public void ExpandNode(TreeNode targetNode)
        {
            flatViewModel.ExpandNode(targetNode);
        }

        public void CollapseNode(TreeNode targetNode)
        {
            flatViewModel.CollapseNode(targetNode);
        }

        protected override void PrepareContainerForItemOverride(DependencyObject element, object item)
        {
            ((UIElement)element).AllowDrop = true;

            base.PrepareContainerForItemOverride(element, item);
        }

        protected override DependencyObject GetContainerForItemOverride()
        {
            TreeViewItem targetItem = new();
            return targetItem;
        }

        protected override void OnDrop(DragEventArgs e)
        {
            if (e.AcceptedOperation == DataPackageOperation.Move)
            {
                Panel panel = this.ItemsPanelRoot;
                Windows.Foundation.Point point = e.GetPosition(panel);

                int aboveIndex = -1;
                int belowIndex = -1;
                int relativeIndex = 0;

                IInsertionPanel insertionPanel = (IInsertionPanel)panel;

                if (insertionPanel != null)
                {
                    insertionPanel.GetInsertionIndexes(point, out aboveIndex, out belowIndex);

                    TreeNode aboveNode = (TreeNode)flatViewModel.GetAt(aboveIndex);
                    TreeNode belowNode = (TreeNode)flatViewModel.GetAt(belowIndex);
                    TreeNode targetNode = (TreeNode)this.ItemFromContainer(draggedTreeViewItem);

                    //Between two items
                    if (aboveNode != null && belowNode != null)
                    {
                        relativeIndex = targetNode.ParentNode.IndexOf(targetNode);
                        targetNode.ParentNode.RemoveAt(relativeIndex);

                        if (belowNode.ParentNode == aboveNode)
                        {
                            aboveNode.InsertAt(0, targetNode);
                        }
                        else
                        {
                            relativeIndex = aboveNode.ParentNode.IndexOf(aboveNode);
                            aboveNode.ParentNode.InsertAt(relativeIndex + 1, targetNode);
                        }
                    }
                    //Bottom of the list
                    else if (aboveNode != null && belowNode == null)
                    {
                        relativeIndex = targetNode.ParentNode.IndexOf(targetNode);
                        targetNode.ParentNode.RemoveAt(relativeIndex);

                        relativeIndex = aboveNode.ParentNode.IndexOf(aboveNode);
                        aboveNode.ParentNode.InsertAt(relativeIndex + 1, targetNode);
                    }
                    //Top of the list
                    else if (aboveNode == null && belowNode != null)
                    {
                        relativeIndex = targetNode.ParentNode.IndexOf(targetNode);
                        targetNode.ParentNode.RemoveAt(relativeIndex);

                        rootNode.InsertAt(0, targetNode);
                    }
                }
            }

            e.Handled = true;
            base.OnDrop(e);
        }

        protected override void OnDragOver(DragEventArgs e)
        {
            DataPackageOperation savedOperation = DataPackageOperation.None;

            e.AcceptedOperation = DataPackageOperation.None;

            Panel panel = this.ItemsPanelRoot;
            Windows.Foundation.Point point = e.GetPosition(panel);

            int aboveIndex = -1;
            int belowIndex = -1;

            IInsertionPanel  insertionPanel = (IInsertionPanel )panel;

            if (insertionPanel != null)
            {
                insertionPanel.GetInsertionIndexes(point, out aboveIndex, out belowIndex);

                if (aboveIndex > -1)
                {
                    TreeNode aboveNode = (TreeNode)flatViewModel.GetAt(aboveIndex);
                    TreeNode targetNode = (TreeNode)this.ItemFromContainer(draggedTreeViewItem);

                    TreeNode  ancestorNode = aboveNode;

                    while (ancestorNode != null && ancestorNode != targetNode)
                    {
                        ancestorNode = ancestorNode.ParentNode;
                    }

                    if (ancestorNode == null)
                    {
                        savedOperation = DataPackageOperation.Move;
                        e.AcceptedOperation = DataPackageOperation.Move;
                    }
                }
                else
                {
                    savedOperation = DataPackageOperation.Move;
                    e.AcceptedOperation = DataPackageOperation.Move;
                }
            }

            base.OnDragOver(e);
            e.AcceptedOperation = savedOperation;
        }
    }
}
