using UnityEngine;
using UnityEditor;
using System.Collections;
using System.Collections.Generic;

namespace o2dtk
{
	namespace TileMap
	{
		[CustomEditor(typeof(TileMapController))]
		public class TileMapControllerEditor : Editor
		{
			// The controller currently being edited
			public TileMapController controller = null;
			//Some Integers
			int lower_bound = 0;
			int left_bound = 0;
			int upper_bound = 0;
			int right_bound = 0;

			// GUI properties
			// Whether the mouse is on the map
			bool on_map = false;
			// Whether the tile is a valid tile
			bool valid_tile
			{
				get
				{
					return (tile_x >= 0 && tile_x < controller.tile_map.size_x && tile_y >= 0 && tile_y < controller.tile_map.size_y);
				}
			}
			// The tile coordinates under the mouse
			int tile_x = 0;
			int tile_y = 0;
			// Gets the chunk coordinates under the mouse
			int chunk_x
			{
				get
				{
					return tile_x / controller.tile_map.chunk_size_x - (tile_x < 0 ? 1 : 0);
				}
			}
			int chunk_y
			{
				get
				{
					return tile_y / controller.tile_map.chunk_size_y - (tile_y < 0 ? 1 : 0);
				}
			}

			public void OnEnable()
			{
				controller = (TileMapController)target;
			}

			public override void OnInspectorGUI()
			{
				controller.tile_map = (TileMap)Utility.GUI.LabeledObjectField("Tile map:", controller.tile_map, typeof(TileMap), false);
				controller.pixels_per_unit = Utility.GUI.LabeledFloatField("Pixels per Unit:", controller.pixels_per_unit);
				controller.render_intercept = (TileMapRenderIntercept)Utility.GUI.LabeledObjectField("Render intercept:", controller.render_intercept, typeof(TileMapRenderIntercept), false);
				controller.update_intercept = (TileMapUpdateIntercept)Utility.GUI.LabeledObjectField("Update intercept:", controller.update_intercept, typeof(TileMapUpdateIntercept), false);

				controller.draw_tile_gridlines = Utility.GUI.LabeledToggle("Draw tile gridlines:", controller.draw_tile_gridlines);
				controller.draw_chunk_gridlines = Utility.GUI.LabeledToggle("Draw chunk gridlines:", controller.draw_chunk_gridlines);
				string[] gridline_options = {"Always", "Selected", "Never"};
				controller.when_draw_gridlines = (TileMapController.GridlinesDrawTime)GUILayout.SelectionGrid((int)controller.when_draw_gridlines, gridline_options, gridline_options.Length);

				GUILayout.BeginHorizontal();
				if (GUILayout.Button("Begin Editing"))
					controller.Begin();
				if (GUILayout.Button("End Editing"))
					controller.End();
				GUILayout.EndHorizontal();

				if (controller.tile_map == null)
					return;

				GUI.enabled = controller.initialized;

				// Input fields for the Bottom Left and Top Right Coordinates
				GUILayout.Label("Chunk Loading and Unloading");
				GUILayout.Label("Chunk Range: (0,0) - (" + controller.tile_map.chunks_x + "," + controller.tile_map.chunks_y + ")");

				// Get the lower left bound chunk
				GUILayout.BeginHorizontal();
				GUILayout.Label("Bottom Left Chunk:");
				GUILayout.FlexibleSpace();
				left_bound = EditorGUILayout.IntField(left_bound);
				lower_bound = EditorGUILayout.IntField(lower_bound);
				GUILayout.EndHorizontal();

				// Get the upper right bound chunk
				GUILayout.BeginHorizontal();
				GUILayout.Label("Top Right Chunk:");
				GUILayout.FlexibleSpace();
				right_bound = EditorGUILayout.IntField(right_bound);
				upper_bound = EditorGUILayout.IntField(upper_bound);
				GUILayout.EndHorizontal();

				// Load and Unload buttons
				GUILayout.BeginHorizontal();

				if (GUILayout.Button("Load Chunks"))
					for (int i = left_bound; i < right_bound; ++i)
						for (int j = lower_bound; j < upper_bound; ++j)
							controller.LoadChunk(i,j);
	
				if (GUILayout.Button("Unload Chunks"))
					for (int i = left_bound; i < right_bound; ++i)
						for (int j = lower_bound; j < upper_bound; ++j)
							controller.UnloadChunk(i,j);

				GUILayout.EndHorizontal();

				// Load and Unload All buttons
				GUILayout.BeginHorizontal();

				if (GUILayout.Button("Load All Chunks"))
					for (int i = 0; i < controller.tile_map.chunks_x; ++i)
						for (int j = 0; j < controller.tile_map.chunks_y; ++j)
							controller.LoadChunk(i,j);
	
				if (GUILayout.Button("Unload All Chunks"))
					for (int i = 0; i < controller.tile_map.chunks_x; ++i)
						for (int j = 0; j < controller.tile_map.chunks_y; ++j)
							controller.UnloadChunk(i,j);

				GUILayout.EndHorizontal();

				GUI.enabled = true;
			}

			public class ContextCommand
			{
				public string name;
				public int tile_x;
				public int tile_y;
				public object arg;

				public ContextCommand(string n, int x, int y, object a)
				{
					name = n;
					tile_x = x;
					tile_y = y;
					arg = a;
				}
			}

			public void ExecuteContext(object command_arg)
			{
				ContextCommand command = command_arg as ContextCommand;

				switch (command.name)
				{
					case "begin_editing":
						controller.Begin();
						break;
					case "end_editing":
						controller.End();
						break;
					case "load_chunk":
					{
						int target_x = command.tile_x / controller.tile_map.chunk_size_x - (command.tile_x < 0 ? 1 : 0);
						int target_y = command.tile_y / controller.tile_map.chunk_size_y - (command.tile_y < 0 ? 1 : 0);
						controller.LoadChunk(target_x, target_y);
						break;
					}
					case "unload_chunk":
					{
						int target_x = command.tile_x / controller.tile_map.chunk_size_x - (command.tile_x < 0 ? 1 : 0);
						int target_y = command.tile_y / controller.tile_map.chunk_size_y - (command.tile_y < 0 ? 1 : 0);
						controller.UnloadChunk(target_x, target_y);
						break;
					}
					case "load_all_chunks":
						for (int x = 0; x < controller.tile_map.chunks_x; ++x)
							for (int y = 0; y < controller.tile_map.chunks_y; ++y)
								controller.LoadChunk(x, y);
						break;
					case "unload_all_chunks":
						for (int x = 0; x < controller.tile_map.chunks_x; ++x)
							for (int y = 0; y < controller.tile_map.chunks_y; ++y)
								controller.UnloadChunk(x, y);
						break;
					default:
						Debug.LogWarning("Invalid context command argument!");
						break;
				}
			}

			public void OnSceneGUI()
			{
				Vector3 mouse_pos = new Vector3();
				on_map = Utility.Editor.ProjectMousePosition(Vector3.forward, controller.transform, out mouse_pos);

				bool needs_repaint = false;
				if (on_map)
				{
					int old_x = tile_x;
					int old_y = tile_y;
					controller.GetTileCoordinates(mouse_pos, out tile_x, out tile_y);

					if (old_x != tile_x || old_y != tile_y)
						needs_repaint = true;
				}

				Handles.BeginGUI();
				GUI.Label(new Rect(10, Screen.height - 60, 200, 20), "Tile: " + tile_x + ", " + tile_y);
				GUI.Label(new Rect(10, Screen.height - 80, 200, 20), "Chunk: " + chunk_x + ", " + chunk_y);
				Handles.EndGUI();

				Event current = Event.current;

				if (current.type == EventType.KeyDown && current.keyCode == KeyCode.Space && on_map)
				{
					GenericMenu menu = new GenericMenu();

					if (!controller.initialized)
						menu.AddItem(new GUIContent("Begin editing"), false, ExecuteContext, new ContextCommand("begin_editing", tile_x, tile_y, null));
					else
					{
						if (controller.IsChunkLoaded(chunk_x, chunk_y))
							menu.AddItem(new GUIContent("Unload chunk"), false, ExecuteContext, new ContextCommand("unload_chunk", tile_x, tile_y, null));
						else
							menu.AddItem(new GUIContent("Load chunk"), false, ExecuteContext, new ContextCommand("load_chunk", tile_x, tile_y, null));

						menu.AddSeparator("");

						if (controller.tile_map.size_x >= 0)
							menu.AddItem(new GUIContent("Chunks/Load all chunks"), false, ExecuteContext, new ContextCommand("load_all_chunks", tile_x, tile_y, null));
						else
							menu.AddDisabledItem(new GUIContent("Chunks/Load all chunks"));

						if (controller.tile_map.size_y >= 0)
							menu.AddItem(new GUIContent("Chunks/Unload all chunks"), false, ExecuteContext, new ContextCommand("unload_all_chunks", tile_x, tile_y, null));
						else
							menu.AddDisabledItem(new GUIContent("Chunks/Unload all chunks"));

						menu.AddSeparator("");
						menu.AddItem(new GUIContent("End editing"), false, ExecuteContext, new ContextCommand("end_editing", tile_x, tile_y, null));

					}

					menu.ShowAsContext();

					current.Use();
				}

				if (needs_repaint)
					SceneView.lastActiveSceneView.Repaint();
			}
		}
	}
}
