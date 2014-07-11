using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace o2dtk
{
	namespace Collections
	{
		[System.Serializable]
		public class Pair<T1, T2> : System.IComparable, System.IComparable<Pair<T1, T2>> where T1 : System.IComparable where T2 : System.IComparable
		{
			public readonly T1 first;
			public readonly T2 second;

			public Pair(T1 f, T2 s)
			{
				first = f;
				second = s;
			}

			public int CompareTo(object other)
			{
				if (other == null)
					return 1;

				Pair<T1, T2> otherPair = other as Pair<T1, T2>;
				if (otherPair == null)
					throw new System.ArgumentException("Object is not a Pair of the correct type");
				else
					return CompareTo(otherPair);
			}

			public int CompareTo(Pair<T1, T2> other)
			{
				if (first.CompareTo(other.first) > 0)
					return 1;
				else if (first.CompareTo(other.first) < 0)
					return -1;
				else
					if (second.CompareTo(other.second) > 0)
						return 1;
					else if (second.CompareTo(other.second) < 0)
						return -1;
					else
						return 0;
			}

			public override bool Equals(System.Object obj)
			{
				if (obj == null)
					return false;

				Pair<T1, T2> other = obj as Pair<T1, T2>;
				if ((System.Object)other == null)
					return false;

				return (first.Equals(other.first) && second.Equals(other.second));
			}

			public bool Equals(Pair<T1, T2> other)
			{
				if ((object)other == null)
					return false;

				return (first.Equals(other.first) && second.Equals(other.second));
			}

			public override int GetHashCode()
			{
				int n1 = first.GetHashCode();
				n1 = (n1 >= 0 ? n1 * 2 : n1 * 2 - 1);
				int n2 = second.GetHashCode();
				n2 = (n2 >= 0 ? n2 * 2 : n2 * 2 - 1);
				return (n1 + n2) * (n1 + n2 + 1) / 2 + n2;
			}

			public static bool operator==(Pair<T1, T2> a, Pair<T1, T2> b)
			{
				if (object.ReferenceEquals(a, null) || object.ReferenceEquals(b, null))
					return object.ReferenceEquals(a, b);
				return (a.first.Equals(b.first) && a.second.Equals(b.second));
			}

			public static bool operator!=(Pair<T1, T2> a, Pair<T1, T2> b)
			{
				if (object.ReferenceEquals(a, null) || object.ReferenceEquals(b, null))
					return !object.ReferenceEquals(a, b);
				return (!a.first.Equals(b.first) || !a.second.Equals(b.second));
			}
		}

		[System.Serializable]
		public class Triple<T1, T2, T3> : Pair<T1, Pair<T2, T3>> where T1 : System.IComparable where T2 : System.IComparable where T3 : System.IComparable
		{
			public new T2 second
			{
				get
				{
					return base.second.first;
				}
			}
			public T3 third
			{
				get
				{
					return base.second.second;
				}
			}

			public Triple(T1 a, T2 b, T3 c) : base(a, new Pair<T2, T3>(b, c))
			{ }
		}
	}
}
