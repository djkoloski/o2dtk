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

		// The tiles in the tile set
		public List<Texture2D> tiles;

		public TileSet()
		{
			name = "";

			first_gid = 0;

			tile_width = tile_height = 0;

			tile_offset_x = tile_offset_y = 0;

			tiles = null;
		}

		public void MakeTilesFromImage(string image_path, bool force = false)
		{
			if (tile_width < 1 || tile_height < 1)
			{
				Debug.LogError("Tile width or height less than 1 while making tiles from an atlas image.");
				return;
			}

			string image_tile_dir = Path.Combine(Path.GetDirectoryName(image_path), Path.GetFileNameWithoutExtension(image_path));

			// Configure settings for importing sprite sheets
			TextureImporter tex_imp = AssetImporter.GetAtPath(image_path) as TextureImporter;
			
			tex_imp.textureType = TextureImporterType.Advanced;
			tex_imp.isReadable = true;
			tex_imp.npotScale = TextureImporterNPOTScale.None;
			tex_imp.mipmapEnabled = false;
			tex_imp.filterMode = FilterMode.Point;
			
			AssetDatabase.ImportAsset(image_path);
			
			Texture2D image = AssetDatabase.LoadAssetAtPath(image_path, typeof(Texture2D)) as Texture2D;

			// Make tiles folder
			string tiles_dir = image_tile_dir + "_tiles";
			
			if (!System.IO.Directory.Exists(tiles_dir))
				System.IO.Directory.CreateDirectory(tiles_dir);

			tiles = new List<Texture2D>();

			Color32[] image_pixels = image.GetPixels32();
			Texture2D cur_tile = new Texture2D(tile_width, tile_height);
			Color32[] pixels = new Color32[tile_width * tile_height];

			int cur_x = 0;
			int cur_y = 0;

			// Make as many tiles as possible
			while (cur_y + tile_height <= image.height)
			{
				while (cur_x + tile_width <= image.width)
				{
					string tile_path = Path.Combine(tiles_dir, "tile_" + tiles.Count + ".png");

					if (File.Exists(tile_path) && force)
						File.Delete(tile_path);

					if (!File.Exists(tile_path))
					{
						// Copy over the pixel data
						for (int j = 0; j < tile_height; ++j)
							for (int i = 0; i < tile_width; ++i)
								pixels[j * tile_width + i] = image_pixels[(j + cur_y) * image.width + i + cur_x];

						// Set the pixels then save the tile
						cur_tile.SetPixels32(pixels);

						byte[] bytes = cur_tile.EncodeToPNG();
						FileStream tile_fs = File.OpenWrite(tile_path);
						BinaryWriter bw = new BinaryWriter(tile_fs);
						bw.Write(bytes);
						tile_fs.Close();
					}

					AssetDatabase.ImportAsset(tile_path);
					
					TextureImporter tile_imp = AssetImporter.GetAtPath(tile_path) as TextureImporter;

					tile_imp.textureType = TextureImporterType.Advanced;
					tile_imp.mipmapEnabled = false;
					tile_imp.filterMode = FilterMode.Point;

					AssetDatabase.ImportAsset(tile_path);

					tiles.Add(AssetDatabase.LoadAssetAtPath(tile_path, typeof(Texture2D)) as Texture2D);

					cur_x += tile_width;
				}

				cur_x = 0;
				cur_y += tile_height;
			}

			Texture2D.DestroyImmediate(cur_tile);
		}
	}
}
