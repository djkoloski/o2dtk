using UnityEngine;
using System.Collections;

namespace o2dtk
{
	namespace TileMap
	{
		[System.Serializable]
		public class TileChunkUpdateEntry
		{
			// The coordinates of the entry
			public int pos_x;
			public int pos_y;
			// The game object of the entry
			public GameObject game_object;
			// The global ID
			public int global_id;
			// User data (in the case of default rendering, this is the sprite renderer of the tile)
			public object user_data;

			// Default constructor
			public TileChunkUpdateEntry(int x, int y, GameObject go, int id = 0, object ud = null)
			{
				pos_x = x;
				pos_y = y;
				game_object = go;
				global_id = id;
				user_data = ud;
			}
		}
	}
}
