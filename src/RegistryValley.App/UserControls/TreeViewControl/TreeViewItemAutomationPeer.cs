using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Automation;
using Microsoft.UI.Xaml.Automation.Peers;
using Microsoft.UI.Xaml.Automation.Provider;
using Microsoft.UI.Xaml.Controls;
using System;

namespace RegistryValley.App.UserControls.TreeViewControl
{
    public sealed class TreeViewItemAutomationPeer : ListViewItemAutomationPeer, IExpandCollapseProvider
    {
        internal TreeViewItemAutomationPeer(ListViewItem owner) : base(owner)
        {
        }

        //IExpandCollapseProvider
        public ExpandCollapseState ExpandCollapseState
        {
            get
            {
                ExpandCollapseState currentState = ExpandCollapseState.Collapsed;
                ListView ancestorListView = GetParentListView(Owner);

                TreeNode targetNode;
                TreeNode targetParentNode;

                if (ancestorListView != null)
                {
                    TreeView ancestorTreeView = (TreeView)ancestorListView;
                    targetNode = (TreeNode)ancestorTreeView.ItemFromContainer((TreeViewItem)Owner);

                    if (Owner.AllowDrop)
                    {
                        if (targetNode.IsExpanded)
                        {
                            currentState = ExpandCollapseState.Expanded;
                        }
                        else
                        {
                            currentState = ExpandCollapseState.Collapsed;
                        }
                    }
                    else
                    {
                        currentState = ExpandCollapseState.LeafNode;
                    }
                }

                return currentState;
            }
        }

        public void Collapse()
        {
            ListView ancestorListView = GetParentListView(Owner);

            if (ancestorListView != null)
            {
                TreeView ancestorTreeView = (TreeView)ancestorListView;
                TreeNode targetNode = (TreeNode)ancestorTreeView.ItemFromContainer((TreeViewItem)Owner);
                ancestorTreeView.CollapseNode(targetNode);
                RaiseExpandCollapseAutomationEvent(ExpandCollapseState.Collapsed);
            }
        }

        public void Expand()
        {
            ListView ancestorListView = GetParentListView((DependencyObject)Owner);

            if (ancestorListView != null)
            {
                TreeView ancestorTreeView = (TreeView)ancestorListView;
                TreeNode targetNode = (TreeNode)ancestorTreeView.ItemFromContainer((TreeViewItem)Owner);
                ancestorTreeView.ExpandNode(targetNode);
                RaiseExpandCollapseAutomationEvent(ExpandCollapseState.Expanded);
            }
        }

        public void RaiseExpandCollapseAutomationEvent(ExpandCollapseState newState)
        {
            ExpandCollapseState oldState;

            if (newState == ExpandCollapseState.Expanded)
            {
                oldState = ExpandCollapseState.Collapsed;
            }
            else
            {
                oldState = ExpandCollapseState.Expanded;
            }

            RaisePropertyChangedEvent(ExpandCollapsePatternIdentifiers.ExpandCollapseStateProperty, oldState, newState);
        }

        //Position override

        //These methods are being overridden so that the TreeView under narrator reads out
        //the position of an item as compared to it's children, not it's overall position
        //in the listview. I've included an override for level as well, to give context on
        //how deep in the tree an item is.
        protected override int GetSizeOfSetCore()
        {
            ListView ancestorListView = GetParentListView(Owner);

            TreeNode targetNode;
            TreeNode targetParentNode;

            int setSize = 0;

            if (ancestorListView != null)
            {
                TreeView ancestorTreeView = (TreeView)ancestorListView;
                targetNode = (TreeNode)ancestorTreeView.ItemFromContainer((TreeViewItem)Owner);
                targetParentNode = targetNode.ParentNode;
                setSize = targetParentNode.Size;
            }

            return setSize;
        }

        protected override int GetPositionInSetCore()
        {
            ListView ancestorListView = GetParentListView(Owner);

            TreeNode targetNode;
            TreeNode targetParentNode;

            int positionInSet = 0;

            if (ancestorListView != null)
            {
                TreeView ancestorTreeView = (TreeView)ancestorListView;
                targetNode = (TreeNode)ancestorTreeView.ItemFromContainer((TreeViewItem)Owner);
                int positionInt;
                targetParentNode = targetNode.ParentNode;
                positionInt = targetParentNode.IndexOf(targetNode);
                positionInSet = positionInt + 1;
            }

            return positionInSet;
        }

        protected override int GetLevelCore()
        {
            ListView ancestorListView = GetParentListView(Owner);

            TreeNode targetNode;
            TreeNode targetParentNode;

            int levelValue = 0;

            if (ancestorListView != null)
            {
                TreeView ancestorTreeView = (TreeView)ancestorListView;
                targetNode = (TreeNode)ancestorTreeView.ItemFromContainer((TreeViewItem)Owner);
                levelValue = targetNode.Depth + 1;
            }

            return levelValue;
        }

        protected override object GetPatternCore(PatternInterface patternInterface)
        {
            if (patternInterface == PatternInterface.ExpandCollapse)
            {
                return this;
            }

            return base.GetPatternCore(patternInterface);
        }

        private ListView GetParentListView(DependencyObject Owner)
        {
            DependencyObject treeViewItemAncestor = Owner;
            ListView ancestorListView = null;

            while (treeViewItemAncestor != null && ancestorListView == null)
            {
                treeViewItemAncestor = Microsoft.UI.Xaml.Media.VisualTreeHelper.GetParent(treeViewItemAncestor);
                ancestorListView = (ListView)treeViewItemAncestor;
            }

            return ancestorListView;
        }
    }
}
