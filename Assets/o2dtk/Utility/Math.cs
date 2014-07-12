using UnityEngine;
using UnityEditor;
using System.Collections;

namespace o2dtk
{
	namespace Utility
	{
		public class Math
		{
			public static int FloorDivide(int a, int b)
			{
				if (b < 0)
				{
					a = -a;
					b = -b;
				}

				if (a < 0)
					return (a - b + 1) / b;
				else
					return a / b;
			}
		}
	}
}
