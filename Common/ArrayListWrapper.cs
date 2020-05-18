using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HttpWebReqInterceptor
{
    public class ArrayListWrapper : ArrayList
    {
        private ArrayList list;

        internal ArrayListWrapper(ArrayList list)
            : base()
        {
            this.list = list;
        }

        public override int Capacity
        {
            get => this.list.Capacity;
            set => this.list.Capacity = value;
        }

        public override int Count => this.list.Count;

        public override bool IsReadOnly => this.list.IsReadOnly;

        public override bool IsFixedSize => this.list.IsFixedSize;

        public override bool IsSynchronized => this.list.IsSynchronized;

        public override object SyncRoot => this.list.SyncRoot;

        public override object this[int index]
        {
            get => this.list[index];
            set => this.list[index] = value;
        }

        public override int Add(object value)
        {
            return this.list.Add(value);
        }

        public override void AddRange(ICollection c)
        {
            this.list.AddRange(c);
        }

        public override int BinarySearch(object value)
        {
            return this.list.BinarySearch(value);
        }

        public override int BinarySearch(object value, IComparer comparer)
        {
            return this.list.BinarySearch(value, comparer);
        }

        public override int BinarySearch(int index, int count, object value, IComparer comparer)
        {
            return this.list.BinarySearch(index, count, value, comparer);
        }

        public override void Clear()
        {
            this.list.Clear();
        }

        public override object Clone()
        {
            return new ArrayListWrapper((ArrayList)this.list.Clone());
        }

        public override bool Contains(object item)
        {
            return this.list.Contains(item);
        }

        public override void CopyTo(Array array)
        {
            this.list.CopyTo(array);
        }

        public override void CopyTo(Array array, int index)
        {
            this.list.CopyTo(array, index);
        }

        public override void CopyTo(int index, Array array, int arrayIndex, int count)
        {
            this.list.CopyTo(index, array, arrayIndex, count);
        }

        public override IEnumerator GetEnumerator()
        {
            return this.list.GetEnumerator();
        }

        public override IEnumerator GetEnumerator(int index, int count)
        {
            return this.list.GetEnumerator(index, count);
        }

        public override int IndexOf(object value)
        {
            return this.list.IndexOf(value);
        }

        public override int IndexOf(object value, int startIndex)
        {
            return this.list.IndexOf(value, startIndex);
        }

        public override int IndexOf(object value, int startIndex, int count)
        {
            return this.list.IndexOf(value, startIndex, count);
        }

        public override void Insert(int index, object value)
        {
            this.list.Insert(index, value);
        }

        public override void InsertRange(int index, ICollection c)
        {
            this.list.InsertRange(index, c);
        }

        public override int LastIndexOf(object value)
        {
            return this.list.LastIndexOf(value);
        }

        public override int LastIndexOf(object value, int startIndex)
        {
            return this.list.LastIndexOf(value, startIndex);
        }

        public override int LastIndexOf(object value, int startIndex, int count)
        {
            return this.list.LastIndexOf(value, startIndex, count);
        }

        public override void Remove(object value)
        {
            this.list.Remove(value);
        }

        public override void RemoveAt(int index)
        {
            this.list.RemoveAt(index);
        }

        public override void RemoveRange(int index, int count)
        {
            this.list.RemoveRange(index, count);
        }

        public override void Reverse(int index, int count)
        {
            this.list.Reverse(index, count);
        }

        public override void SetRange(int index, ICollection c)
        {
            this.list.SetRange(index, c);
        }

        public override ArrayList GetRange(int index, int count)
        {
            return this.list.GetRange(index, count);
        }

        public override void Sort()
        {
            this.list.Sort();
        }

        public override void Sort(IComparer comparer)
        {
            this.list.Sort(comparer);
        }

        public override void Sort(int index, int count, IComparer comparer)
        {
            this.list.Sort(index, count, comparer);
        }

        public override object[] ToArray()
        {
            return this.list.ToArray();
        }

        public override Array ToArray(Type type)
        {
            return this.list.ToArray(type);
        }

        public override void TrimToSize()
        {
            this.list.TrimToSize();
        }

        public ArrayList Swap()
        {
            ArrayList old = this.list;
            this.list = new ArrayList(old.Capacity);
            return old;
        }
    }

}
