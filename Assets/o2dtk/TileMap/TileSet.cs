using UnityEngine;
using System.Collections;
using o2dtk.Collections;

namespace o2dtk
{
	namespace TileMap
	{
		[System.Serializable]
		public class TileData
		{
			public PropertyMap properties;
			public TileAnimation animation;
		}

		[System.Serializable]
		public class TileDataMap : Map<int, TileData>
		{ }

		[System.Serializable]
		public class TileSet : ScriptableObject
		{
			// The size of each tile in the tile set in pixels
			public int slice_size_x;
			public int slice_size_y;
			// The offset to render each tile with
			public int offset_x;
			public int offset_y;

			// The sprites in the tile set
			public Sprite[] tiles;

			// Data for the tiles in the tile set
			public TileDataMap tile_data;

			// Determines whether a tile is animated
			public bool IsTileAnimated(int id)
			{
				if (!tile_data.ContainsKey(id))
					return false;

				TileData data = tile_data[id];

				return (data.animation != null && data.animation.length > 0);
			}

			// Gets the current local tile ID for an animated tile at a tile in milliseconds
			public int GetAnimatedTileIndex(int id, int milliseconds)
			{
				return tile_data[id].animation.GetKeyByTime(milliseconds).id;
			}
		}
	}
}
