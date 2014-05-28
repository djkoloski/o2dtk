using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace o2dtk
{
	public class TileLibrary
	{
		// A cache of the tilesets that contain certain GIDs to make lookup faster
		private Dictionary<uint, TileSet> tileset_cache;
		// The tilesets currently in the library
		public List<TileSet> tilesets;

		// Default constructor
		public TileLibrary()
		{
			tileset_cache = new Dictionary<uint, TileSet>();
			tilesets = new List<TileSet>();
		}

		// Finds the tileset that contains the given GID
		public TileSet GetTilesetWithGID(uint gid)
		{
			if (tileset_cache.ContainsKey(gid))
				return tileset_cache[gid];
			
			int index = 0;

			for (; index < tilesets.Count; ++index)
			{
				if (tilesets[index].first_gid > gid)
				{
					--index;
					break;
				}
			}

			if (index < 0 || index >= tilesets.Count || gid - tilesets[index].first_gid >= tilesets[index].materials.Count)
			{
				Debug.LogWarning("Attempted to find tile with GID " + gid + ", but no tile set contains that GID");
				return null;
			}

			tileset_cache[gid] = tilesets[index];
			return tilesets[index];
		}

		// Gets the texture of the tile with the given GID
		public Material GetMaterialByGID(uint gid)
		{
			TileSet ts = GetTilesetWithGID(gid);

			if (ts == null)
				return null;

			gid -= ts.first_gid;

			return ts.materials[(int)gid];
		}

		// Adds a tile set to the library (tile sets must be added in ascending order of GIDs)
		public void AddTileSet(TileSet tileset)
		{
			tilesets.Add(tileset);
		}
	}
}
