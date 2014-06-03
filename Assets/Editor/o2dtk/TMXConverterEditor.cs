using UnityEngine;
using UnityEditor;
using System.Collections;
using System.IO;

namespace o2dtk
{
	namespace TileMap
	{
		public class TMXImporterEditor : EditorWindow
		{
			// The current editor window instance
			private static EditorWindow window = null;

			// The settings to import the TMX file with
			private static TMXImportSettings settings = new TMXImportSettings();
			// The TMX file to convert from
			private static Object input_file = null;
			// The directory to put the converted tile map in
			private static Object output_dir = null;
			// The directory to put the tile sets in
			private static Object tile_sets_dir = null;
			// The directory to put the map's resources in
			private static Object resources_dir = null;

			[MenuItem("Open 2D/Tile Maps/TMX Importer")]
			public static void OpenTMXConverter()
			{
				if (window != null)
					Debug.Log("TMX Importer already open");
				else
					window = EditorWindow.GetWindow(typeof(TMXImporterEditor));
			}

			public void OnGUI()
			{
				input_file = Utility.GUI.LabeledFileField("TMX File:", input_file);

				Utility.GUI.Label("Import settings:");

				output_dir = Utility.GUI.LabeledDirectoryField("Tile map output directory:", output_dir);

				settings.output_name = Utility.GUI.LabeledTextField("Output name:", settings.output_name);

				tile_sets_dir = Utility.GUI.LabeledDirectoryField("Tile sets directory:", tile_sets_dir);

				resources_dir = Utility.GUI.LabeledDirectoryField("Resources directory:", resources_dir);

				settings.rebuild_chunks = Utility.GUI.LabeledToggle("Rebuild chunks:", settings.rebuild_chunks);

				settings.chunk_size_x = Utility.GUI.LabeledIntField("Chunk width:", settings.chunk_size_x);

				settings.chunk_size_y = Utility.GUI.LabeledIntField("Chunk height:", settings.chunk_size_y);

				if (Utility.GUI.Button("Import TMX"))
				{
					settings.input_path = AssetDatabase.GetAssetPath(input_file);
					settings.output_dir = AssetDatabase.GetAssetPath(output_dir);
					settings.tile_sets_dir = AssetDatabase.GetAssetPath(tile_sets_dir);
					settings.resources_dir = Path.Combine(AssetDatabase.GetAssetPath(resources_dir), settings.output_name);

					TMXImporter.ImportTMX(settings);
				}
			}
		}
	}
}
