using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;

namespace RegistryValley.App.UserControls.TreeView
{
    public class TreeView : ListView
    {
        public static readonly DependencyProperty NodeItemSourceProperty =
            DependencyProperty.Register(
                nameof(NodeItemSource),
                typeof(object),
                typeof(TreeView),
                new PropertyMetadata(null)
                );

        public object NodeItemSource
        {
            get => (object)GetValue(NodeItemSourceProperty);
            set => SetValue(NodeItemSourceProperty, value);
        }

        public TreeView()
        {
        }
    }
}
