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

			public void OnEnable()
			{
				controller = (TileMapController)target;
			}

			public override void OnInspectorGUI()
			{
				controller.tile_map = (TileMap)Utility.GUI.LabeledObjectField("Tile map:", controller.tile_map, typeof(TileMap), false);
				controller.pixels_per_unit = Utility.GUI.LabeledFloatField("Pixels per Unit:", controller.pixels_per_unit);

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

			/*
			 * TODO use this to test staggered isometric world-to-tile mapping
			public void OnSceneGUI()
			{
				Plane plane = new Plane(controller.transform.TransformDirection(Vector3.forward), controller.transform.position);
				Ray ray = HandleUtility.GUIPointToWorldRay(Event.current.mousePosition);
				Vector3 hit = new Vector3();
				float dist;

				if (plane.Raycast(ray, out dist))
				{
					hit = ray.origin + (ray.direction.normalized * dist);
				}

				int x = 0;
				int y = 0;

				controller.GetTileCoordinates(hit, out x, out y);
				Debug.Log("Position: " + x + "," + y);
			}
			 */
		}
	}
}
