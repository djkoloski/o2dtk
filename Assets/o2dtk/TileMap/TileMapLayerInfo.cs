using UnityEngine;
using System.Collections;

namespace o2dtk
{
	namespace TileMap
	{
		[System.Serializable]
		public class TileMapLayerInfo
		{
			// The name of the tile map layer
			public string name;
			// The size of the layer in tiles
			public int size_x;
			public int size_y;
			// The default alpha value of all tiles in the layer
			public float default_alpha;
		}
	}
}
