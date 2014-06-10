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
			// Whether to flip axis precedences
			public bool flip_major_precedence = false;
			public bool flip_minor_precedence = false;
			// The directory to put the converted tile map in
			public Object output_dir = null;
			// The directory to put the tile sets in
			public Object tile_sets_dir = null;
			// The directory to put the map's resources in
			public Object resources_dir = null;
			// The file to use as a custom importer
			public MonoScript importer_file = null;

			[MenuItem("Open 2D/Tile Maps/TMX Importer")]
			public static void OpenTMXConverter()
			{
				EditorWindow.GetWindow(typeof(TMXImporterEditor), false, "TMX Importer", true);
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

				flip_major_precedence = Utility.GUI.LabeledToggle("Flip major (X) axis precedence", flip_major_precedence);

				flip_minor_precedence = Utility.GUI.LabeledToggle("Flip minor (Y) axis precedence", flip_minor_precedence);

				importer_file = Utility.GUI.LabeledMonoScriptField("Import delegate:", importer_file);

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
					settings.flip_major_precedence = flip_major_precedence;
					settings.flip_minor_precedence = flip_minor_precedence;
					settings.output_dir = AssetDatabase.GetAssetPath(output_dir);
					settings.tile_sets_dir = AssetDatabase.GetAssetPath(tile_sets_dir);
					settings.resources_dir = Path.Combine(AssetDatabase.GetAssetPath(resources_dir), settings.output_name);
					if (importer_file != null)
					{
						System.Type importer_type = importer_file.GetClass();
						if (importer_type != null)
						{
							if (Utility.TypeInfo.ClassImplementsInterface(importer_type, typeof(TileMapImportDelegate)))
								settings.importer = System.Activator.CreateInstance(importer_type) as TileMapImportDelegate;
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
