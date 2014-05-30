using UnityEngine;
using UnityEditor;
using System.Collections;

namespace o2dtk
{
	[CustomEditor(typeof(TileMap))]
	public class TileMapEditor : Editor
	{
		// The GUIs that can be active
		private static string[] gui_names = new string[] { "Info", "Load" };

		// The tile map that is currently being edited
		private TileMap tileMap;
		// The current GUI
		private int current_gui;
		// The tile map file to load from
		private Object tilemap_file;

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

			if (tileMap.tilemap_file)
			{
				if (!tileMap.loaded)
					tileMap.LoadFromFile(tileMap.tilemap_file);
				tilemap_file = tileMap.tilemap_file;
			}
			else
				tilemap_file = null;
		}

		// Draws the information GUI
		void InfoGUI()
		{
			if (tileMap.loaded)
			{
				Utility.GUI.Label("Width:", tileMap.width + " tiles");
				Utility.GUI.Label("Height:", tileMap.height + " tiles");
				Utility.GUI.Label("Tile Layers:", tileMap.tile_layers.Count.ToString());
				Utility.GUI.Label("Object Layers:", tileMap.object_layers.Count.ToString());
				Utility.GUI.Label("Chunk Width:", tileMap.chunk_width.ToString());
				Utility.GUI.Label("Chunk Height:", tileMap.chunk_height.ToString());
				Utility.GUI.Label("Tile width:", tileMap.tile_width.ToString());
				Utility.GUI.Label("Tile height:", tileMap.tile_height.ToString());
			}
			else
				Utility.GUI.Label("Tile map not loaded");
		}

		// Draws the tile map loading GUI
		void LoadGUI()
		{
			if (Utility.GUI.Button("Unload tile map"))
				tileMap.Clear();

			tilemap_file = Utility.GUI.LabeledFileField("Tile map file:", tilemap_file);

			if (Utility.GUI.Button("Load tile map"))
				tileMap.LoadFromFile(tilemap_file);

			GUI.enabled = tileMap.loaded;
			
			Utility.GUI.Label("Chunk Range: (0,0) - (" + tileMap.chunks_x + "," + tileMap.chunks_y + ")" );
			Utility.GUI.Label("Bottom Left Chunk (x,y):");

			GUILayout.BeginHorizontal();
			chunk_left = EditorGUILayout.IntField(chunk_left);
			chunk_bottom = EditorGUILayout.IntField(chunk_bottom);
			GUILayout.EndHorizontal();
			
			Utility.GUI.Label("Top Right Chunk (x,y):");
			
			GUILayout.BeginHorizontal();
			chunks_x = EditorGUILayout.IntField(chunks_x);
			chunks_y = EditorGUILayout.IntField(chunks_y);
			GUILayout.EndHorizontal();

			GUILayout.BeginHorizontal();
			if (GUILayout.Button("Load chunks"))
			{
				for (uint y = 0; y < chunks_y - chunk_bottom; ++y)
					for (uint x = 0; x < chunks_x - chunk_left; ++x)
						tileMap.LoadChunk((uint)(chunk_left + x), (uint)(chunk_bottom + y));
			}
			if (GUILayout.Button("Unload chunks"))
			{
				for (uint y = 0; y < chunks_y - chunk_bottom; ++y)
					for (uint x = 0; x < chunks_x - chunk_left; ++x)
						tileMap.UnloadChunk((uint)(chunk_left + x), (uint)(chunk_bottom + y));
			}
			GUILayout.EndHorizontal();

			GUI.enabled = true;
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
				default:
					break;
			}
		}
	}
}
