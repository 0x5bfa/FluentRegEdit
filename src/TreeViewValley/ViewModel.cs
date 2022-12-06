using Microsoft.UI.Xaml.Interop;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices.WindowsRuntime;

namespace TreeViewValley
{
    public sealed class ViewModel
    {
        public ViewModel()
        {
            flatVectorRealizedItems.CollectionChanged += UpdateTreeView;
        }

        public int Size
        {
            get => flatVectorRealizedItems.Count;
        }

        public event NotifyCollectionChangedEventHandler VectorChanged;

        private event NotifyCollectionChangedEventHandler ViewModelChanged;

        public ObservableCollection<TreeNode> flatVectorRealizedItems = new();

        public ObservableCollection<EventRegistrationToken> collectionChangedEventTokenVector;

        public ObservableCollection<EventRegistrationToken> propertyChangedEventTokenVector;

        private static ConditionalWeakTable<INotifyCollectionChanged, EventRegistrationTokenTable<NotifyCollectionChangedEventHandler>> weakTableForVectorChanged =
            new ConditionalWeakTable<INotifyCollectionChanged, EventRegistrationTokenTable<NotifyCollectionChangedEventHandler>>();

        private static ConditionalWeakTable<INotifyPropertyChanged, EventRegistrationTokenTable<PropertyChangedEventHandler>> weakTableForPropertyChanged =
            new ConditionalWeakTable<INotifyPropertyChanged, EventRegistrationTokenTable<PropertyChangedEventHandler>>();

        public void Append(object value)
        {
            TreeNode targetNode = (TreeNode)value;
            flatVectorRealizedItems.Append(targetNode);

            collectionChangedEventTokenVector.Add(add_CollectionChanged(this.TreeNodeVectorChanged));
            targetNode.TreeNodeChanged += this.TreeNodeVectorChanged;
            propertyChangedEventTokenVector.Add(add_PropertyChanged(this.TreeNodePropertyChanged));
            targetNode.PropertyChanged += this.TreeNodePropertyChanged;
        }

        internal EventRegistrationToken add_CollectionChanged(NotifyCollectionChangedEventHandler value)
        {
            INotifyCollectionChanged _this = Unsafe.As<INotifyCollectionChanged>(this);
            EventRegistrationTokenTable<NotifyCollectionChangedEventHandler> table = weakTableForVectorChanged.GetOrCreateValue(_this);

            EventRegistrationToken token = table.AddEventHandler(new NotifyCollectionChangedEventHandler(value));

            return token;
        }

        internal EventRegistrationToken add_PropertyChanged(PropertyChangedEventHandler value)
        {
            INotifyPropertyChanged _this = Unsafe.As<INotifyPropertyChanged>(this);
            EventRegistrationTokenTable<PropertyChangedEventHandler> table = weakTableForPropertyChanged.GetOrCreateValue(_this);

            EventRegistrationToken token = table.AddEventHandler(value);
            _this.PropertyChanged += value;

            return token;
        }

        internal void remove_CollectionChanged(EventRegistrationToken token)
        {
            INotifyCollectionChanged _this = Unsafe.As<INotifyCollectionChanged>(this);
            EventRegistrationTokenTable<NotifyCollectionChangedEventHandler> table = weakTableForVectorChanged.GetOrCreateValue(_this);

            table.RemoveEventHandler(token);
            //if (handler != null)
            //{
            //    _this.CollectionChanged -= handler;
            //}
        }

        internal void remove_PropertyChanged(EventRegistrationToken token)
        {
            INotifyPropertyChanged _this = Unsafe.As<INotifyPropertyChanged>(this);
            EventRegistrationTokenTable<PropertyChangedEventHandler> table = weakTableForPropertyChanged.GetOrCreateValue(_this);

            table.RemoveEventHandler(token);
            //if (handler != null)
            //{
            //    _this.PropertyChanged -= handler;
            //}
        }

        public void Clear()
        {
            while (flatVectorRealizedItems.Count != 0)
            {
                RemoveAtEnd();
            }
        }

        public TreeNode First()
        {
            return flatVectorRealizedItems.First();
        }

        public object GetAt(int index)
        {
            if (index > -1 && index < flatVectorRealizedItems.Count)
            {
                return flatVectorRealizedItems.ElementAt(index);
            }

            return null;
        }

        //public IBindableVectorView GetView()
        //{
        //    return (IBindableVectorView)flatVectorRealizedItems.GetView();
        //}

        public int IndexOf(object value)
        {
            return flatVectorRealizedItems.IndexOf((TreeNode)value);
        }

        public void InsertAt(int index, object value)
        {
            if (index > -1 && index <= flatVectorRealizedItems.Count)
            {
                TreeNode targetNode = (TreeNode)value;
                flatVectorRealizedItems.Insert(index, targetNode);

                collectionChangedEventTokenVector.Insert(index, add_CollectionChanged(this.TreeNodeVectorChanged));
                targetNode.TreeNodeChanged += this.TreeNodeVectorChanged;
                propertyChangedEventTokenVector.Insert(index, add_PropertyChanged(this.TreeNodePropertyChanged));
                targetNode.PropertyChanged += this.TreeNodePropertyChanged;
            }
        }

        public void RemoveAt(int index)
        {
            if (index > -1 && index < flatVectorRealizedItems.Count)
            {
                TreeNode targetNode = flatVectorRealizedItems.ElementAt(index);
                flatVectorRealizedItems.RemoveAt(index);

                var eventIndex = index;

                remove_CollectionChanged(collectionChangedEventTokenVector[eventIndex]);
                collectionChangedEventTokenVector.RemoveAt(eventIndex);
                remove_CollectionChanged(propertyChangedEventTokenVector[eventIndex]);
                propertyChangedEventTokenVector.RemoveAt(eventIndex);

            }
        }

        public void RemoveAtEnd()
        {
            int index = flatVectorRealizedItems.Count - 1;
            if (index >= 0)
            {
                TreeNode targetNode = flatVectorRealizedItems.ElementAt(index);
                flatVectorRealizedItems.RemoveAt(index);

                var eventIndex = index;

                remove_CollectionChanged(collectionChangedEventTokenVector[eventIndex]);
                collectionChangedEventTokenVector.RemoveAt(eventIndex);
                remove_CollectionChanged(propertyChangedEventTokenVector[eventIndex]);
                propertyChangedEventTokenVector.RemoveAt(eventIndex);
            }
        }

        public void SetAt(int index, object value)
        {
            if (index > -1 && index < flatVectorRealizedItems.Count)
            {
                TreeNode targetNode = (TreeNode)value;
                TreeNode removeNode = flatVectorRealizedItems.ElementAt(index);
                flatVectorRealizedItems[index] = targetNode;

                var eventIndex = index;

                remove_CollectionChanged(collectionChangedEventTokenVector[eventIndex]);
                collectionChangedEventTokenVector.RemoveAt(eventIndex);
                collectionChangedEventTokenVector.Insert(eventIndex, add_CollectionChanged(this.TreeNodeVectorChanged));

                remove_CollectionChanged(propertyChangedEventTokenVector[eventIndex]);
                propertyChangedEventTokenVector.RemoveAt(eventIndex);
                propertyChangedEventTokenVector.Insert(eventIndex, add_PropertyChanged(this.TreeNodePropertyChanged));
            }
        }

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

        //public int IndexOf(TreeNode targetNode)
        //{
        //    int index;
        //    int isIndexed = IndexOf(targetNode);

        //    if (isIndexed != -1)
        //    {
        //        return index;
        //    }
        //    else
        //    {
        //        return -1;
        //    }
        //}

        public void UpdateTreeView(object sender, NotifyCollectionChangedEventArgs e)
        {
            VectorChanged(this, e);
        }

        public void TreeNodeVectorChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            var action = e.Action;

            switch (action)
            {
                //Reset case, commonly seen when a TreeNode is cleared.
                //removes all nodes that need removing then 
                //toggles a collapse / expand to ensure order.
                case System.Collections.Specialized.NotifyCollectionChangedAction.Reset:
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
                                if ((flatVectorRealizedItems.ElementAt(i)).ParentNode == null)
                                {
                                    RemoveNodeAndDescendantsFromView(flatVectorRealizedItems.ElementAt(i));
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
                case System.Collections.Specialized.NotifyCollectionChangedAction.Add:
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

                            AddNodeToView(targetNode, (parentIndex + parentNode.Size + allOpenedDescendantsCount));
                            if (targetNode.IsExpanded)
                            {
                                AddNodeDescendantsToView(targetNode, parentIndex + parentNode.Size, allOpenedDescendantsCount);
                            }
                        }

                        break;
                    }
                //Removes a node from the ViewModel when a TreeNode
                //removes a child.
                case System.Collections.Specialized.NotifyCollectionChangedAction.Remove:
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
                                if ((flatVectorRealizedItems.ElementAt(i)).ParentNode == null)
                                {
                                    RemoveNodeAndDescendantsFromView(flatVectorRealizedItems.ElementAt(i));
                                }
                            }
                        }

                        break;
                    }
                //Triggered by a replace such as SetAt.
                //Updates the TreeNode that changed in the 
                case System.Collections.Specialized.NotifyCollectionChangedAction.Replace:
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
