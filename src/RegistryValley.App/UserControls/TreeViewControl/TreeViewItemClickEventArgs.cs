namespace RegistryValley.App.UserControls.TreeViewControl
{
    public class TreeViewItemClickEventArgs
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
}
