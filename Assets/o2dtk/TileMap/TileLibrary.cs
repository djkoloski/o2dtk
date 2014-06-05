using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace o2dtk
{
	namespace TileMap
	{
		[System.Serializable]
		public class TileLibrary
		{
			[System.Serializable]
			private class TileSetInfo : System.IComparable<TileSetInfo>
			{
				// The tile set
				public TileSet tile_set;
				// The first ID in the tile set
				public int first_id;

				// Basic constructor
				public TileSetInfo(TileSet ts, int fid)
				{
					tile_set = ts;
					first_id = fid;
				}

				// Compares two tile set infos' first ids against each other
				public int CompareTo(TileSetInfo other)
				{
					if (other.first_id > first_id)
						return -1;
					else if (other.first_id < first_id)
						return 1;
					else
						return 0;
				}
			}
			
			// The tile sets in the library sorted by first ID in the tile set
			[SerializeField]
			private List<TileSetInfo> tile_sets;

			// Default constructor
			public TileLibrary()
			{
				tile_sets = new List<TileSetInfo>();
			}

			// Adds a tile set to the library starting with the given ID
			public void AddTileSet(TileSet tile_set, int first_id)
			{
				TileSetInfo tsi = new TileSetInfo(tile_set, first_id);
				int nearest = tile_sets.BinarySearch(tsi);

				if (nearest > 0)
					return;

				tile_sets.Insert(~nearest, tsi);
			}

			// Gets the tile set that has the given ID in its range and
			//   adjusts the passed ID to the local index in the tile set
			public TileSet GetTileSetAndIndex(ref int id)
			{
				int i = 0;

				for (; i < tile_sets.Count; ++i)
					if (tile_sets[i].first_id > id)
						break;

				--i;

				if (i < 0 || i >= tile_sets.Count || id - tile_sets[i].first_id >= tile_sets[i].tile_set.tiles.Length)
					return null;

				id -= tile_sets[i].first_id;
				return tile_sets[i].tile_set;
			}
		}
	}
}
