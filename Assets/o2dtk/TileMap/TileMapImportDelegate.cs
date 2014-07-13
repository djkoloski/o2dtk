using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace o2dtk
{
	namespace TileMap
	{
		public class TileMapImportDelegate : ScriptableObject
		{
			// Takes in information about the objects in the tile map and creates
			//   user data. The given tile map object belongs to the passed tile map
			public virtual void ImportTileMapObjects(List<TileMapObject> objects, TileMap tile_map)
			{ }
		}
	}
}
