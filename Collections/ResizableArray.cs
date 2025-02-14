using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;


namespace RC.DBA.Collections
{
    public class ResizableArray<T> : IList<T>, ICollection<T>
    {
        T[] _array;
        int _count;

        public ResizableArray()
        {
            _array = Array.Empty<T>();
        }
        public ResizableArray(int initialCapacity)
        {
            _array = new T[initialCapacity];
        }

        public ResizableArray(T[] array)
        {
            _array = array;
            _count = array.Length;

        }

        public T this[int index] { get => _array[index]; set => _array[index] = value; }

        public T[] InternalArray { get { return _array; } }

        public int Count => _count;

        public bool IsReadOnly => false;

        public T[] ToArray()
        {
            T[] result = new T[_count];
            Array.Copy(_array, result, _count);

            return result;
        }

        public void Add(T element)
        {
            int length = _array.Length;
            if (_count == length)
            {
                length = length == 0 ? 4 : length * 2;
                Array.Resize(ref _array, length);
            }

            _array[_count++] = element;
        }

        public void AddByRef(ref T element)
        {
            int length = _array.Length;
            if (_count == length)
            {
                length = length == 0 ? 4 : length * 2;
                Array.Resize(ref _array, length);
            }

            _array[_count++] = element;
        }

        public void Clear()
        {
            _count = 0; _array = Array.Empty<T>();
        }

        public bool Contains(T item)
        {
            var comparer = EqualityComparer<T>.Default;
            for (int i = 0; i < _count; i++)
            {
                if (comparer.Equals(item, _array[i])) return true;
            }

            return false;
        }

        public void CopyTo(T[] array, int arrayIndex)
        {
            Array.Copy(_array, 0, array, arrayIndex, _count);
        }

        public int IndexOf(T item)
        {
            var comparer = EqualityComparer<T>.Default;
            for (int i = 0; i < _count; i++)
            {
                if (comparer.Equals(item, _array[i])) return i;
            }
            return -1;
        }

        public void Insert(int index, T item)
        {
            int length = _array.Length;

            if (length == 0) length = 4;

            
            if (index < _count)
            {
                if((_count+1) > length)
                {
                    length = length * 2;
                    Array.Resize(ref _array, length);
                }
                for (int i = index; i < _count; i++)
                {
                    _array[i + 1] = _array[i];
                }
                _count++;
            }
            else
            {
                _count = index + 1;
                while (_count > length)
                    length = length * 2;

                if (length > _array.Length)
                    Array.Resize(ref _array, length);
            }


            _array[index] = item;
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
                _array[i] = _array[i + 1];
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
            private T[] _array;
            private int _index;
            private int _count;
            private T _current;

            internal ValueEnumerator(T[] array, int count)
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
                    _current = _array[_index++];
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
    }
}
