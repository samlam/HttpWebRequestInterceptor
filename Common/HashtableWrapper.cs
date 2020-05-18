using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HttpWebReqInterceptor
{
    public class HashtableWrapper : Hashtable, IEnumerable
    {
        private readonly Hashtable table;

        internal HashtableWrapper(Hashtable table)
            : base()
        {
            this.table = table;
        }

        public override int Count => this.table.Count;

        public override bool IsReadOnly => this.table.IsReadOnly;

        public override bool IsFixedSize => this.table.IsFixedSize;

        public override bool IsSynchronized => this.table.IsSynchronized;

        public override object SyncRoot => this.table.SyncRoot;

        public override ICollection Keys => this.table.Keys;

        public override ICollection Values => this.table.Values;

        public override object this[object key]
        {
            get => this.table[key];
            set => this.table[key] = value;
        }

        public override void Add(object key, object value)
        {
            this.table.Add(key, value);
        }

        public override void Clear()
        {
            this.table.Clear();
        }

        public override bool Contains(object key)
        {
            return this.table.Contains(key);
        }

        public override bool ContainsKey(object key)
        {
            return this.table.ContainsKey(key);
        }

        public override bool ContainsValue(object key)
        {
            return this.table.ContainsValue(key);
        }

        public override void CopyTo(Array array, int arrayIndex)
        {
            this.table.CopyTo(array, arrayIndex);
        }

        public override object Clone()
        {
            return new HashtableWrapper((Hashtable)this.table.Clone());
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return this.table.GetEnumerator();
        }

        public override IDictionaryEnumerator GetEnumerator()
        {
            return this.table.GetEnumerator();
        }

        public override void Remove(object key)
        {
            this.table.Remove(key);
        }
    }
}
