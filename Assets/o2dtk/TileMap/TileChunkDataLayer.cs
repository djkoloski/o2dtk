using UnityEngine;
using System.Collections;

namespace o2dtk
{
	namespace TileMap
	{
		[System.Serializable]
		public class TileChunkDataLayer
		{
			// The IDs of the tiles in the layer
			public int[] ids;

			// Basic Constructor
			public TileChunkDataLayer(int size_x, int size_y)
			{
				ids = new int[size_x * size_y];
			}
		}
	}
}
