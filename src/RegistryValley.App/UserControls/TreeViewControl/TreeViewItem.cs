using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Automation.Peers;
using Microsoft.UI.Xaml.Controls;

namespace RegistryValley.App.UserControls.TreeViewControl
{
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

        protected override AutomationPeer OnCreateAutomationPeer()
        {
            return new TreeViewItemAutomationPeer(this);
        }
    }
}
