using UnityEngine;
using UnityEditor;
using System.Collections;
using System.Collections.Generic;
using System.IO;

namespace o2dtk
{
	public class TileSet
	{
		// The name of the tile set
		public string name;
		// The width of each tile in the tile set
		public uint slice_width;
		// The height of each tile in the tile set
		public uint slice_height;
		// The X offset of each tile in the tile set
		public int offset_x;
		// The Y offset of each tile in the tile set
		public int offset_y;
		// The first GID of the tile set
		public uint first_gid;

		// The materials of the tiles in the tile set
		public List<Material> materials;

		// Default constructor
		public TileSet()
		{
			materials = new List<Material>();
		}

		// Loads the given number of tiles from the given folder
		public void LoadFromDir(string path, uint num_tiles)
		{
			for (uint i = 0; i < num_tiles; ++i)
			{
				string tile_mat_path = Path.Combine(path, "tile_" + i + ".mat");
				Material tile_mat = AssetDatabase.LoadAssetAtPath(tile_mat_path, typeof(Material)) as Material;
				materials.Add(tile_mat);
			}
		}
	}
}
