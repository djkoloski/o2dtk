using UnityEngine;
using System.Collections;

namespace o2dtk
{
	namespace TileMap
	{
		[System.Serializable]
		public class TileChunkUpdateEntry
		{
			// The game object of the entry
			public GameObject game_object;
			// User data (in the case of default rendering, this is the sprite renderer of the tile)
			public object user_data;

			// Default constructor
			public TileChunkUpdateEntry(GameObject go, object ud = null)
			{
				game_object = go;
				user_data = ud;
			}
		}
	}
}
