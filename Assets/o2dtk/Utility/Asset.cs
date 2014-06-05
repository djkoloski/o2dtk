using UnityEngine;
using UnityEditor;
using System.Collections;
using System.IO;

namespace o2dtk
{
	namespace Utility
	{
		public class Asset
		{
			// Gets the asset at the path or makes a new one and sets it as dirty
			static public T LoadAndEdit<T>(string path) where T : ScriptableObject
			{
				T result = null;

				if (File.Exists(path))
					result = AssetDatabase.LoadAssetAtPath(path, typeof(T)) as T;
				else
				{
					result = ScriptableObject.CreateInstance<T>();
					AssetDatabase.CreateAsset(result, path);
				}

				EditorUtility.SetDirty(result);

				return result;
			}
		}
	}
}
