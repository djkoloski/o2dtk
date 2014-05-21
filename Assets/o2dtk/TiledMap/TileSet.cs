using UnityEngine;
using UnityEditor;
using System.Collections;
using System.Collections.Generic;
using System.IO;

namespace o2dtk
{
	public class TileSet
	{
		// The name of the base shader to make materials from
		private static string base_material_name = "Transparent/Diffuse";
	
		// The name of the tile set
		public string name;

		// The first GID of the tile set
		public uint first_gid;

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
		// The materials for the tiles in the set
		public List<Material> materials;

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

			string image_name = Path.GetFileNameWithoutExtension(image_path);
			string tileset_name = image_name + "_" + tile_width + "x" + tile_height + "_tiles";
			string tiles_dir = Path.Combine(Path.GetDirectoryName(image_path), tileset_name);

			string progress_bar_title = "Loading " + tileset_name + "";

			// Make tiles folder
			if (!System.IO.Directory.Exists(tiles_dir))
				System.IO.Directory.CreateDirectory(tiles_dir);

			EditorUtility.DisplayProgressBar(progress_bar_title, "Preparing tile atlas", 0.0f);

			// Configure settings for importing sprite sheets
			TextureImporter tex_imp = AssetImporter.GetAtPath(image_path) as TextureImporter;
			
			tex_imp.textureType = TextureImporterType.Advanced;
			tex_imp.isReadable = true;
			tex_imp.npotScale = TextureImporterNPOTScale.None;
			tex_imp.mipmapEnabled = false;
			tex_imp.filterMode = FilterMode.Point;
			
			AssetDatabase.ImportAsset(image_path);
			
			Texture2D image = AssetDatabase.LoadAssetAtPath(image_path, typeof(Texture2D)) as Texture2D;

			tiles = new List<Texture2D>();
			materials = new List<Material>();

			Color32[] image_pixels = image.GetPixels32();
			Texture2D cur_tile = new Texture2D(tile_width, tile_height);
			Color32[] pixels = new Color32[tile_width * tile_height];

			int total_tiles = (image.width / tile_width) * (image.height / tile_height);

			int cur_x = 0;
			int cur_y = 0;

			// Make as many tiles as possible
			while (cur_y + tile_height <= image.height)
			{
				while (cur_x + tile_width <= image.width)
				{
					EditorUtility.DisplayProgressBar(progress_bar_title, "Loading tile " + tiles.Count + " of " + total_tiles, (float)tiles.Count / total_tiles);
					
					string tile_path = Path.Combine(tiles_dir, "tile_" + tiles.Count + ".png");
					string material_path = Path.Combine(tiles_dir, "tile_" + tiles.Count + ".mat");

					if (force)
					{
						if (File.Exists(tile_path))
							File.Delete(tile_path);
						if (File.Exists(material_path))
							File.Delete(material_path);
					}

					if (!File.Exists(tile_path))
					{
						// Copy over the pixel data
						for (int i = 0; i < tile_height; ++i)
							System.Array.Copy(image_pixels, (image.height - (i + cur_y) - 1) * image.width + cur_x, pixels, (tile_height - i - 1) * tile_width, tile_width);

						// Set the pixels then save the tile
						cur_tile.SetPixels32(pixels);

						byte[] bytes = cur_tile.EncodeToPNG();
						FileStream tile_fs = File.OpenWrite(tile_path);
						BinaryWriter bw = new BinaryWriter(tile_fs);
						bw.Write(bytes);
						tile_fs.Close();

						AssetDatabase.ImportAsset(tile_path);
					
						TextureImporter tile_imp = AssetImporter.GetAtPath(tile_path) as TextureImporter;

						tile_imp.textureType = TextureImporterType.Advanced;
						tile_imp.textureFormat = TextureImporterFormat.AutomaticTruecolor;
						tile_imp.mipmapEnabled = false;
						tile_imp.filterMode = FilterMode.Point;

						AssetDatabase.ImportAsset(tile_path);
					}

					tiles.Add(AssetDatabase.LoadAssetAtPath(tile_path, typeof(Texture2D)) as Texture2D);

					if (!File.Exists(material_path))
					{
						Material tex_mat = new Material(Shader.Find(base_material_name));
						tex_mat.mainTexture = tiles[tiles.Count - 1];
						AssetDatabase.CreateAsset(tex_mat, material_path);
						AssetDatabase.SaveAssets();
						materials.Add(tex_mat);
					}
					else
						materials.Add(AssetDatabase.LoadAssetAtPath(material_path, typeof(Material)) as Material);

					cur_x += tile_width;
				}

				cur_x = 0;
				cur_y += tile_height;
			}

			EditorUtility.DisplayProgressBar(progress_bar_title, "Finalizing...", 1.0f);

			Texture2D.DestroyImmediate(cur_tile);

			EditorUtility.ClearProgressBar();
		}
	}
}
