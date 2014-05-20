using UnityEngine;
using System.Collections;
using System.Collections.Generic;

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
		public List<Texture2D> tiles;

		public TileSet()
		{
			name = "";

			first_gid = 0;

			tile_width = tile_height = 0;

			tile_offset_x = tile_offset_y = 0;

			image = null;

			tiles = null;
		}

		~TileSet()
		{
			foreach (Texture2D tile in tiles)
				Texture2D.Destroy(tile);
			tiles.Clear();
		}

		public void MakeTilesFromImage()
		{
			if (tile_width < 1 || tile_height < 1)
			{
				Debug.LogError("Tile width or height less than 1 while making tiles from an atlas image.");
				return;
			}

			tiles = new List<Texture2D>();

			Color32[] image_pixels = image.GetPixels32();
			Texture2D cur_tile;
			Color32[] pixels = new Color32[tile_width * tile_height];

			int cur_x = 0;
			int cur_y = 0;

			// Make as many tiles as possible
			while (cur_y + tile_height <= image.height)
			{
				while (cur_x + tile_width <= image.width)
				{
					cur_tile = new Texture2D(tile_width, tile_height);

					// Copy over the pixel data
					for (int j = 0; j < tile_height; ++j)
					{
						for (int i = 0; i < tile_width; ++i)
						{
							pixels[j * tile_width + i] = image_pixels[(j + cur_y) * tile_width + i + cur_x];
						}
					}

					// Set the pixels then save the tile
					cur_tile.SetPixels32(pixels);
					cur_tile.name = name + "_tile_" + tiles.Count;
					cur_tile.filterMode = FilterMode.Point;
					cur_tile.wrapMode = TextureWrapMode.Repeat;
					tiles.Add(cur_tile);

					cur_x += tile_width;
				}

				cur_x = 0;
				cur_y += tile_height;
			}
		}
	}
}
