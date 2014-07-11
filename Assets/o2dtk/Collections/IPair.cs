using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace o2dtk
{
	namespace Collections
	{
		[System.Serializable]
		public class IPair : Pair<int, int>
		{
			public IPair(int a = 0, int b = 0) : base(a, b)
			{ }
		}

		[System.Serializable]
		public class ITriple : Triple<int, int, int>
		{
			public ITriple(int a = 0, int b = 0, int c = 0) : base(a, b, c)
			{ }
		}
	}
}
