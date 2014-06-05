using UnityEngine;
using System.Collections;

namespace o2dtk
{
	namespace TileMap
	{
		[System.Serializable]
		public class TileChunkAnimatedSpriteEntry
		{
			// The original ID
			public int original_id;
			// The sprite renderer
			public SpriteRenderer sprite_renderer;

			// Default constructor
			public TileChunkAnimatedSpriteEntry(int o = 0, SpriteRenderer sr = null)
			{
				original_id = o;
				sprite_renderer = sr;
			}
		}
	}
}
