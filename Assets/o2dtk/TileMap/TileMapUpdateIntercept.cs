using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using o2dtk.Collections;

namespace o2dtk
{
	namespace TileMap
	{
		public class TileMapUpdateIntercept : ScriptableObject
		{
			// Takes in a tile chunk tile entry and updates it every frame.
			//   Returns whether the update was intercepted. If true is returned,
			//   the default update is not run, if false is, the default update is
			//   run on the tile entry
			public virtual bool InterceptTileUpdate(TileChunkController chunk_controller, int x, int y, int layer_index, TileChunkUpdateEntry entry)
			{
				return false;
			}
		}
	}
}
