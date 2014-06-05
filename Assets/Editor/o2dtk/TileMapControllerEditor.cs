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
				lower_bound = EditorGUILayout.IntField(lower_bound);
				left_bound = EditorGUILayout.IntField(left_bound);
				GUILayout.EndHorizontal();

				// Get the upper right bound chunk
				GUILayout.BeginHorizontal();
				GUILayout.Label("Top Right Chunk:");
				GUILayout.FlexibleSpace();
				upper_bound = EditorGUILayout.IntField(upper_bound);
				right_bound = EditorGUILayout.IntField(right_bound);
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

				GUI.enabled = true;
			}
		}
	}
}
