// WARNING: this is experimental code that plays with Unity's internals.
// This code was written for Unity 4.5.1f3, and is not guaranteed to work with later versions
// This may not work in any but edit mode

using UnityEditor;
using UnityEngine;
using UnityEditorInternal;
using System.Reflection;

namespace o2dtk
{
	namespace Utility
	{
		public class SortingLayers
		{
			private static System.Type secret = typeof(InternalEditorUtility);
			private static PropertyInfo sortingLayerNames = secret.GetProperty("sortingLayerNames", BindingFlags.Static | BindingFlags.NonPublic);
			private static PropertyInfo sortingLayerUniqueIDs = secret.GetProperty("sortingLayerUniqueIDs", BindingFlags.Static | BindingFlags.NonPublic);
			private static MethodInfo AddSortingLayer = secret.GetMethod("AddSortingLayer", BindingFlags.Static | BindingFlags.NonPublic);
			private static MethodInfo SetSortingLayerName = secret.GetMethod("SetSortingLayerName", BindingFlags.Static | BindingFlags.NonPublic);

			public static string[] GetSortingLayerNames()
			{
				return (string[])sortingLayerNames.GetValue(null, new object[0]);
			}

			public static int[] GetSortingLayerUniqueIDs()
			{
				return (int[])sortingLayerUniqueIDs.GetValue(null, new object[0]);
			}

			public static string StandardSortingLayerName(int index)
			{
				return ("o2dtk_" + index);
			}

			public static int StandardSortingLayerID(int index)
			{
				return SortingLayerID(StandardSortingLayerName(index));
			}

			public static int SortingLayerID(string name)
			{
				string[] names = GetSortingLayerNames();
				int[] ids = GetSortingLayerUniqueIDs();

				for (int i = 0; i < names.Length; ++i)
					if (names[i] == name)
						return ids[i];

				AddSortingLayer.Invoke(null, new object[0]);
				SetSortingLayerName.Invoke(null, new object[2]{ names.Length, name });

				ids = GetSortingLayerUniqueIDs();

				return ids[ids.Length - 1];
			}
		}
	}
}
