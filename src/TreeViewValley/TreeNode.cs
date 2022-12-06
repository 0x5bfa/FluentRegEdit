using System.ComponentModel;
using System.Linq;
using System.Collections.ObjectModel;
using Windows.Foundation.Metadata;

namespace TreeViewValley
{
    [Windows.UI.Xaml.Data.Bindable]
    [WebHostHidden]
    public sealed class TreeNode : INotifyPropertyChanged
    {
        public TreeNode()
        {
            childrenVector.CollectionChanged += ChildrenVectorChanged;
        }

        #region Properties
        public event System.Collections.Specialized.NotifyCollectionChangedEventHandler VectorChanged;

        public event PropertyChangedEventHandler PropertyChanged;

        public event System.Collections.Specialized.NotifyCollectionChangedEventHandler TreeNodeChanged;

        private ObservableCollection<TreeNode> childrenVector = new();

        public int Size
        {
            get => childrenVector.Count;
        }

        private object data = null;
        public object Data
        {
            get => data;
            set
            {
                data = value;
                this.PropertyChanged(this, new PropertyChangedEventArgs("Data"));
            }
        }

        private TreeNode parentNode = null;
        public TreeNode ParentNode
        {
            get => parentNode;
            set
            {
                parentNode = value;
                this.PropertyChanged(this, new PropertyChangedEventArgs("ParentNode"));
                this.PropertyChanged(this, new PropertyChangedEventArgs("Depth"));
            }
        }

        private bool isExpanded = false;
        public bool IsExpanded
        {
            get => isExpanded;
            set
            {
                isExpanded = value;
                this.PropertyChanged(this, new PropertyChangedEventArgs("IsExpanded"));
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

        public void Append(object value)
        {
            int count = childrenVector.Count;
            TreeNode targetNode = (TreeNode)value;
            targetNode.ParentNode = this;
            childrenVector.Append(targetNode);

            //If the count was 0 before we appended, then the HasItems property needs to change.
            if (count == 0)
            {
                this.PropertyChanged(this, new PropertyChangedEventArgs(nameof(HasItems)));
            }

            this.PropertyChanged(this, new PropertyChangedEventArgs(nameof(Size)));
        }

        public void Clear()
        {
            int count = childrenVector.Count;
            TreeNode childNode;
            for (int i = 0; i < Size; i++)
            {
                childNode = (TreeNode)GetAt(i);
                childNode.ParentNode = null;
            }

            childrenVector.Clear();

            //If the count was not 0 before we cleared, then the HasItems property needs to change.
            if (count == 0)
            {
                this.PropertyChanged(this, new PropertyChangedEventArgs(nameof(HasItems)));
            }

            this.PropertyChanged(this, new PropertyChangedEventArgs(nameof(Size)));
        }

        public TreeNode First()
        {
            return childrenVector.First();
        }

        public object GetAt(int index)
        {
            if (index > -1 && index < childrenVector.Count)
            {
                return childrenVector.ElementAt(index);
            }

            return null;
        }

        //public IBindableVectorView GetView()
        //{
        //    return (IBindableVectorView)childrenVector.GetView();
        //}

        public int IndexOf(object value)
        {
            return childrenVector.IndexOf((TreeNode)value);
        }

        public void InsertAt(int index, object value)
        {
            if (index > -1 && index <= childrenVector.Count)
            {
                int count = childrenVector.Count;
                TreeNode targetNode = (TreeNode)value;
                targetNode.ParentNode = this;

                childrenVector.Insert(index, (TreeNode)value);

                //If the count was 0 before we insert, then the HasItems property needs to change.
                if (count == 0)
                {
                    this.PropertyChanged(this, new PropertyChangedEventArgs(nameof(HasItems)));
                }

                this.PropertyChanged(this, new PropertyChangedEventArgs(nameof(Size)));
            }
        }

        public void RemoveAt(int index)
        {
            if (index > -1 && index < childrenVector.Count)
            {
                int count = childrenVector.Count;
                TreeNode targetNode = childrenVector.ElementAt(index);
                targetNode.ParentNode = null;
                childrenVector.RemoveAt(index);

                //If the count was 1 before we remove, then the HasItems property needs to change.
                if (count == 1)
                {
                    this.PropertyChanged(this, new PropertyChangedEventArgs(nameof(HasItems)));
                }

                this.PropertyChanged(this, new PropertyChangedEventArgs(nameof(Size)));
            }
        }

        public void RemoveAtEnd()
        {
            int count = childrenVector.Count;
            TreeNode targetNode = childrenVector.ElementAt(childrenVector.Count - 1);
            targetNode.ParentNode = null;
            childrenVector.RemoveAt(childrenVector.Count - 1);

            //If the count was 1 before we remove, then the HasItems property needs to change.
            if (count == 1)
            {
                this.PropertyChanged(this, new PropertyChangedEventArgs(nameof(HasItems)));
            }

            this.PropertyChanged(this, new PropertyChangedEventArgs(nameof(Size)));
        }

        public void SetAt(int index, object value)
        {
            if (index > -1 && index <= childrenVector.Count)
            {
                childrenVector.ElementAt(index).ParentNode = null;
                TreeNode targetNode = (TreeNode)value;
                targetNode.ParentNode = this;
                childrenVector[index] = targetNode;
            }
        }

        public void ChildrenVectorChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            VectorChanged(this, e);
        }
    }
}
