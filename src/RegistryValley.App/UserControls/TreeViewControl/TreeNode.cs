using System.ComponentModel;
using System.Linq;
using System.Collections.ObjectModel;
using Windows.Foundation.Metadata;
using System;
using Windows.Foundation.Collections;
//using Windows.UI.Xaml.Interop;
using System.Collections.Specialized;

namespace RegistryValley.App.UserControls.TreeViewControl
{
    public class TreeNode : INotifyPropertyChanged
    {
        public TreeNode()
        {
            childrenCollection.CollectionChanged += CollectionChanged;
        }

        #region Properties
        public event NotifyCollectionChangedEventHandler ChildrenCollectionChanged;
        public event NotifyCollectionChangedEventHandler TreeNodeChanged;
        public event PropertyChangedEventHandler PropertyChanged;

        private ObservableCollection<TreeNode> childrenCollection = new();

        public int Size { get => childrenCollection.Count; }

        private object data = null;
        public object Data
        {
            get => data;
            set
            {
                data = value;
                PropertyChanged(this, new PropertyChangedEventArgs(nameof(Data)));
            }
        }

        private TreeNode parentNode = null;
        public TreeNode ParentNode
        {
            get => parentNode;
            set
            {
                parentNode = value;
                PropertyChanged(this, new PropertyChangedEventArgs(nameof(ParentNode)));
                PropertyChanged(this, new PropertyChangedEventArgs(nameof(Depth)));
            }
        }

        private bool isExpanded = false;
        public bool IsExpanded
        {
            get => isExpanded;
            set
            {
                isExpanded = value;
                PropertyChanged(this, new PropertyChangedEventArgs("IsExpanded"));
            }
        }

        public bool HasItems
        {
            get => (Size != 0);
        }

        public int Depth
        {
            get
            {
                TreeNode ancestorNode = this;
                int depth = -1;

                while ((ancestorNode.ParentNode) != null)
                {
                    depth++;
                    ancestorNode = ancestorNode.ParentNode;
                }

                return depth;
            }
        }
        #endregion

        public void Add(object value)
        {
            int count = childrenCollection.Count;
            TreeNode targetNode = (TreeNode)value;
            targetNode.ParentNode = this;
            childrenCollection.Add(targetNode);

            //If the count was 0 before we appended, then the HasItems property needs to change.
            if (count == 0)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(nameof(HasItems)));
            }

            PropertyChanged(this, new PropertyChangedEventArgs(nameof(Size)));

        }

        public IEnumerable<TreeNode> Append(object value)
        {
            int count = childrenCollection.Count;
            TreeNode targetNode = (TreeNode)value;
            targetNode.ParentNode = this;
            var appenddedCollection = childrenCollection.Append(targetNode);

            //If the count was 0 before we appended, then the HasItems property needs to change.
            if (count == 0)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(nameof(HasItems)));
            }

            PropertyChanged(this, new PropertyChangedEventArgs(nameof(Size)));

            return appenddedCollection;
        }

        public void Clear()
        {
            int count = childrenCollection.Count;
            TreeNode childNode;

            for (int index = 0; index < Size; index++)
            {
                childNode = (TreeNode)GetAt(index);
                childNode.ParentNode = null;
            }

            childrenCollection.Clear();

            //If the count was not 0 before we cleared, then the HasItems property needs to change.
            if (count == 0)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(nameof(HasItems)));
            }

            PropertyChanged(this, new PropertyChangedEventArgs(nameof(Size)));
        }

        public TreeNode First()
        {
            return childrenCollection.First();
        }

        public object GetAt(int index)
        {
            if (index > -1 && index < childrenCollection.Count)
            {
                return childrenCollection.ElementAt(index);
            }

            return null;
        }

        public int IndexOf(object value)
        {
            return childrenCollection.IndexOf((TreeNode)value);
        }

        public void InsertAt(int index, object value)
        {
            if (index > -1 && index <= childrenCollection.Count)
            {
                int count = childrenCollection.Count;
                TreeNode targetNode = (TreeNode)value;
                targetNode.ParentNode = this;

                childrenCollection.Insert(index, (TreeNode)value);

                //If the count was 0 before we insert, then the HasItems property needs to change.
                if (count == 0)
                {
                    PropertyChanged(this, new PropertyChangedEventArgs(nameof(HasItems)));
                }

                PropertyChanged(this, new PropertyChangedEventArgs(nameof(Size)));
            }
        }

        public void RemoveAt(int index)
        {
            if (index > -1 && index < childrenCollection.Count)
            {
                int count = childrenCollection.Count;
                TreeNode targetNode = childrenCollection.ElementAt(index);
                targetNode.ParentNode = null;
                childrenCollection.RemoveAt(index);

                //If the count was 1 before we remove, then the HasItems property needs to change.
                if (count == 1)
                {
                    PropertyChanged(this, new PropertyChangedEventArgs(nameof(HasItems)));
                }

                PropertyChanged(this, new PropertyChangedEventArgs(nameof(Size)));
            }
        }

        public void RemoveAtEnd()
        {
            int count = childrenCollection.Count;
            TreeNode targetNode = childrenCollection.ElementAt(childrenCollection.Count - 1);
            targetNode.ParentNode = null;
            childrenCollection.RemoveAt(childrenCollection.Count - 1);

            //If the count was 1 before we remove, then the HasItems property needs to change.
            if (count == 1)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(nameof(HasItems)));
            }

            PropertyChanged(this, new PropertyChangedEventArgs(nameof(Size)));
        }

        public void SetAt(int index, object value)
        {
            if (index > -1 && index <= childrenCollection.Count)
            {
                childrenCollection.ElementAt(index).ParentNode = null;
                TreeNode targetNode = (TreeNode)value;
                targetNode.ParentNode = this;
                childrenCollection[index] = targetNode;
            }
        }

        public void CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            ChildrenCollectionChanged(this, e);
        }
    }
}
