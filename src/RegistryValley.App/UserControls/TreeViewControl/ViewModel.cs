using Microsoft.UI.Xaml.Controls.Primitives;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices.WindowsRuntime;

namespace RegistryValley.App.UserControls.TreeViewControl
{
    public class ViewModel<T> : Collection<T>
    {
        public ViewModel()
        {
            flatCollectionRealizedItems.CollectionChanged += UpdateTreeView;
        }

        #region Properties
        public ObservableCollection<T> flatCollectionRealizedItems = new();

        public int Size { get => flatCollectionRealizedItems.Count; }

        public event NotifyCollectionChangedEventHandler FlatCollectionChanged;
        #endregion

        #region Event Registration Token Properties/Methods
        public ObservableCollection<EventRegistrationToken> CollectionChangedEventTokenCollection = new();
        public ObservableCollection<EventRegistrationToken> PropertyChangedEventTokenCollection = new();

        private static ConditionalWeakTable<INotifyCollectionChanged, EventRegistrationTokenTable<NotifyCollectionChangedEventHandler>> weakTableForVectorChanged = new();
        private static ConditionalWeakTable<INotifyPropertyChanged, EventRegistrationTokenTable<PropertyChangedEventHandler>> weakTableForPropertyChanged = new();

        private EventRegistrationToken RegisterCollectionChanged(NotifyCollectionChangedEventHandler value)
        {
            INotifyCollectionChanged _this = Unsafe.As<INotifyCollectionChanged>(this);
            EventRegistrationTokenTable<NotifyCollectionChangedEventHandler> table = weakTableForVectorChanged.GetOrCreateValue(_this);

            EventRegistrationToken token = table.AddEventHandler(new NotifyCollectionChangedEventHandler(value));

            return token;
        }

        private EventRegistrationToken RegisterPropertyChanged(PropertyChangedEventHandler value)
        {
            INotifyPropertyChanged _this = Unsafe.As<INotifyPropertyChanged>(this);
            EventRegistrationTokenTable<PropertyChangedEventHandler> table = weakTableForPropertyChanged.GetOrCreateValue(_this);

            EventRegistrationToken token = table.AddEventHandler(value);

            return token;
        }

        private void UnregisterCollectionChanged(EventRegistrationToken token)
        {
            INotifyCollectionChanged _this = Unsafe.As<INotifyCollectionChanged>(this);
            EventRegistrationTokenTable<NotifyCollectionChangedEventHandler> table = weakTableForVectorChanged.GetOrCreateValue(_this);

            var handler = table.ExtractHandler(token);
            if (handler != null)
            {
                _this.CollectionChanged -= handler;
            }
        }

        private void UnregisterPropertyChanged(EventRegistrationToken token)
        {
            INotifyPropertyChanged _this = Unsafe.As<INotifyPropertyChanged>(this);
            EventRegistrationTokenTable<PropertyChangedEventHandler> table = weakTableForPropertyChanged.GetOrCreateValue(_this);

            var handler = table.ExtractHandler(token);
            if (handler != null)
            {
                _this.PropertyChanged -= handler;
            }
        }
        #endregion

        #region Collection Inherited Methods
        public void Add(object value)
        {
            T targetNode = (T)value;
            flatCollectionRealizedItems.Add(targetNode);

            CollectionChangedEventTokenCollection.Add(RegisterCollectionChanged(this.TreeNodeChildrenCollectionChanged));
            ((TreeNode)value).TreeNodeChanged += this.TreeNodeChildrenCollectionChanged;
            PropertyChangedEventTokenCollection.Add(RegisterPropertyChanged(this.TreeNodePropertyChanged));
            ((TreeNode)value).PropertyChanged += this.TreeNodePropertyChanged;
        }

        public new void Clear()
        {
            while (flatCollectionRealizedItems.Count != 0)
            {
                RemoveAtEnd();
            }
        }

        public T First()
        {
            return flatCollectionRealizedItems.First();
        }

        public object GetAt(int index)
        {
            if (!(index > -1 && index < flatCollectionRealizedItems.Count))
            {
                return null;
            }

            return flatCollectionRealizedItems.ElementAt(index);
        }

        public int IndexOf(object value)
        {
            return flatCollectionRealizedItems.IndexOf((T)value);
        }

        public void InsertAt(int index, object value)
        {
            if (!(index > -1 && index <= flatCollectionRealizedItems.Count))
            {
                return;
            }

            T targetNode = (T)value;
            flatCollectionRealizedItems.Insert(index, targetNode);

            CollectionChangedEventTokenCollection.Insert(index, RegisterCollectionChanged(this.TreeNodeChildrenCollectionChanged));
            ((TreeNode)value).TreeNodeChanged += this.TreeNodeChildrenCollectionChanged;
            PropertyChangedEventTokenCollection.Insert(index, RegisterPropertyChanged(this.TreeNodePropertyChanged));
            ((TreeNode)value).PropertyChanged += this.TreeNodePropertyChanged;
        }

        public new void RemoveAt(int index)
        {
            if (!(index > -1 && index < flatCollectionRealizedItems.Count))
            {
                return;
            }

            T targetNode = flatCollectionRealizedItems.ElementAt(index);
            flatCollectionRealizedItems.RemoveAt(index);

            var eventIndex = index;

            UnregisterCollectionChanged(CollectionChangedEventTokenCollection[eventIndex]);
            CollectionChangedEventTokenCollection.RemoveAt(eventIndex);
            UnregisterPropertyChanged(PropertyChangedEventTokenCollection[eventIndex]);
            PropertyChangedEventTokenCollection.RemoveAt(eventIndex);
        }

        public void RemoveAtEnd()
        {
            int index = flatCollectionRealizedItems.Count - 1;
            if (index >= 0)
            {
                T targetNode = flatCollectionRealizedItems.ElementAt(index);
                flatCollectionRealizedItems.RemoveAt(index);

                var eventIndex = index;

                UnregisterCollectionChanged(CollectionChangedEventTokenCollection[eventIndex]);
                CollectionChangedEventTokenCollection.RemoveAt(eventIndex);
                UnregisterPropertyChanged(PropertyChangedEventTokenCollection[eventIndex]);
                PropertyChangedEventTokenCollection.RemoveAt(eventIndex);
            }
        }

        public void SetAt(int index, object value)
        {
            if (!(index > -1 && index < flatCollectionRealizedItems.Count))
            {
                return;
            }

            T targetNode = (T)value;
            T removeNode = flatCollectionRealizedItems.ElementAt(index);
            flatCollectionRealizedItems[index] = (T)value;

            var eventIndex = index;

            UnregisterCollectionChanged(CollectionChangedEventTokenCollection[eventIndex]);
            CollectionChangedEventTokenCollection.RemoveAt(eventIndex);
            CollectionChangedEventTokenCollection.Insert(eventIndex, RegisterCollectionChanged(this.TreeNodeChildrenCollectionChanged));

            UnregisterPropertyChanged(PropertyChangedEventTokenCollection[eventIndex]);
            PropertyChangedEventTokenCollection.RemoveAt(eventIndex);
            PropertyChangedEventTokenCollection.Insert(eventIndex, RegisterPropertyChanged(this.TreeNodePropertyChanged));
        }
        #endregion

        #region Expand/Collapse Sync Methods
        public void ExpandNode(TreeNode targetNode)
        {
            if (!targetNode.IsExpanded)
            {
                targetNode.IsExpanded = true;
            }
        }

        public void CollapseNode(TreeNode targetNode)
        {
            if (targetNode.IsExpanded)
            {
                targetNode.IsExpanded = false;
            }
        }
        #endregion

        public void AddNodeToView(TreeNode targetNode, int index)
        {
            InsertAt(index, targetNode);
        }

        public int AddNodeDescendantsToView(TreeNode targetNode, int index, int offset)
        {
            if (targetNode.IsExpanded)
            {
                TreeNode childNode;
                for (int i = 0; i < targetNode.Size; i++)
                {
                    childNode = (TreeNode)targetNode.GetAt(i);
                    offset++;
                    AddNodeToView(childNode, index + offset);
                    offset = AddNodeDescendantsToView(childNode, index, offset);
                }

                return offset;
            }

            return offset;
        }

        public void RemoveNodeAndDescendantsFromView(TreeNode targetNode)
        {
            if (targetNode.IsExpanded)
            {
                TreeNode childNode;
                for (int i = 0; i < targetNode.Size; i++)
                {
                    childNode = (TreeNode)targetNode.GetAt(i);
                    RemoveNodeAndDescendantsFromView(childNode);
                }
            }

            int index = IndexOf(targetNode);
            RemoveAt(index);
        }

        public int CountDescendants(TreeNode targetNode)
        {
            int descendantCount = 0;
            TreeNode childNode;
            for (int i = 0; i < targetNode.Size; i++)
            {
                childNode = (TreeNode)targetNode.GetAt(i);
                descendantCount++;
                if (childNode.IsExpanded)
                {
                    descendantCount = descendantCount + CountDescendants(childNode);
                }
            }

            return descendantCount;
        }

        public void UpdateTreeView(object sender, NotifyCollectionChangedEventArgs e)
        {
            FlatCollectionChanged?.Invoke(this, e);
        }

        public void TreeNodeChildrenCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            var action = e.Action;

            switch (action)
            {
                // Reset case, commonly seen when a TreeNode is cleared.
                // removes all nodes that need removing then 
                // toggles a collapse/expand to ensure order.
                case NotifyCollectionChangedAction.Reset:
                    {
                        TreeNode resetNode = (TreeNode)sender;
                        int resetIndex = IndexOf(resetNode);

                        if (resetIndex != Size - 1 && resetNode.IsExpanded)
                        {
                            TreeNode childNode = resetNode;
                            TreeNode parentNode = resetNode.ParentNode;
                            int stopIndex;
                            bool isLastRelativeChild = true;

                            while (parentNode != null && isLastRelativeChild)
                            {
                                int relativeIndex;
                                relativeIndex = parentNode.IndexOf(childNode);

                                if (parentNode.Size - 1 != relativeIndex)
                                {
                                    isLastRelativeChild = false;
                                }
                                else
                                {
                                    childNode = parentNode;
                                    parentNode = parentNode.ParentNode;
                                }
                            }

                            if (parentNode != null)
                            {
                                int siblingIndex;
                                siblingIndex = parentNode.IndexOf(childNode);
                                TreeNode siblingNode = (TreeNode)parentNode.GetAt(siblingIndex + 1);
                                stopIndex = IndexOf(siblingNode);
                            }
                            else
                            {
                                stopIndex = Size;
                            }

                            for (int i = stopIndex - 1; i > resetIndex; i--)
                            {
                                if (flatCollectionRealizedItems.ElementAt(i) is TreeNode tn)
                                {
                                    if (tn.ParentNode == null)
                                    {
                                        RemoveNodeAndDescendantsFromView(tn);
                                    }
                                }
                            }

                            if (resetNode.IsExpanded)
                            {
                                CollapseNode(resetNode);
                                ExpandNode(resetNode);
                            }
                        }

                        break;
                    }
                //Inserts the TreeNode into the correct index of the ViewModel
                case NotifyCollectionChangedAction.Add:
                    {
                        //We will find the correct index of insertion by first checking if the
                        //node we are inserting into is expanded. If it is we will start walking
                        //down the tree and counting the open items. This is to ensure we place
                        //the inserted item in the correct index. If along the way we bump into
                        //the item being inserted, we insert there then return, because we don't
                        //need to do anything further.
                        int index = e.NewStartingIndex;
                        TreeNode targetNode = (TreeNode)((TreeNode)sender).GetAt(index);
                        TreeNode parentNode = targetNode.ParentNode;
                        TreeNode childNode;

                        int parentIndex = IndexOf(parentNode);
                        int allOpenedDescendantsCount = 0;

                        if (parentNode.IsExpanded)
                        {
                            for (int i = 0; i < parentNode.Size; i++)
                            {
                                childNode = (TreeNode)parentNode.GetAt(i);

                                if (childNode == targetNode)
                                {
                                    AddNodeToView(targetNode, (parentIndex + i + 1 + allOpenedDescendantsCount));
                                    if (targetNode.IsExpanded)
                                    {
                                        AddNodeDescendantsToView(targetNode, parentIndex + i + 1, allOpenedDescendantsCount);
                                    }

                                    return;
                                }

                                if (childNode.IsExpanded)
                                {
                                    allOpenedDescendantsCount += CountDescendants(childNode);
                                }
                            }

                            AddNodeToView(targetNode, parentIndex + parentNode.Size + allOpenedDescendantsCount);

                            if (targetNode.IsExpanded)
                            {
                                AddNodeDescendantsToView(targetNode, parentIndex + parentNode.Size, allOpenedDescendantsCount);
                            }
                        }

                        break;
                    }
                //Removes a node from the ViewModel when a TreeNode
                //removes a child.
                case NotifyCollectionChangedAction.Remove:
                    {
                        TreeNode removeNode = (TreeNode)sender;
                        int removeIndex = IndexOf(removeNode);

                        if (removeIndex != Size - 1 && removeNode.IsExpanded)
                        {
                            TreeNode childNode = removeNode;
                            TreeNode parentNode = removeNode.ParentNode;
                            int stopIndex;
                            bool isLastRelativeChild = true;
                            while (parentNode != null && isLastRelativeChild)
                            {
                                int relativeIndex;
                                relativeIndex = parentNode.IndexOf(childNode);

                                if (parentNode.Size - 1 != relativeIndex)
                                {
                                    isLastRelativeChild = false;
                                }
                                else
                                {
                                    childNode = parentNode;
                                    parentNode = parentNode.ParentNode;
                                }
                            }

                            if (parentNode != null)
                            {
                                int siblingIndex;
                                siblingIndex = parentNode.IndexOf(childNode);
                                TreeNode siblingNode = (TreeNode)parentNode.GetAt(siblingIndex + 1);
                                stopIndex = IndexOf(siblingNode);
                            }
                            else
                            {
                                stopIndex = Size;
                            }

                            for (int i = stopIndex - 1; i > removeIndex; i--)
                            {
                                if (flatCollectionRealizedItems.ElementAt(i) is TreeNode tn)
                                {
                                    if (tn.ParentNode == null)
                                    {
                                        RemoveNodeAndDescendantsFromView(tn);
                                    }
                                }
                            }
                        }

                        break;
                    }
                //Triggered by a replace such as SetAt.
                //Updates the TreeNode that changed in the 
                case NotifyCollectionChangedAction.Replace:
                    {
                        int index = e.NewStartingIndex;
                        TreeNode targetNode = (TreeNode)((TreeNode)sender).GetAt(index);
                        TreeNode parentNode = targetNode.ParentNode;
                        TreeNode childNode;
                        int allOpenedDescendantsCount = 0;
                        int parentIndex = IndexOf(parentNode);

                        for (int i = 0; i < parentNode.Size; i++)
                        {
                            childNode = (TreeNode)parentNode.GetAt(i);
                            if (childNode.IsExpanded)
                            {
                                allOpenedDescendantsCount += CountDescendants(childNode);
                            }
                        }

                        TreeNode removeNode = (TreeNode)GetAt(parentIndex + index + allOpenedDescendantsCount + 1);
                        if (removeNode.IsExpanded)
                        {
                            CollapseNode(removeNode);
                        }

                        RemoveAt(parentIndex + index + allOpenedDescendantsCount + 1);
                        InsertAt(parentIndex + index + allOpenedDescendantsCount + 1, targetNode);

                        break;
                    }
            }
        }

        public void TreeNodePropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == "IsExpanded")
            {
                TreeNode targetNode = (TreeNode)sender;
                if (targetNode.IsExpanded)
                {
                    if (targetNode.Size != 0)
                    {
                        int openedDescendantOffset = 0;
                        int index = IndexOf(targetNode);
                        index++;
                        TreeNode childNode;

                        for (int i = 0; i < targetNode.Size; i++)
                        {
                            childNode = (TreeNode)targetNode.GetAt(i);
                            AddNodeToView(childNode, (index + i + openedDescendantOffset));
                            openedDescendantOffset = AddNodeDescendantsToView(childNode, (index + i), openedDescendantOffset);
                        }
                    }
                }
                else
                {
                    TreeNode childNode;
                    for (int i = 0; i < targetNode.Size; i++)
                    {
                        childNode = (TreeNode)targetNode.GetAt(i);
                        RemoveNodeAndDescendantsFromView(childNode);
                    }
                }
            }
        }
    }
}
