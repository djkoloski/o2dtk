using UnityEngine;
using System.Collections;

namespace o2dtk
{
	namespace TileMap
	{
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

			// The animations in the tile set
			//   A tile that is not animated has its respective entry set to null
			public TileAnimation[] animations;

			// Determines whether a tile is animated
			public bool IsTileAnimated(int id)
			{
				if (animations == null || id >= animations.Length)
					return false;

				return (animations[id] != null && animations[id].length > 0);
			}

			// Gets the current local tile ID for an animated tile at a tile in milliseconds
			public int GetAnimatedTileIndex(int id, int milliseconds)
			{
				return animations[id].GetKeyByTime(milliseconds).id;
			}
		}
	}
}
