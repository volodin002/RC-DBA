using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RC.DBA.Collections
{
    public class HashSetList<T> : IList<T>, ICollection<T>
    {
        // store lower 31 bits of hash code
        protected const int Lower31BitMask = 0x7FFFFFFF;

        //static Slot[] _empty = new Slot[0];

        internal struct Slot
        {
            public int next; // Index of next entry, -1 if last   
            public T value;
        }

        protected int[] _hash_array;
        private Slot[] _array;
        protected int _count;

        public HashSetList()
        {
            _array = Array.Empty<Slot>();
        }
        public HashSetList(int initialCapacity)
        {
            _hash_array = new int[initialCapacity];
            _array = new Slot[initialCapacity];
        }

        public bool TryAdd(T item)
        {
            var comparer = EqualityComparer<T>.Default;
            int hashCode = comparer.GetHashCode(item) & Lower31BitMask;
            int length = _array.Length;

            int hashIndex;
            if (length > 0)
            {
                hashIndex = hashCode % length;

                for (int i = _hash_array[hashIndex] - 1; i >= 0; i = _array[i].next)
                {
                    if (comparer.Equals(_array[i].value, item))
                    {
                        return false; // already exists !
                    }
                }
            }
            else
                hashIndex = 0;

            if (_count == length) // resize buffers
            {
                length = length == 0 ? 4 : length * 2;
                Array.Resize(ref _array, length);

                _hash_array = new int[length];

                for (int i = 0; i < _count; i++)
                {
                    int idHashCode = comparer.GetHashCode(_array[i].value) & Lower31BitMask;

                    int idHashIndex = idHashCode % length;
                    _array[i].next = _hash_array[idHashIndex] - 1;
                    _hash_array[idHashIndex] = i + 1;
                }
                // recompute hashIndex with new array Length
                hashIndex = hashCode % length;
            }

            
            _array[_count++] = new Slot() { value = item, next = _hash_array[hashIndex] - 1 };
            _hash_array[hashIndex] = _count;

            return true;
        }


        #region IList
        public T this[int index] { get => _array[index].value; set => _array[index].value = value; }

        public int Count => _count;

        public bool IsReadOnly => false;

        public void Add(T item)
        {
            TryAdd(item);
        }

        public void Clear()
        {
            _count = 0; _array = Array.Empty<Slot>(); _hash_array = null;
        }

        public bool Contains(T item)
        {
            return IndexOf(item) >= 0;
        }

        public void CopyTo(T[] array, int arrayIndex)
        {
            for (int i = 0; i < _count; i++)
            {
                int index = arrayIndex + i;
                if (index == array.Length) break;
                array[index] = _array[i].value;
            }
        }

        public int IndexOf(T item)
        {
            var comparer = EqualityComparer<T>.Default;
            int hashCode = comparer.GetHashCode(item) & Lower31BitMask;
            int length = _array.Length;
            if (length == 0) return -1;

            int hashIndex = hashCode % length;

            for (int i = _hash_array[hashIndex] - 1; i >= 0; i = _array[i].next)
            {
                if (comparer.Equals(_array[i].value, item))
                {
                    return i;
                }
            }

            return -1;
        }

        public void Insert(int index, T item)
        {
            int length = _array.Length;
            bool resize = false;
            if (++_count == length)
            {
                length = length == 0 ? 4 : length * 2;
                Array.Resize(ref _array, length);
                resize = true;
            }
            for (int i = index; i < _count; i++)
            {
                _array[i + 1].value = _array[i].value;
            }
            _array[index].value = item;


            if (resize) // recomute hash
            {
                _hash_array = new int[length];
                var comparer = EqualityComparer<T>.Default;
                for (int i = 0; i < length; i++)
                {
                    int idHashCode = comparer.GetHashCode(_array[i].value) & Lower31BitMask;

                    int idHashIndex = idHashCode % length;
                    _array[i].next = _hash_array[idHashIndex] - 1;
                    _hash_array[idHashIndex] = i + 1;
                }
            }
        }

        public bool Remove(T item)
        {
            int index = IndexOf(item);
            if (index < 0) return false;

            RemoveAt(index);

            return true;
        }

        public void RemoveAt(int index)
        {
            _count--;
            for (int i = index; i < _count; i++)
            {
                _array[i].value = _array[i + 1].value;
            }

            // recomute hash
            var comparer = EqualityComparer<T>.Default;
            int length = _array.Length;    

            for (int i = 0; i < _count; i++)
            {
                int idHashCode = comparer.GetHashCode(_array[i].value) & Lower31BitMask;

                int idHashIndex = idHashCode % length;
                _array[i].next = _hash_array[idHashIndex] - 1;
                _hash_array[idHashIndex] = i + 1;
            }
            
        }

        #region // IEnumerable<T>

        /// we avoid boxing!!! compiler can use this method in foreach !!!
        public ValueEnumerator GetEnumerator() => new ValueEnumerator(_array, _count);

        /// <internalonly/> we avoid boxing and hide interface implementation!!!
        IEnumerator<T> IEnumerable<T>.GetEnumerator() => new ValueEnumerator(_array, _count);

        /// <internalonly/> we avoid boxing and hide interface implementation!!!
        IEnumerator IEnumerable.GetEnumerator() => throw new NotImplementedException();

        #endregion // IEnumerable<T>

        public struct ValueEnumerator : IEnumerator<T>
        {
            private Slot[] _array;
            private int _index;
            private int _count;
            private T _current;

            internal ValueEnumerator(Slot[] array, int count)
            {
                _array = array;
                _index = 0;
                _count = count;
                _current = default(T);
            }

            public void Dispose()
            {
                _array = null;
                _current = default(T);
            }

            public bool MoveNext()
            {
                if (_index < _count)
                {
                    _current = _array[_index++].value;
                    return true;
                }

                _current = default(T);
                return false;
            }

            public void Reset()
            {
                _index = 0;
                _current = default(T);
            }

            public T Current => _current;

            object IEnumerator.Current => throw new NotImplementedException();
        }

        #endregion // IList
    }
}
