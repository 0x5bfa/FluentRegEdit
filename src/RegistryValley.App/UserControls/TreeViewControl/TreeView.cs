using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;

namespace RegistryValley.App.UserControls.TreeViewControl
{
    public delegate void TreeViewItemClickHandler(TreeView sender, TreeViewItemClickEventArgs args);

    public class TreeView : ListView
    {
        public TreeView()
        {
            flatViewModel = new();
            rootNode = new TreeNode();

            flatViewModel.ExpandNode(rootNode);

            rootNode.ChildrenCollectionChanged += flatViewModel.TreeNodeChildrenCollectionChanged;
            ItemClick += TreeView_OnItemClick;

            ItemsSource = flatViewModel;

            flatViewModel.FlatCollectionChanged += (_, e) =>
            {
                ItemsSource = flatViewModel;
            };
        }

        #region properties
        private ViewModel<TreeNode> flatViewModel;

        // This event is used to expose an alternative to itemclick to developers.
        public event TreeViewItemClickHandler TreeViewItemClick;

        // This RootNode property is used by the TreeView to handle additions into the TreeView and
        // accurate CollectionChanged with multiple 'root level nodes'. This node will not be placed
        // in the flatViewModel, but has it's ChildrenCollectionChanged event hooked up to flatViewModel's handler.
        private TreeNode rootNode;
        public TreeNode RootNode { get => rootNode; }
        #endregion

        public void TreeView_OnItemClick(object sender, ItemClickEventArgs args)
        {
            TreeViewItemClickEventArgs treeViewArgs = new()
            {
                ClickedItem = args.ClickedItem
            };

            // Trigger listviewitem click event
            TreeViewItemClick(this, treeViewArgs);

            if (!treeViewArgs.IsHandled)
            {
                TreeNode targetNode = (TreeNode)args.ClickedItem;

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
    }
}
