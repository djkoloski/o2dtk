using UnityEngine;
using UnityEditor;
using System.Collections;

namespace o2dtk
{
	[CustomEditor(typeof(TiledMap))]
	public class TiledMapEditor : Editor
	{
		private static string[] gui_names = new string[] { "Info", "Load" };
		
		private TiledMap tiledMap_;
		private int current_gui_;
		private TiledMapImportSettings settings_;

		private bool show_map_info = false;
		private bool show_tileset_info = false;
		private bool show_chunk_info = false;

		public void OnEnable()
		{
			tiledMap_ = (TiledMap)target;

			current_gui_ = 0;
			settings_ = new TiledMapImportSettings();

			show_map_info = show_tileset_info = show_chunk_info = false;
		}

		void SpacedLabel(string label, string value)
		{
			GUILayout.BeginHorizontal();
			GUILayout.Label(label);
			GUILayout.FlexibleSpace();
			GUILayout.Label(value);
			GUILayout.EndHorizontal();
		}

		void InfoGUI()
		{
			if (tiledMap_.tiles_loaded)
			{
				show_map_info = EditorGUILayout.Foldout(show_map_info, "Tiled map info");
				if (show_map_info)
				{
					SpacedLabel("Width:", tiledMap_.width + " tiles");
					SpacedLabel("Height:", tiledMap_.height + " tiles");
					SpacedLabel("Layers:", tiledMap_.layers.Count.ToString());

					if (tiledMap_.chunked)
					{
						show_chunk_info = EditorGUILayout.Foldout(show_chunk_info, "Chunking info");

						if (show_chunk_info)
						{
							SpacedLabel("Chunk width:", tiledMap_.chunk_width.ToString());
							SpacedLabel("Chunk height:", tiledMap_.chunk_height.ToString());
						}
					}
					else
					{
						GUILayout.BeginHorizontal();
						GUILayout.Label("Map not chunked");
						GUILayout.EndHorizontal();
					}
				}
			}
			else
			{
				GUILayout.BeginHorizontal();
				GUILayout.Label("Map not loaded");
				GUILayout.EndHorizontal();
			}

			if (tiledMap_.tilesets_loaded)
			{
				show_tileset_info = EditorGUILayout.Foldout(show_tileset_info, "Tilesets info");
				if (show_tileset_info)
				{
					SpacedLabel("Tile width:", tiledMap_.tile_width.ToString());
					SpacedLabel("Tile height:", tiledMap_.tile_height.ToString());
				}
			}
		}

		void LoadGUI()
		{
			GUILayout.BeginHorizontal();
			tiledMap_.tiledMapFile = EditorGUILayout.ObjectField(tiledMap_.tiledMapFile, typeof(Object), true);
			GUILayout.EndHorizontal();

			GUILayout.BeginHorizontal();
			GUILayout.Label("Load settings:");
			GUILayout.EndHorizontal();

			GUILayout.BeginHorizontal();
			settings_.slice_tilesets = GUILayout.Toggle(settings_.slice_tilesets, "Slice tilesets");
			GUILayout.EndHorizontal();

			GUILayout.BeginHorizontal();
			GUI.enabled = settings_.slice_tilesets;
			settings_.rebuild_tilesets = GUILayout.Toggle(settings_.rebuild_tilesets && GUI.enabled, "Rebuild all tilesets");
			GUI.enabled = true;
			GUILayout.EndHorizontal();

			GUILayout.BeginHorizontal();
			GUI.enabled = settings_.slice_tilesets || tiledMap_.tilesets_loaded;
			settings_.load_map = GUILayout.Toggle(settings_.load_map && GUI.enabled, "Load map tiles");
			GUI.enabled = true;
			GUILayout.EndHorizontal();

			GUILayout.BeginHorizontal();
			GUI.enabled = settings_.load_map;
			settings_.chunk_map = GUILayout.Toggle(settings_.chunk_map && GUI.enabled, "Chunk map");
			GUILayout.EndHorizontal();

			GUILayout.BeginHorizontal();
			GUI.enabled = settings_.chunk_map;
			settings_.chunk_width = EditorGUILayout.IntField(settings_.chunk_width);
			settings_.chunk_height = EditorGUILayout.IntField(settings_.chunk_height);
			GUI.enabled = true;
			GUILayout.EndHorizontal();

			GUILayout.BeginHorizontal();
			if (GUILayout.Button("Load tiled map"))
			{
				bool load = true;

				if (settings_.rebuild_tilesets)
					if (!EditorUtility.DisplayDialog("Rebuild all tilesets?", "This may take a while. Tilesets will be rebuilt regardless of whether or not they are already sliced. Rebuild all tilesets?", "Rebuild", "Cancel"))
						load = false;

				if (settings_.chunk_map && (settings_.chunk_width < 1 || settings_.chunk_height < 1))
				{
					EditorUtility.DisplayDialog("Invalid chunk size", "The chunk width and height must be greater than 0 if chunking.", "OK");
					load = false;
				}
				
				if (load)
					tiledMap_.LoadTiledMap(settings_);
			}
			GUILayout.EndHorizontal();
		}

		public override void OnInspectorGUI()
		{
			current_gui_ = GUILayout.SelectionGrid(current_gui_, gui_names, gui_names.Length);

			switch (current_gui_)
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
