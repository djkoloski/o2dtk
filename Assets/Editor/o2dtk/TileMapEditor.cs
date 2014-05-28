using UnityEngine;
using UnityEditor;
using System.Collections;

namespace o2dtk
{
	[CustomEditor(typeof(TileMap))]
	public class TileMapEditor : Editor
	{
		// The GUIs that can be active
		private static string[] gui_names = new string[] { "Info", "Load", "Convert" };

		// The tile map that is currently being edited
		private TileMap tileMap;
		// The current GUI
		private int current_gui;
		// The import settings to be used while loading TMX files
		private TMXImportSettings settings;
		// The tile map file to load from
		private Object tilemap_file;
		// The TMX file to convert from
		private Object tmx_file;

		// The leftmost chunk to load
		private int chunk_left;
		// The rightmost chunk to load
		private int chunk_bottom;
		// The number of chunks to load horizontally
		private int chunks_x;
		// The number of chunks to load vertically
		private int chunks_y;

		// Initialize the editor for a new tile map
		public void OnEnable()
		{
			tileMap = (TileMap)target;
			current_gui = 0;
			settings = new TMXImportSettings();
			tilemap_file = null;
			tmx_file = null;
		}

		// Makes a spaced label in the inspector
		void SpacedLabel(string label, string value)
		{
			GUILayout.BeginHorizontal();
			GUILayout.Label(label);
			GUILayout.FlexibleSpace();
			GUILayout.Label(value);
			GUILayout.EndHorizontal();
		}

		// Draws the information GUI
		void InfoGUI()
		{
			if (tileMap.loaded)
			{
				SpacedLabel("Width:", tileMap.width + " tiles");
				SpacedLabel("Height:", tileMap.height + " tiles");
				SpacedLabel("Tile Layers:", tileMap.tile_layers.Count.ToString());
				SpacedLabel("Object Layers:", tileMap.object_layers.Count.ToString());
				SpacedLabel("Chunk Width:", tileMap.chunk_width.ToString());
				SpacedLabel("Chunk Height:", tileMap.chunk_height.ToString());
				SpacedLabel("Tile width:", tileMap.tile_width.ToString());
				SpacedLabel("Tile height:", tileMap.tile_height.ToString());
			}
			else
			{
				GUILayout.BeginHorizontal();
				GUILayout.Label("Tile map not loaded");
				GUILayout.EndHorizontal();
			}
		}

		// Draws the tile map loading GUI
		void LoadGUI()
		{
			GUILayout.BeginHorizontal();
			GUILayout.Label("Tile map file:");
			GUILayout.FlexibleSpace();
			tilemap_file = EditorGUILayout.ObjectField(tilemap_file, typeof(Object), true);
			GUILayout.EndHorizontal();

			GUILayout.BeginHorizontal();
			if (GUILayout.Button("Load tile map"))
				tileMap.LoadFromFile(tilemap_file);
			GUILayout.EndHorizontal();

			GUI.enabled = tileMap.loaded;
			GUILayout.BeginHorizontal();
			GUILayout.Label("Chunk left:");
			chunk_left = EditorGUILayout.IntField(chunk_left);
			GUILayout.EndHorizontal();
			GUILayout.BeginHorizontal();
			GUILayout.Label("Chunk bottom:");
			chunk_bottom = EditorGUILayout.IntField(chunk_bottom);
			GUILayout.EndHorizontal();
			GUILayout.BeginHorizontal();
			GUILayout.Label("Chunks X:");
			chunks_x = EditorGUILayout.IntField(chunks_x);
			GUILayout.EndHorizontal();
			GUILayout.BeginHorizontal();
			GUILayout.Label("Chunks Y:");
			chunks_y = EditorGUILayout.IntField(chunks_y);
			GUILayout.EndHorizontal();

			GUILayout.BeginHorizontal();
			if (GUILayout.Button("Load chunks"))
			{
				for (uint y = 0; y < chunks_y; ++y)
					for (uint x = 0; x < chunks_x; ++x)
						tileMap.LoadChunk((uint)(chunk_left + x), (uint)(chunk_bottom + y));
			}
			if (GUILayout.Button("Unload chunks"))
			{
				for (uint y = 0; y < chunks_y; ++y)
					for (uint x = 0; x < chunks_x; ++x)
						tileMap.UnloadChunk((uint)(chunk_left + x), (uint)(chunk_bottom + y));
			}
			GUILayout.EndHorizontal();
		}

		void ConvertGUI()
		{
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

			GUI.enabled = settings.rebuild_chunks;
			GUILayout.BeginHorizontal();
			GUILayout.Label("Chunk width:");
			settings.chunk_width = (uint)EditorGUILayout.IntField((int)settings.chunk_width);
			GUILayout.EndHorizontal();
			GUILayout.BeginHorizontal();
			GUILayout.Label("Chunk height:");
			settings.chunk_height = (uint)EditorGUILayout.IntField((int)settings.chunk_height);
			GUILayout.EndHorizontal();
			GUI.enabled = true;

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

		public override void OnInspectorGUI()
		{
			current_gui = GUILayout.SelectionGrid(current_gui, gui_names, gui_names.Length);

			switch (current_gui)
			{
				case 0:
					InfoGUI();
					break;
				case 1:
					LoadGUI();
					break;
				case 2:
					ConvertGUI();
					break;
				default:
					break;
			}
		}
	}
}
