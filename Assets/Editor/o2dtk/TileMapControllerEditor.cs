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

			void OnEnable()
			{
				controller = (TileMapController)target;
			}

			void OnGUI()
			{
				controller.tile_map = (TileMap)Utility.GUI.LabeledObjectField("Tile map:", controller.tile_map, typeof(TileMap), false);

				controller.pixels_per_unit = Utility.GUI.LabeledFloatField("Pixels per Unit:", controller.pixels_per_unit);
			}
		}
	}
}
