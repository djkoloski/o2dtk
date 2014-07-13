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

			// GUI properties
			// Whether the mouse is on the map
			bool on_map = false;
			// The tile coordinates under the mouse
			int tile_x = 0;
			int tile_y = 0;
			// Gets the chunk coordinates under the mouse
			int chunk_x
			{
				get
				{
					return Utility.Math.FloorDivide(tile_x, controller.tile_map.chunk_size_x);
				}
			}
			int chunk_y
			{
				get
				{
					return Utility.Math.FloorDivide(tile_y, controller.tile_map.chunk_size_y);
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

				controller.draw_tile_gridlines = Utility.GUI.LabeledToggle("Draw tile gridlines:", controller.draw_tile_gridlines);
				controller.draw_chunk_gridlines = Utility.GUI.LabeledToggle("Draw chunk gridlines:", controller.draw_chunk_gridlines);
				string[] gridline_options = {"Always", "Selected", "Never"};
				controller.when_draw_gridlines = (TileMapController.GridlinesDrawTime)GUILayout.SelectionGrid((int)controller.when_draw_gridlines, gridline_options, gridline_options.Length);

				if (controller.tile_map == null)
					return;

				GUILayout.Label("Chunk Range: (" + controller.tile_map.chunk_left + "," + controller.tile_map.chunk_bottom + ") - (" + controller.tile_map.chunk_right + "," + controller.tile_map.chunk_top + ")");
			}

			public class ContextCommand
			{
				public string name;
				public object[] args;

				public ContextCommand(string n, object[] a)
				{
					name = n;
					args = a;
				}
			}

			public void ExecuteContext(object command_arg)
			{
				ContextCommand command = command_arg as ContextCommand;

				switch (command.name)
				{
					case "ping_chunk":
						EditorGUIUtility.PingObject(controller.tile_map.GetChunk((int)command.args[0], (int)command.args[1]));
						break;
					case "begin_editing":
						controller.Begin();
						break;
					case "end_editing":
						controller.End();
						break;
					case "load_chunk":
					{
						controller.LoadChunk((int)command.args[0], (int)command.args[1]);
						break;
					}
					case "unload_chunk":
					{
						controller.UnloadChunk((int)command.args[0], (int)command.args[1]);
						break;
					}
					case "load_all_chunks":
						for (int x = controller.tile_map.chunk_left; x <= controller.tile_map.chunk_right; ++x)
							for (int y = controller.tile_map.chunk_bottom; y <= controller.tile_map.chunk_top; ++y)
								controller.LoadChunk(x, y);
						break;
					case "unload_all_chunks":
						for (int x = controller.tile_map.chunk_left; x <= controller.tile_map.chunk_right; ++x)
							for (int y = controller.tile_map.chunk_bottom; y <= controller.tile_map.chunk_top; ++y)
								controller.UnloadChunk(x, y);
						break;
					default:
						Debug.LogWarning("Invalid context command argument!");
						break;
				}
			}

			public void OnSceneGUI()
			{
				if (controller.tile_map == null)
					return;

				Vector3 mouse_pos = new Vector3();
				on_map = Utility.Editor.ProjectMousePosition(Vector3.forward, controller.transform, out mouse_pos);

				bool needs_repaint = false;
				if (on_map)
				{
					int old_x = tile_x;
					int old_y = tile_y;
					controller.NormalToTile(controller.worldToNormalMatrix.MultiplyPoint(mouse_pos), out tile_x, out tile_y);

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
						menu.AddItem(new GUIContent("Begin editing"), false, ExecuteContext, new ContextCommand("begin_editing", null));
					else
					{
						if (controller.IsChunkLoaded(chunk_x, chunk_y))
							menu.AddItem(new GUIContent("Unload chunk"), false, ExecuteContext, new ContextCommand("unload_chunk", new object[2]{chunk_x, chunk_y}));
						else
							menu.AddItem(new GUIContent("Load chunk"), false, ExecuteContext, new ContextCommand("load_chunk", new object[2]{chunk_x, chunk_y}));

						menu.AddItem(new GUIContent("Ping chunk"), false, ExecuteContext, new ContextCommand("ping_chunk", new object[2]{chunk_x, chunk_y}));

						menu.AddSeparator("");

						if (controller.tile_map.size_x >= 0)
							menu.AddItem(new GUIContent("Chunks/Load all chunks"), false, ExecuteContext, new ContextCommand("load_all_chunks", null));
						else
							menu.AddDisabledItem(new GUIContent("Chunks/Load all chunks"));

						if (controller.tile_map.size_y >= 0)
							menu.AddItem(new GUIContent("Chunks/Unload all chunks"), false, ExecuteContext, new ContextCommand("unload_all_chunks", null));
						else
							menu.AddDisabledItem(new GUIContent("Chunks/Unload all chunks"));

						menu.AddSeparator("");
						menu.AddItem(new GUIContent("End editing"), false, ExecuteContext, new ContextCommand("end_editing", null));

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
