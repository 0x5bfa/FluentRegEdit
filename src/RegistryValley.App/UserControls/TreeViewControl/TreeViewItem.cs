using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Automation.Peers;
using Microsoft.UI.Xaml.Controls;

namespace RegistryValley.App.UserControls.TreeViewControl
{
    public class TreeViewItem : ListViewItem
    {
        public TreeViewItem()
        {
        }

        protected override AutomationPeer OnCreateAutomationPeer()
        {
            return new TreeViewItemAutomationPeer(this);
        }
    }
}
