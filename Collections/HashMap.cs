using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RC.DBA.Collections
{
    public class HashMap<TKey, TValue> : IEnumerable<TValue>
    {
        // store lower 31 bits of hash code
        protected const int Lower31BitMask = 0x7FFFFFFF;

        //static Slot[] _empty = new Slot[0];

        public struct Slot
        {
            public int next; // Index of next entry, -1 if last   
            public TKey key;
            public TValue value;
        }

        protected int[] _hash_array;
        protected Slot[] _array;
        protected int _count;

        public int Count => _count;

        public IEnumerable<TValue> Values => this; //new ValueEnumerable(_array, _count);

        public HashMap()
        {
            _array = Array.Empty<Slot>();
        }
        public HashMap(int initialCapacity)
        {
            _hash_array = new int[initialCapacity];
            _array = new Slot[initialCapacity];
        }

        private HashMap(int[] hash_array, Slot[] array, int count)
        {
            _hash_array = hash_array;
            _array = array;
            _count = count;
        }

        public int IndexOf(TKey key)
        {
            var comparer = EqualityComparer<TKey>.Default;
            int hashCode = comparer.GetHashCode(key) & Lower31BitMask;
            int length = _array.Length;
            if (length == 0) return -1;

            int hashIndex = hashCode % length;

            for (int i = _hash_array[hashIndex] - 1; i >= 0; i = _array[i].next)
            {
                if (comparer.Equals(_array[i].key, key))
                {
                    return i;
                }
            }

            return -1;
        }

        public TValue GetValueByIndex(int index)
        {
            return _array[index].value;
        }

        public void SetValueByIndex(int index, TValue value)
        {
            _array[index].value = value;
        }

        public ref TValue GetValueRefByIndex(int index)
        {
            return ref _array[index].value;
        }

        public ref readonly TValue GetValueRRefByIndex(int index)
        {
            return ref _array[index].value;
        }

        public bool TryGetValue(TKey key, ref TValue value)
        {
            int index = IndexOf(key);
            if (index < 0) return false;

            value = _array[index].value;

            return true;
        }

        public void Add(TKey key, TValue value)
        {
            var comparer = EqualityComparer<TKey>.Default;
            int hashCode = comparer.GetHashCode(key) & Lower31BitMask;
            int length = _array.Length;

            if (_count == length) // resize buffers
            {
                length = length == 0 ? 4 : length * 2;
                Array.Resize(ref _array, length);

                _hash_array = new int[length];

                for (int i = 0; i < _count; i++)
                {
                    int idHashCode = comparer.GetHashCode(_array[i].key) & Lower31BitMask;

                    int idHashIndex = idHashCode % length;
                    _array[i].next = _hash_array[idHashIndex] - 1;
                    _hash_array[idHashIndex] = i + 1;
                }
            }

            int hashIndex = hashCode % length;
            _array[_count++] = new Slot() { key = key, value = value, next = _hash_array[hashIndex] - 1 };
            _hash_array[hashIndex] = _count;
        }

        public bool TryAdd(TKey key, TValue value)
        {
            var comparer = EqualityComparer<TKey>.Default;
            int hashCode = comparer.GetHashCode(key) & Lower31BitMask;
            int length = _array.Length;

            int hashIndex;
            if (length > 0)
            {
                hashIndex = hashCode % length;

                for (int i = _hash_array[hashIndex] - 1; i >= 0; i = _array[i].next)
                {
                    if (comparer.Equals(_array[i].key, key))
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
                    int idHashCode = comparer.GetHashCode(_array[i].key) & Lower31BitMask;

                    int idHashIndex = idHashCode % length;
                    _array[i].next = _hash_array[idHashIndex] - 1;
                    _hash_array[idHashIndex] = i + 1;
                }
                // recompute hashIndex with new array Length
                hashIndex = hashCode % length;
            }

            _array[_count++] = new Slot() { key = key, value = value, next = _hash_array[hashIndex] - 1 };
            _hash_array[hashIndex] = _count;

            return true;
        }


        public void Set(TKey key, TValue value)
        {
            int index = IndexOf(key);
            if (index < 0)
                Add(key, value);
            else
                SetValueByIndex(index, value);
        }

        public HashMap<TKey, TValue> Copy()
        {
            if (_count == 0)
                return new HashMap<TKey, TValue>(null, Array.Empty<Slot>(), 0);

            int length = _array.Length;
            var hash_array = new int[length];
            Array.Copy(_hash_array, hash_array, length);
            var array = new Slot[length];
            Array.Copy(_array, array, length);

            return new HashMap<TKey, TValue>(hash_array, array, _count);
        }

        public TValue[] ToArray()
        {
            var result = new TValue[_count];
            for (int i = 0; i < _count; i++)
            {
                result[i] = _array[i].value;
            }

            return result;
        }

        public List<TValue> ToList()
        {
            var result = new List<TValue>(_count);
            for (int i = 0; i < _count; i++)
            {
                result.Add(_array[i].value);
            }

            return result;
        }

        #region // IEnumerable<TValue>

        /// we avoid boxing!!! compiler can use this method in foreach !!!
        public ValueEnumerator GetEnumerator() => new ValueEnumerator(_array, _count);

        /// <internalonly/> we avoid boxing and hide interface implementation!!!
        IEnumerator<TValue> IEnumerable<TValue>.GetEnumerator() => new ValueEnumerator(_array, _count);

        /// <internalonly/> we avoid boxing and hide interface implementation!!!
        IEnumerator IEnumerable.GetEnumerator() => throw new NotImplementedException();

        #endregion // IEnumerable<TValue>

        public struct ValueEnumerator : IEnumerator<TValue>
        {
            private Slot[] _array;
            private int _index;
            private int _count;
            private TValue _current;

            public ValueEnumerator(Slot[] array, int count)
            {
                _array = array;
                _index = 0;
                _count = count;
                _current = default(TValue);
            }

            public void Dispose()
            {
                _array = null;
                _current = default(TValue);
            }

            public bool MoveNext()
            {
                if (_index < _count)
                {
                    _current = _array[_index++].value;
                    return true;
                }

                _current = default(TValue);
                return false;
            }

            public void Reset()
            {
                _index = 0;
                _current = default(TValue);
            }

            public TValue Current => _current;

            object IEnumerator.Current => throw new NotImplementedException();
        }

        /*
        protected struct ValueEnumerable : IEnumerable<TValue>
        {
            private Slot[] _array;
            private int _count;
            
            internal ValueEnumerable(Slot[] array, int count)
            {
                _array = array;
                _count = count;
            }

            public IEnumerator<TValue> GetEnumerator() => new ValueEnumerator(_array, _count);

            IEnumerator IEnumerable.GetEnumerator() => throw new NotImplementedException();
        }
        */
    }
}
