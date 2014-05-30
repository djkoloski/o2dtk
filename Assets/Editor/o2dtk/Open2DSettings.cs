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
			tilesets_root_obj = AssetDatabase.LoadAssetAtPath(Open2D.settings["tilesets_root"], typeof(Object));
			tilemaps_root_obj = AssetDatabase.LoadAssetAtPath(Open2D.settings["tilemaps_root"], typeof(Object));
		}

		// A temporary for checking when the chosen tile set directory is changed
		private static Object tilesets_root_obj = null;
		// A temporary for checking when the chosen TMX directory is changed
		private static Object tilemaps_root_obj = null;

		public void OnGUI()
		{
			tilesets_root_obj = Utility.GUI.LabeledDirectoryField("Tile sets directory:", tilesets_root_obj);
			tilemaps_root_obj = Utility.GUI.LabeledDirectoryField("Tile maps directory:", tilemaps_root_obj);

			Open2D.UpdateSettingsEntry("tilesets_root", AssetDatabase.GetAssetPath(tilesets_root_obj));
			Open2D.UpdateSettingsEntry("tilemaps_root", AssetDatabase.GetAssetPath(tilemaps_root_obj));
		}
	}
}
