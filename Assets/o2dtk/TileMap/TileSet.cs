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
		}
	}
}
