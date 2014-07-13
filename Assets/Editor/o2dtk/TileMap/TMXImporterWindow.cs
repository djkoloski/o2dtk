using UnityEngine;
using UnityEditor;
using System.Collections;
using System.IO;

namespace o2dtk
{
	namespace TileMap
	{
		public class TMXImporterWindow : EditorWindow
		{
			// The TMX file to convert from
			string input_path = "";
			// The output name for the tile map
			public string output_name = "";
			// Whether tile sets should be forced to be rebuilt
			public bool force_rebuild_tile_sets = false;
			// Whether or not chunks should be rebuilt
			public bool rebuild_chunks = false;
			// The chunk size
			public int chunk_size_x = 0;
			public int chunk_size_y = 0;
			// The origin to import the map with
			public TileMap.Origin origin = TileMap.Origin.BottomLeft;
			// The placement of the origin of the map
			public int origin_x = 0;
			public int origin_y = 0;
			// Whether to flip axis precedences
			public bool flip_precedence_x = false;
			public bool flip_precedence_y = false;
			// The directory to put the converted tile map in
			public Object output_dir = null;
			// The directory to put the tile sets in
			public Object tile_sets_dir = null;
			// The directory to put the map's resources in
			public Object resources_dir = null;
			// The asset to use as a custom importer
			public Object importer = null;

			[MenuItem("Open 2D/Tile Maps/TMX Importer")]
			public static void OpenTMXConverter()
			{
				EditorWindow.GetWindow(typeof(TMXImporterWindow), false, "TMX Importer", true);
			}

			public void OnGUI()
			{
				Utility.GUI.Label("TMX File:");

				GUILayout.BeginHorizontal();
				input_path = EditorGUILayout.TextField(input_path);
				GUILayout.FlexibleSpace();
				if (GUILayout.Button("..."))
				{
					input_path = EditorUtility.OpenFilePanel("TMX File", input_path, "tmx");
					output_name = Path.GetFileNameWithoutExtension(input_path);
				}
				GUILayout.EndHorizontal();

				Utility.GUI.Label("Import settings:");

				output_dir = Utility.GUI.LabeledDirectoryField("Tile map output directory:", output_dir);

				output_name = Utility.GUI.LabeledTextField("Output name:", output_name);

				tile_sets_dir = Utility.GUI.LabeledDirectoryField("Tile sets directory:", tile_sets_dir);

				resources_dir = Utility.GUI.LabeledDirectoryField("Resources directory:", resources_dir);

				force_rebuild_tile_sets = Utility.GUI.LabeledToggle("Force rebuild tile sets", force_rebuild_tile_sets);

				rebuild_chunks = Utility.GUI.LabeledToggle("Rebuild chunks", rebuild_chunks);

				chunk_size_x = Utility.GUI.LabeledIntField("Chunk width:", chunk_size_x);

				chunk_size_y = Utility.GUI.LabeledIntField("Chunk height:", chunk_size_y);

				origin = (TileMap.Origin)Utility.GUI.LabeledEnumField("Map origin:", origin);

				origin_x = Utility.GUI.LabeledIntField("Origin X:", origin_x);

				origin_y = Utility.GUI.LabeledIntField("Origin Y:", origin_y);

				flip_precedence_x = Utility.GUI.LabeledToggle("Flip X axis precedence", flip_precedence_x);

				flip_precedence_y = Utility.GUI.LabeledToggle("Flip Y axis precedence", flip_precedence_y);

				importer = Utility.GUI.LabeledObjectField("Import delegate:", importer);

				if (Utility.GUI.Button("Import TMX"))
				{
					bool import = true;

					TMXImportSettings settings = new TMXImportSettings();
					settings.input_path = input_path;
					settings.output_name = output_name;
					settings.force_rebuild_tile_sets = force_rebuild_tile_sets;
					settings.rebuild_chunks = rebuild_chunks;
					settings.chunk_size_x = chunk_size_x;
					settings.chunk_size_y = chunk_size_y;
					settings.origin = origin;
					settings.origin_x = origin_x;
					settings.origin_y = origin_y;
					settings.flip_precedence_x = flip_precedence_x;
					settings.flip_precedence_y = flip_precedence_y;
					settings.output_dir = AssetDatabase.GetAssetPath(output_dir);
					settings.tile_sets_dir = AssetDatabase.GetAssetPath(tile_sets_dir);
					settings.resources_dir = Path.Combine(AssetDatabase.GetAssetPath(resources_dir), settings.output_name);

					if (importer != null)
					{
						System.Type importer_type = importer.GetType();
						if (importer_type != null)
						{
							if (importer_type.IsSubclassOf(typeof(TileMapImportDelegate)))
								settings.importer = (TileMapImportDelegate)importer;
							else
							{
								EditorUtility.DisplayDialog("Invalid import delegate", "Import delegate found in file ('" + importer_type.FullName + "') does not implement TileMapImportDelegate.", "OK");
								import = false;
							}
						}
						else
						{
							EditorUtility.DisplayDialog("Invalid import delegate", "Import delegate file does not contain a valid import delegate", "OK");
							import = false;
						}
					}

					if (import)
						TMXImporter.ImportTMX(settings);
				}
			}
		}
	}
}
