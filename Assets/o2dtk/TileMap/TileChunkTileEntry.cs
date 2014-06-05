using UnityEngine;
using System.Collections;

namespace o2dtk
{
	namespace TileMap
	{
		[System.Serializable]
		public class TileChunkTileEntry
		{
			// The coordinates of the entry
			public int pos_x;
			public int pos_y;
			// The game object of the entry
			public GameObject game_object;
			// The original ID
			public int original_id;
			// The sprite renderer
			public SpriteRenderer sprite_renderer;

			// Default constructor
			public TileChunkTileEntry(int x, int y, GameObject go, int o = 0, SpriteRenderer sr = null)
			{
				pos_x = x;
				pos_y = y;
				game_object = go;
				original_id = o;
				sprite_renderer = sr;
			}
		}
	}
}
