using UnityEngine;
using UnityEditor;
using System.Collections;

namespace o2dtk
{
	public class TMXConverterEditor : EditorWindow
	{
		// The current editor window
		private static EditorWindow window = null;

		// The TMX file to convert from
		private static Object tmx_file = null;
		// The settings to convert the TMX file with
		private static TMXImportSettings settings = new TMXImportSettings();

		[MenuItem("Open 2D/Tile Maps/TMX Converter")]
		public static void OpenTMXConverter()
		{
			window = EditorWindow.GetWindowWithRect(typeof(TMXConverterEditor), new Rect(0, 0, 250, 250), false, "TMX Converter");
		}

		public void OnGUI()
		{
			GUILayout.Space(5);
			
			GUILayout.BeginHorizontal();
			GUILayout.Label("TMX file:");
			GUILayout.FlexibleSpace();
			tmx_file = EditorGUILayout.ObjectField(tmx_file, typeof(Object), true);
			GUILayout.EndHorizontal();

			GUILayout.BeginHorizontal();
			GUILayout.Label("Conversion settings:");
			GUILayout.EndHorizontal();

			GUILayout.BeginHorizontal();
			settings.rebuild_tilesets = GUILayout.Toggle(settings.rebuild_tilesets, "Rebuild tilesets");
			GUILayout.EndHorizontal();

			GUI.enabled = settings.rebuild_tilesets;
			GUILayout.BeginHorizontal();
			settings.force_rebuild_tilesets = GUILayout.Toggle(settings.force_rebuild_tilesets && GUI.enabled, "Force");
			GUILayout.EndHorizontal();
			GUI.enabled = true;

			GUILayout.BeginHorizontal();
			settings.rebuild_chunks = GUILayout.Toggle(settings.rebuild_chunks, "Rebuild chunks");
			GUILayout.EndHorizontal();

			GUILayout.BeginHorizontal();
			GUILayout.Label("Chunk width:");
			settings.chunk_width = (uint)EditorGUILayout.IntField((int)settings.chunk_width);
			GUILayout.EndHorizontal();
			GUILayout.BeginHorizontal();
			GUILayout.Label("Chunk height:");
			settings.chunk_height = (uint)EditorGUILayout.IntField((int)settings.chunk_height);
			GUILayout.EndHorizontal();

			GUILayout.BeginHorizontal();
			if (GUILayout.Button("Convert TMX"))
			{
				bool load = true;

				if (settings.force_rebuild_tilesets)
					if (!EditorUtility.DisplayDialog("Rebuild all tilesets?", "This may take a while. Tilesets will be rebuilt regardless of whether or not they are already sliced. Rebuild all tilesets?", "Rebuild", "Cancel"))
						load = false;

				if (settings.rebuild_chunks && (settings.chunk_width < 0 || settings.chunk_height < 0))
				{
					EditorUtility.DisplayDialog("Invalid chunk size", "The chunk width and height must be greater than 0 or 0 to use the map width and height.", "OK");
					load = false;
				}

				if (load)
					TMXConverter.LoadTMX(AssetDatabase.GetAssetPath(tmx_file), settings);
			}
			GUILayout.EndHorizontal();
		}
	}
}
