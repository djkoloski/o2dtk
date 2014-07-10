using UnityEngine;
using System.Collections;
using System.Collections.Generic;

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
			public virtual bool InterceptTileUpdate(TileChunkController chunk_controller, TileChunkUpdateEntry entry)
			{
				return false;
			}
		}
	}
}
