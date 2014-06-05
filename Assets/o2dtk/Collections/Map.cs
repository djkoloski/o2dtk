using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace o2dtk
{
	namespace Collections
	{
		[System.Serializable]
		public class Map<TKey, TValue> : IDictionary<TKey, TValue>,
			ICollection<KeyValuePair<TKey, TValue>>, IDictionary, ICollection,
			IEnumerable<KeyValuePair<TKey, TValue>>, IEnumerable
		{
			public class Enumerator : IEnumerator, IEnumerator<KeyValuePair<TKey, TValue>>,
				IDictionaryEnumerator
			{
				// The map the enumerator is enumerating
				private Map<TKey, TValue> map_;
				// The current index in the map
				private int index_ = -1;

				// Gets both the key and value of the current dictionary entry
				public DictionaryEntry Entry
				{
					get
					{
						try
						{
							return new DictionaryEntry(map_.Keys[index_], map_.Values[index_]);
						}
						catch (System.IndexOutOfRangeException)
						{
							throw new System.InvalidOperationException();
						}
					}
				}
				// Gets the key of the current dictionary entry
				object IDictionaryEnumerator.Key
				{
					get
					{
						try
						{
							return map_.Keys[index_];
						}
						catch (System.IndexOutOfRangeException)
						{
							throw new System.InvalidOperationException();
						}
					}
				}
				// Gets the value of the current dictionary entry
				object IDictionaryEnumerator.Value
				{
					get
					{
						try
						{
							return map_.Values[index_];
						}
						catch (System.IndexOutOfRangeException)
						{
							throw new System.InvalidOperationException();
						}
					}
				}

				// Basic constructor
				public Enumerator(Map<TKey, TValue> map)
				{
					map_ = map;
				}

				// Moves to the next element
				public bool MoveNext()
				{
					++index_;
					return (index_ < map_.Count);
				}

				// Resets the enumerator to before the first element
				public void Reset()
				{
					index_ = -1;
				}

				// Gets the current object
				object IEnumerator.Current
				{
					get
					{
						return Current;
					}
				}

				// Gets the current pair
				public KeyValuePair<TKey, TValue> Current
				{
					get
					{
						try
						{
							return new KeyValuePair<TKey, TValue>(map_.Keys[index_], map_.Values[index_]);
						}
						catch (System.IndexOutOfRangeException)
						{
							throw new System.InvalidOperationException();
						}
					}
				}

				// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources
				public void Dispose()
				{
					map_ = null;
					index_ = -1;
				}
			}

			// The sorted list of keys in the map
			[SerializeField]
			private List<TKey> keys_;
			// The values in key-sorted order in the map
			[SerializeField]
			private List<TValue> values_;
			// Gets the number of key/value pairs contained in the Map<TKey, TValue>
			public int Count
			{
				get
				{
					return keys_.Count;
				}
			}
			// Gets a value indicating whether the Map<TKey, TValue> is read-only
			public bool IsReadOnly
			{
				get
				{
					return false;
				}
			}
			// Gets a value indicating whether the Map<TKey, TValue> is fixed-size
			public bool IsFixedSize
			{
				get
				{
					return false;
				}
			}
			// Gets a value indicating whether the access to the Map<TKey, TValue> is synchronized
			public bool IsSynchronized
			{
				get
				{
					return true;
				}
			}
			// Gets an object that can be used to synchronize acces to the Map<TKey, TValue>
			public object SyncRoot
			{
				get
				{
					return this;
				}
			}
			// Gets or sets the value associated with the specified key to an object
			public object this[object key]
			{
				get
				{
					return this[key];
				}
				set
				{
					this[key] = value;
				}
			}
			// Gets or sets the value associated with the specified key
			public TValue this[TKey key]
			{
				get
				{
					int index = Search(key);
					if (index < 0)
						throw new KeyNotFoundException();
					return values_[index];
				}
				set
				{
					int index = Search(key);
					if (index < 0)
						InsertAt(~index, key, value);
					else
						values_[index] = value;
				}
			}
			// Gets a list containing the keys in the Map<TKey, TValue>
			public List<TKey> Keys
			{
				get
				{
					return keys_;
				}
			}
			ICollection<TKey> IDictionary<TKey, TValue>.Keys
			{
				get
				{
					return keys_;
				}
			}
			ICollection IDictionary.Keys
			{
				get
				{
					return keys_;
				}
			}
			// Gets a list containing the values in the Map<TKey, TValue>
			public List<TValue> Values
			{
				get
				{
					return values_;
				}
			}
			ICollection<TValue> IDictionary<TKey, TValue>.Values
			{
				get
				{
					return values_;
				}
			}
			ICollection IDictionary.Values
			{
				get
				{
					return values_;
				}
			}

			// Initializes a new instance of the Map<TKey, TValue> class that is empty.
			public Map()
			{
				keys_ = new List<TKey>();
				values_ = new List<TValue>();
			}

			// Initializes a new instance of the Map<TKey, TValue> class that contains elements
			//   copied from the specified Map<TKey, TValue>
			public Map(Map<TKey, TValue> other)
			{
				keys_ = new List<TKey>(other.keys_);
				values_ = new List<TValue>(other.values_);
			}

			// Finds and returns the binary search for the key
			private int Search(TKey key)
			{
				if (key == null)
					throw new System.ArgumentNullException();
				return keys_.BinarySearch(key);
			}

			// Inserts a key and value at the given index
			private void InsertAt(int index, TKey key, TValue value)
			{
				keys_.Insert(index, key);
				values_.Insert(index, value);
			}

			// Removes the key and value at the given index
			private void RemoveAt(int index)
			{
				keys_.RemoveAt(index);
				values_.RemoveAt(index);
			}

			// Adds the specified key and value to the map
			void IDictionary.Add(object key, object value)
			{
				Add((TKey)key, (TValue)value);
			}
			public void Add(TKey key, TValue value)
			{
				int nearest = Search(key);

				if (nearest >= 0)
					throw new System.ArgumentException();

				InsertAt(~nearest, key, value);
			}

			// Adds the specified pair of key and value to the map
			public void Add(KeyValuePair<TKey, TValue> pair)
			{
				Add(pair.Key, pair.Value);
			}

			// Removes all keys and values from the Map<TKey, TValue>
			public void Clear()
			{
				keys_.Clear();
				values_.Clear();
			}

			// Determines whether the Map<TKey, TValue> contains the specified key
			public bool ContainsKey(TKey key)
			{
				return (Search(key) >= 0);
			}

			// Determines whether the Map<TKey, TValue> contains a specific value
			public bool ContainsValue(TValue value)
			{
				return (values_.IndexOf(value) >= 0);
			}

			// Determines whether the Map<TKey, TValue> contains the specified key and value pair
			public bool Contains(object pair)
			{
				return Contains((KeyValuePair<TKey, TValue>)pair);
			}
			public bool Contains(KeyValuePair<TKey, TValue> pair)
			{
				int nearest = Search(pair.Key);
				if (nearest < 0)
					return false;
				return values_[nearest].Equals(pair.Value);
			}

			// Returns an enumerator that iterates through the Map<TKey, TValue>
			public Enumerator GetEnumerator()
			{
				return new Enumerator(this);
			}
			IEnumerator<KeyValuePair<TKey, TValue>> IEnumerable<KeyValuePair<TKey, TValue>>.GetEnumerator()
			{
				return GetEnumerator();
			}
			IEnumerator IEnumerable.GetEnumerator()
			{
				return GetEnumerator();
			}
			IDictionaryEnumerator IDictionary.GetEnumerator()
			{
				return GetEnumerator();
			}

			// Removes the value with the specified key from the Map<TKey, TValue>
			public bool Remove(TKey key)
			{
				int nearest = Search(key);

				if (nearest >= 0)
				{
					RemoveAt(nearest);
					return true;
				}
				return false;
			}
			void IDictionary.Remove(object key)
			{
				Remove((TKey)key);
			}

			// Removes the specified key and value pair from the Map<TKey, TValue>
			public bool Remove(KeyValuePair<TKey, TValue> pair)
			{
				int nearest = Search(pair.Key);

				if (nearest >= 0)
				{
					if (values_[nearest].Equals(pair.Value))
					{
						RemoveAt(nearest);
						return true;
					}
				}

				return false;
			}

			// Gets the value associated with the specified key
			public bool TryGetValue(TKey key, out TValue value)
			{
				int nearest = Search(key);

				if (nearest >= 0)
				{
					value = values_[nearest];
					return true;
				}
				else
				{
					value = default(TValue);
					return false;
				}
			}

			// Copies the entire Map<TKey, TValue> to a compatible one-dimensional array,
			//   starting at the specified index of the target array
			public void CopyTo(KeyValuePair<TKey, TValue>[] array, int offset)
			{
				for (int i = 0; i < keys_.Count; ++i)
					array[offset + i] = new KeyValuePair<TKey, TValue>(keys_[i], values_[i]);
			}
			void ICollection.CopyTo(System.Array array, int offset)
			{
				for (int i = 0; i < keys_.Count; ++i)
					array.SetValue(new KeyValuePair<TKey, TValue>(keys_[i], values_[i]), offset + i);
			}
		}
	}
}
