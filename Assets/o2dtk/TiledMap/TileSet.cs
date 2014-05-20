using UnityEngine;
using System.Collections;

namespace o2dtk
{
	public class TileSet
	{
		// The name of the tile set
		public string name;

		// The first GID of the tile set
		public int first_gid;

		// The width of each tile in the tile set
		public int tile_width;
		// The height of each tile in the tile set
		public int tile_height;

		// The X offset of each tile in the tile set
		public int tile_offset_x;
		// The Y offste of each tile in the tile set
		public int tile_offset_y;

		// The image for the tile set (either a file or loaded on the fly)
		public Texture2D image;

		// The tiles in the tile set
		public Texture2D[] tiles;

		public TileSet()
		{
			name = "";

			first_gid = 0;

			tile_width = tile_height = 0;

			tile_offset_x = tile_offset_y = 0;

			image = null;

			tiles = null;
		}

		public void MakeTilesFromImage()
		{
			if (tile_width < 1 || tile_height < 1)
			{
				Debug.LogError("Tile width or height less than 1 while making tiles from an atlas image.");
				return;
			}

			Color32[] image_pixels = image.GetPixels32();
			Texture2D cur_tile = new Texture2D(tile_width, tile_height);
			Color32[] pixels;

			int cur_x = 0;
			int cur_y = 0;

			while (cur_y + tile_height <= image.height)
			{
				while (cur_x + tile_width <= image.width)
				{
					// TODO write code for slicing
				}

				cur_x = 0;
				cur_y += image.height;
			}
		}
	}
}
