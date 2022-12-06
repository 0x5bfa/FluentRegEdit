using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Automation.Peers;
using Microsoft.UI.Xaml.Controls;

namespace RegistryValley.App.UserControls.TreeViewControl
{
    [Microsoft.UI.Xaml.Data.Bindable]
    [Windows.Foundation.Metadata.WebHostHidden]
    public sealed class TreeViewItem : ListViewItem
    {
        public TreeViewItem()
        {
        }

        private ListView GetAncestorListView(TreeViewItem targetItem)
        {
            DependencyObject TreeViewItemAncestor = this;
            ListView ancestorListView = null;

            while (TreeViewItemAncestor != null && ancestorListView == null)
            {
                TreeViewItemAncestor = Microsoft.UI.Xaml.Media.VisualTreeHelper.GetParent(TreeViewItemAncestor);
                ancestorListView = (ListView)TreeViewItemAncestor;
            }

            return ancestorListView;
        }

        protected override void OnDrop(DragEventArgs e)
        {
            if (e.AcceptedOperation == Windows.ApplicationModel.DataTransfer.DataPackageOperation.Move)
            {
                TreeViewItem droppedOnItem = (TreeViewItem)this;

                ListView ancestorListView = GetAncestorListView(droppedOnItem);

                if (ancestorListView != null)
                {
                    TreeView ancestorTreeView = (TreeView)ancestorListView;
                    TreeViewItem droppedItem = ancestorTreeView.draggedTreeViewItem;
                    TreeNode droppedNode = (TreeNode)ancestorTreeView.ItemFromContainer(droppedItem);
                    TreeNode droppedOnNode = (TreeNode)ancestorTreeView.ItemFromContainer(droppedOnItem);

                    //Remove the item that was dragged
                    int removeIndex;
                    removeIndex = droppedNode.ParentNode.IndexOf(droppedNode);

                    if (droppedNode != droppedOnNode)
                    {
                        droppedNode.ParentNode.RemoveAt(removeIndex);

                        //Add the dragged dropped item as a child of the node it was dropped onto
                        droppedOnNode.Add(droppedNode);

                        //If not set to true then the Reorder code of listview wil override what is being done here.
                        e.Handled = true;
                    }
                    else
                    {
                        e.AcceptedOperation = Windows.ApplicationModel.DataTransfer.DataPackageOperation.None;
                    }
                }
            }
        }

        protected override void OnDragEnter(DragEventArgs e)
        {
            TreeViewItem draggedOverItem = (TreeViewItem)this;

            e.AcceptedOperation = Windows.ApplicationModel.DataTransfer.DataPackageOperation.None;

            ListView ancestorListView = GetAncestorListView(draggedOverItem);

            if (ancestorListView != null)
            {
                TreeView ancestorTreeView = (TreeView)ancestorListView;
                TreeViewItem draggedTreeViewItem = ancestorTreeView.draggedTreeViewItem;
                TreeNode draggedNode = (TreeNode)ancestorTreeView.ItemFromContainer(draggedTreeViewItem);
                TreeNode draggedOverNode = (TreeNode)ancestorTreeView.ItemFromContainer(draggedOverItem);
                TreeNode walkNode = draggedOverNode.ParentNode;

                while (walkNode != null && walkNode != draggedNode)
                {
                    walkNode = walkNode.ParentNode;
                }

                if (walkNode != draggedNode && draggedNode != draggedOverNode)
                {
                    e.AcceptedOperation = Windows.ApplicationModel.DataTransfer.DataPackageOperation.Move;
                }
            }
        }

        protected override void OnDragOver(DragEventArgs e)
        {
            e.DragUIOverride.IsGlyphVisible = true;
            e.AcceptedOperation = Windows.ApplicationModel.DataTransfer.DataPackageOperation.None;

            TreeViewItem draggedOverItem = (TreeViewItem)this;

            ListView ancestorListView = GetAncestorListView(draggedOverItem);

            if (ancestorListView != null)
            {
                TreeView ancestorTreeView = (TreeView)ancestorListView;
                TreeViewItem draggedTreeViewItem = ancestorTreeView.draggedTreeViewItem;
                TreeNode draggedNode = (TreeNode)ancestorTreeView.ItemFromContainer(draggedTreeViewItem);
                TreeNode draggedOverNode = (TreeNode)ancestorTreeView.ItemFromContainer(draggedOverItem);
                TreeNode walkNode = draggedOverNode.ParentNode;

                while (walkNode != null && walkNode != draggedNode)
                {
                    walkNode = walkNode.ParentNode;
                }

                if (walkNode != draggedNode && draggedNode != draggedOverNode)
                {
                    e.AcceptedOperation = Windows.ApplicationModel.DataTransfer.DataPackageOperation.Move;
                }
            }
        }

        protected override AutomationPeer OnCreateAutomationPeer()
        {
            return new TreeViewItemAutomationPeer(this);
        }
    }
}
