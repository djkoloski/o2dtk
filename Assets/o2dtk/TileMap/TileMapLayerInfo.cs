using UnityEngine;
using System.Collections;
using o2dtk.Collections;

namespace o2dtk
{
	namespace TileMap
	{
		[System.Serializable]
		public class TileMapLayerInfo
		{
			// The name of the tile map layer
			public string name;
			// The default alpha value of all tiles in the layer
			public float default_alpha;
			// The properties of the layer
			public PropertyMap properties;

			// The name of the sorting layer this layer uses
			public string unity_sorting_layer_name;
			// The unique ID of the sorting layer this layer uses
			public int unity_sorting_layer_unique_id;

			// Default constructor
			public TileMapLayerInfo()
			{
				name = "";
				default_alpha = 1.0f;
				properties = new PropertyMap();
			}
		}
	}
}
