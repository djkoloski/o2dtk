using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace o2dtk
{
	public class TileLibrary
	{
		private Dictionary<uint, TileSet> tileset_cache;
		
		public List<TileSet> tilesets;

		public TileLibrary()
		{
			tileset_cache = new Dictionary<uint, TileSet>();
			tilesets = new List<TileSet>();
		}

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

			if (index < 0)
			{
				tileset_cache[gid] = null;
				return null;
			}

			if (gid - tilesets[index].first_gid >= tilesets[index].tiles.Count)
			{
				tileset_cache[gid] = null;
				return null;
			}

			tileset_cache[gid] = tilesets[index];
			return tilesets[index];
		}

		public Texture2D GetTextureByGID(uint gid)
		{
			TileSet ts = GetTilesetWithGID(gid);

			if (ts == null)
				return null;
			
			gid -= ts.first_gid;
			
			return ts.tiles[(int)gid];
		}

		public Material GetMaterialByGID(uint gid)
		{
			TileSet ts = GetTilesetWithGID(gid);

			if (ts == null)
				return null;

			gid -= ts.first_gid;

			return ts.materials[(int)gid];
		}

		public void AddTileSet(TileSet tileset)
		{
			tilesets.Add(tileset);
		}
	}
}
