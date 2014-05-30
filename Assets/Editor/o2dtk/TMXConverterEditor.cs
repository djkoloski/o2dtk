using UnityEngine;
using UnityEditor;
using System.Collections;
using System.IO;

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
		// The directory to put the converted TMX file in
		private static Object tmx_dir = null;

		[MenuItem("Open 2D/Tile Maps/TMX Converter")]
		public static void OpenTMXConverter()
		{
			window = EditorWindow.GetWindowWithRect(typeof(TMXConverterEditor), new Rect(0, 0, 250, 250), false, "TMX Converter");
		}

		public void OnGUI()
		{
			tmx_file = Utility.GUI.LabeledFileField("TMX file:", tmx_file);

			Utility.GUI.Label("Conversion settings:");

			settings.rebuild_tilesets = Utility.GUI.BeginToggleGroup("Rebuild tilesets", settings.rebuild_tilesets);

			settings.force_rebuild_tilesets = Utility.GUI.Toggle("Force", settings.force_rebuild_tilesets);

			Utility.GUI.EndToggleGroup();

			settings.rebuild_chunks = Utility.GUI.Toggle("Rebuild chunks", settings.rebuild_chunks);

			settings.chunk_width = (uint)Utility.GUI.LabeledIntField("Chunk width:", (int)settings.chunk_width);
			
			settings.chunk_height = (uint)Utility.GUI.LabeledIntField("Chunk height:", (int)settings.chunk_height);

			tmx_dir = Utility.GUI.LabeledDirectoryField("Output directory:", tmx_dir);

			if (Utility.GUI.Button("Convert TMX"))
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
					TMXConverter.LoadTMX(AssetDatabase.GetAssetPath(tmx_file), settings, (tmx_dir == null ? Open2D.settings["tilemaps_root"] : AssetDatabase.GetAssetPath(tmx_dir)));
			}
		}
	}
}
