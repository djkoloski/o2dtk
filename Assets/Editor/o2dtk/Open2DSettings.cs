using UnityEngine;
using UnityEditor;
using System.Collections;
using System.IO;

namespace o2dtk
{
	public class Open2DSettingsEditor : EditorWindow
	{
		// The current editor window
		private static EditorWindow window = null;

		[MenuItem("Open 2D/Settings")]
		public static void OpenSettings()
		{
			window = EditorWindow.GetWindow(typeof(Open2DSettingsEditor), false, "Open 2D Settings", true);
		}

		public void Awake()
		{
			if (!Directory.Exists(Open2D.settings.tilesets_root))
			{
				Directory.CreateDirectory(Open2D.settings.tilesets_root);
				AssetDatabase.ImportAsset(Open2D.settings.tilesets_root);
			}

			tilesets_root_obj = AssetDatabase.LoadAssetAtPath(Open2D.settings.tilesets_root, typeof(Object));
		}

		// A temporary for checking when the chosen tile set directory is changed
		private static Object tilesets_root_obj = null;

		public void OnGUI()
		{
			GUILayout.Space(5);

			GUILayout.BeginHorizontal();
			GUILayout.Label("Tile sets directory:");
			GUILayout.FlexibleSpace();

			Object new_root_obj = EditorGUILayout.ObjectField(tilesets_root_obj, typeof(Object), true);
			if (new_root_obj != tilesets_root_obj)
			{
				string path = AssetDatabase.GetAssetPath(new_root_obj);
				FileAttributes attr = File.GetAttributes(path);

				if ((attr & FileAttributes.Directory) == FileAttributes.Directory)
				{
					tilesets_root_obj = new_root_obj;
					Open2D.settings.tilesets_root = path;
					Open2D.SaveSettings();
				}
				else
					EditorUtility.DisplayDialog("Invalid tile sets directory", "The given object is not a directory.", "OK");
			}

			GUILayout.EndHorizontal();
		}
	}
}
