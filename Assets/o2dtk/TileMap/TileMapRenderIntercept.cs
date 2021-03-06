using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace o2dtk
{
	namespace TileMap
	{
		public class TileMapRenderIntercept : ScriptableObject
		{
			// Takes in information about the objects in the tile map and creates
			//   user data. The given tile map object belongs to the passed tile map.
			//   Returns whether rendering was replaced by the intercept: true will
			//   prevent the controller from rendering a tile, false will continue
			//   rendering a tile.
			public virtual bool InterceptTileRender(
				TileChunkController chunk_controller,
				Transform layer_transform,
				Quaternion rotation,
				Vector3 scale,
				int local_x,
				int local_y,
				int layer_index,
				int global_id
				)
			{
				return false;
			}
		}
	}
}
