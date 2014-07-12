using UnityEngine;
using UnityEditor;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using o2dtk.Collections;

namespace o2dtk
{
	namespace TileMap
	{
		class Converter
		{
			// Gets the new map index of a tile given its map index in a top-left origin
			public static int PivotTopLeftOrigin(int map_index, int size_x, int size_y, TileMap.Origin new_origin)
			{
				int x = map_index % size_x;
				int y = map_index / size_x;
				if (new_origin == TileMap.Origin.TopRight || new_origin == TileMap.Origin.BottomRight)
					x = size_x - x - 1;
				if (new_origin == TileMap.Origin.BottomLeft || new_origin == TileMap.Origin.BottomRight)
					y = size_y - y - 1;
				return y * size_x + x;
			}

			// Gets a path relative to the resources directory instead of the assets directory
			public static string GetRelativeResourcesPath(string assets_path)
			{
				return assets_path.Remove(0, 17).Replace('\\', '/');
			}

			// Makes a sliced tile sprite asset name from its coordinates
			public static string MakeTileSpriteAssetName(int size_x, int size_y, int index)
			{
				return "tile_" + size_x + "x" + size_y + "_" + index;
			}

			// Parses a sliced tile sprite asset name into its coordinates or fails
			public static bool ParseTileSpriteAssetName(string asset_name, out int size_x, out int size_y, out int index)
			{
				string[] coords = asset_name.Split(new char[] {'x', '_'});

				size_x = 0;
				size_y = 0;
				index = 0;

				if (coords.Length != 4)
					return false;

				if (coords[0] != "tile")
					return false;

				if (!int.TryParse(coords[1], out size_x) || !int.TryParse(coords[2], out size_y) || !int.TryParse(coords[3], out index))
					return false;

				return true;
			}

			// Makes a new tile set from an atlas and a slicing size then saves it to the tile set directory
			public static TileSet MakeTileSet(
				string source_path,
				int margin_x, int margin_y,
				int spacing_x, int spacing_y,
				int slice_size_x, int slice_size_y,
				int offset_x, int offset_y,
				int transparent_color,
				TileAnimation[] animations,
				PropertyMap[] properties,
				string tile_sets_dir,
				bool force_rebuild
				)
			{
				string dest_path = tile_sets_dir + "/" + Path.GetFileName(source_path);
				string name = Path.GetFileNameWithoutExtension(source_path) + "_" + slice_size_x + "x" + slice_size_y;
				string tile_set_dest = tile_sets_dir + "/" + name + ".asset";

				if (File.Exists(tile_set_dest) && !force_rebuild)
					return AssetDatabase.LoadAssetAtPath(tile_set_dest, typeof(TileSet)) as TileSet;

				if (!File.Exists(dest_path))
				{
					File.Copy(source_path, dest_path, false);
					AssetDatabase.ImportAsset(dest_path);
				}

				TextureImporter importer = AssetImporter.GetAtPath(dest_path) as TextureImporter;

				importer.textureType = TextureImporterType.Sprite;
				importer.textureFormat = (transparent_color >= 0 ? TextureImporterFormat.ARGB32 : TextureImporterFormat.AutomaticTruecolor);
				importer.spriteImportMode = SpriteImportMode.Multiple;
				importer.filterMode = FilterMode.Point;
				importer.spritePivot = Vector2.zero;
				importer.spritePixelsToUnits = 1.0f;
				importer.isReadable = (transparent_color >= 0);

				AssetDatabase.ImportAsset(dest_path);

				Texture2D atlas = AssetDatabase.LoadAssetAtPath(dest_path, typeof(Texture2D)) as Texture2D;

				int tiles_x = (atlas.width - 2 * margin_x + spacing_x) / (slice_size_x + spacing_x);
				int tiles_y = (atlas.height - 2 * margin_y + spacing_y) / (slice_size_y + spacing_y);
				int tile_count = tiles_x * tiles_y;
				int nudge_x = atlas.width + spacing_x - 2 * margin_x - tiles_x * (slice_size_x + spacing_x);
				int nudge_y = atlas.height + spacing_y - 2 * margin_y - tiles_y * (slice_size_y + spacing_y);

				TileSet tile_set = Utility.Asset.LoadAndEdit<TileSet>(tile_set_dest);
				tile_set.slice_size_x = slice_size_x;
				tile_set.slice_size_y = slice_size_y;
				tile_set.offset_x = offset_x;
				tile_set.offset_y = offset_y;
				tile_set.tiles = new Sprite[tile_count];
				tile_set.name = name;
				tile_set.animations = animations;
				tile_set.properties = properties;

				bool reimport_required = false;
				importer = AssetImporter.GetAtPath(dest_path) as TextureImporter;

				importer.textureType = TextureImporterType.Sprite;
				importer.textureFormat = (transparent_color >= 0 ? TextureImporterFormat.ARGB32 : TextureImporterFormat.AutomaticTruecolor);
				importer.spriteImportMode = SpriteImportMode.Multiple;
				importer.filterMode = FilterMode.Point;
				importer.spritePivot = Vector2.zero;
				importer.spritePixelsToUnits = 1.0f;
				importer.isReadable = (transparent_color >= 0);

				bool[] skip_tiles = new bool[tile_count];
				int add_tiles = tile_count;

				int add_index = 0;

				if (importer.spritesheet != null)
				{
					add_index = importer.spritesheet.Length;

					foreach (SpriteMetaData meta in importer.spritesheet)
					{
						int size_x = 0;
						int size_y = 0;
						int index = 0;

						if (!ParseTileSpriteAssetName(meta.name, out size_x, out size_y, out index))
							continue;

						if (size_x != slice_size_x || size_y != slice_size_y)
							continue;

						skip_tiles[index] = true;
						--add_tiles;
					}
				}

				if (add_tiles > 0)
				{
					SpriteMetaData[] new_spritesheet = new SpriteMetaData[add_index + add_tiles];
					System.Array.Copy(importer.spritesheet, new_spritesheet, add_index);

					for (int i = 0; i < tile_count; ++i)
					{
						if (skip_tiles[i])
							continue;

						int x = i % tiles_x;
						int y = tiles_y - i / tiles_x - 1;

						SpriteMetaData new_meta = new SpriteMetaData();

						new_meta.alignment = (int)SpriteAlignment.Center;
						new_meta.name = MakeTileSpriteAssetName(slice_size_x, slice_size_y, i);
						new_meta.pivot = Vector2.zero;
						new_meta.rect =
							new Rect(
								margin_x + x * (slice_size_x + spacing_x) + nudge_x,
								margin_y + y * (slice_size_y + spacing_y) + nudge_y,
								slice_size_x,
								slice_size_y
							);

						new_spritesheet[add_index++] = new_meta;
					}

					importer.spritesheet = new_spritesheet;

					reimport_required = true;
				}

				if (reimport_required)
					AssetDatabase.ImportAsset(dest_path);

				if (transparent_color >= 0)
				{
					int r = transparent_color >> 16;
					int g = (transparent_color >> 8) & 0xFF;
					int b = transparent_color & 0xFF;

					Color32[] pixels = atlas.GetPixels32();

					for (int i = 0; i < pixels.Length; ++i)
					{
						if (pixels[i].r == r && pixels[i].g == g && pixels[i].b == b)
							pixels[i].a = 0;
					}

					atlas.SetPixels32(pixels);
					atlas.Apply();

					File.WriteAllBytes(dest_path, atlas.EncodeToPNG());

					importer.isReadable = false;
					AssetDatabase.ImportAsset(dest_path);
				}

				Object[] assets = AssetDatabase.LoadAllAssetRepresentationsAtPath(dest_path);

				foreach (Object asset in assets)
				{
					int size_x = 0;
					int size_y = 0;
					int index = 0;

					if (!ParseTileSpriteAssetName(asset.name, out size_x, out size_y, out index))
						continue;

					if (size_x != slice_size_x || size_y != slice_size_y)
						continue;

					tile_set.tiles[index] = asset as Sprite;
				}

				return tile_set;
			}

			// Creates and saves the chunks of a tile map to its resource directory
			public static void BuildChunks(
				TileMap tile_map,
				List<TileChunkDataLayer> layers,
				int chunk_size_x,
				int chunk_size_y,
				string resources_dir,
				string progress_bar_title = null
				)
			{
				string chunks_dir = resources_dir + "/" + "chunks";

				if (Directory.Exists(chunks_dir))
					Directory.Delete(chunks_dir, true);

				Directory.CreateDirectory(chunks_dir);

				int count = 0;
				for (int chunk_y = 0; chunk_y < tile_map.chunks_y; ++chunk_y)
				{
					int index_y = chunk_y + tile_map.chunk_bottom;
					for (int chunk_x = 0; chunk_x < tile_map.chunks_x; ++chunk_x)
					{
						int index_x = chunk_x + tile_map.chunk_left;

						string chunk_dest = chunks_dir + "/" + index_x + "_" + index_y + ".asset";

						if (progress_bar_title != null)
							EditorUtility.DisplayProgressBar(progress_bar_title, "Building chunk " + (count + 1) + " / " + tile_map.chunks_total + ": " + index_x + "_" + index_y, (float)count / (float)(tile_map.chunks_total - 1));

						TileChunk chunk = Utility.Asset.LoadAndEdit<TileChunk>(chunk_dest);

						chunk.index_x = index_x;
						chunk.index_y = index_y;
						chunk.pos_x = Mathf.Max(index_x * chunk_size_x, tile_map.left);
						chunk.pos_y = Mathf.Max(index_y * chunk_size_y, tile_map.bottom);
						chunk.size_x = Mathf.Min((index_x + 1) * chunk_size_x, tile_map.right + 1) - chunk.pos_x;
						chunk.size_y = Mathf.Min((index_y + 1) * chunk_size_y, tile_map.top + 1) - chunk.pos_y;
						chunk.data_layers = new List<TileChunkDataLayer>();
						chunk.user_data = new List<ScriptableObject>();

						foreach (TileChunkDataLayer data_layer in layers)
						{
							TileChunkDataLayer chunk_layer = new TileChunkDataLayer(chunk.size_x, chunk.size_y);

							for (int y = 0; y < chunk.size_y; ++y)
								for (int x = 0; x < chunk.size_x; ++x)
									chunk_layer.ids[y * chunk.size_x + x] = data_layer.ids[(chunk.pos_y - tile_map.bottom + y) * tile_map.size_x + chunk.pos_x - tile_map.left + x];

							chunk.data_layers.Add(chunk_layer);
						}

						++count;
					}
				}

				EditorUtility.ClearProgressBar();
				AssetDatabase.Refresh();
			}
		}
	}
}
