using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace o2dtk
{
	namespace TileMap
	{
		[System.Serializable]
		public class TileChunk : ScriptableObject
		{
			// The position of the chunk in tiles
			public int pos_x;
			public int pos_y;
			// The size of the chunk in tiles
			public int size_x;
			public int size_y;

			// The data layers of the chunk
			public List<TileChunkDataLayer> data_layers;
		}
	}
}
