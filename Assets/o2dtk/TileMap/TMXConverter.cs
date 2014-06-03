using UnityEngine;
using UnityEditor;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Xml;

namespace o2dtk
{
	namespace TileMap
	{
		public class TMXImportSettings
		{
			// The path to the input TMX file
			public string input_path;
			// The directory where the map should be saved when done
			public string output_dir;
			// The name of the tile map when it is saved
			public string output_name;
			// The directory where the tile sets should be put
			//   This should be a project folder like Assets/TileSets
			public string tile_sets_dir;
			// The directory where the tile map chunks and other dynamic information should be put
			//   This should be (Assets/Resources/)TileMapName/
			public string resources_dir;

			// Whether the chunks of the tile set should be rebuilt
			public bool rebuild_chunks;
			// The size of each chunk
			public int chunk_size_x;
			public int chunk_size_y;

			// Default constructor
			public TMXImportSettings()
			{
				input_path = "";
				output_dir = "";
				output_name = "";
				tile_sets_dir = "";
				resources_dir = "";

				rebuild_chunks = false;
				chunk_size_x = 0;
				chunk_size_y = 0;
			}
		}

		public class TMXImporter
		{
			// Makes a sliced tile sprite asset name from its coordinates
			public static string MakeTileSpriteAssetName(int size_x, int size_y, int index)
			{
				return "tile_" + size_x + "x" + size_y + "_" + index;
			}

			// Parses a sliced tile sprite asset name into its coordinates or fails
			public static bool ParseTileSpriteAssetName(string asset_name, out int size_x, out int size_y, out int index)
			{
				char[] delims = {'x', '_'};
				string[] coords = asset_name.Split(delims);

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
			public static TileSet MakeTileSet(string source_path, int slice_size_x, int slice_size_y, TMXImportSettings settings)
			{
				Texture2D atlas = AssetDatabase.LoadAssetAtPath(source_path, typeof(Texture2D)) as Texture2D;

				int tiles_x = atlas.width / slice_size_x;
				int tiles_y = atlas.height / slice_size_y;
				int tile_count = tiles_x * tiles_y;

				string name = Path.GetFileNameWithoutExtension(source_path) + "_" + slice_size_x + "x" + slice_size_y;

				TileSet tile_set = ScriptableObject.CreateInstance<TileSet>();
				tile_set.slice_size_x = slice_size_x;
				tile_set.slice_size_y = slice_size_y;
				tile_set.offset_x = 0;
				tile_set.offset_y = 0;
				tile_set.tiles = new Sprite[tile_count];
				tile_set.name = name;

				int nudge_y = atlas.height - tiles_y * slice_size_y;

				string dest_path = settings.tile_sets_dir + "/" + Path.GetFileName(source_path);

				TextureImporter importer = null;
				bool reimport_required = false;

				if (!File.Exists(dest_path))
					AssetDatabase.CopyAsset(source_path, dest_path);

				AssetDatabase.ImportAsset(dest_path);

				importer = AssetImporter.GetAtPath(dest_path) as TextureImporter;

				importer.textureType = TextureImporterType.Sprite;
				importer.textureFormat = TextureImporterFormat.AutomaticTruecolor;
				importer.spriteImportMode = SpriteImportMode.Multiple;
				importer.filterMode = FilterMode.Point;
				importer.spritePivot = Vector2.zero;
				importer.spritePixelsToUnits = 1.0f;

				reimport_required = true;

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

						new_meta.alignment = (int)SpriteAlignment.BottomLeft;
						new_meta.name = MakeTileSpriteAssetName(slice_size_x, slice_size_y, i);
						new_meta.pivot = Vector2.zero;
						new_meta.rect = new Rect(x * slice_size_x, y * slice_size_y + nudge_y, slice_size_x, slice_size_y);

						new_spritesheet[add_index++] = new_meta;
					}

					importer.spritesheet = new_spritesheet;

					reimport_required = true;
				}

				if (reimport_required)
					AssetDatabase.ImportAsset(dest_path);

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

				string tile_set_dest = settings.tile_sets_dir + "/" + name + ".asset";

				if (File.Exists(tile_set_dest))
					AssetDatabase.DeleteAsset(tile_set_dest);

				AssetDatabase.CreateAsset(tile_set, tile_set_dest);

				return tile_set;
			}

			// Creates and saves the chunks of a tile map to its resource directory
			public static void BuildChunks(TileMap tile_map, List<TileChunkDataLayer> layers, TMXImportSettings settings)
			{
				string chunks_dir = settings.resources_dir + "/" + "chunks";

				if (!Directory.Exists(chunks_dir))
					Directory.CreateDirectory(chunks_dir);

				int pos_x = 0;
				int pos_y = 0;

				while (pos_y + settings.chunk_size_y <= tile_map.size_y)
				{
					while (pos_x + settings.chunk_size_x <= tile_map.size_x)
					{
						TileChunk chunk = ScriptableObject.CreateInstance<TileChunk>();

						chunk.pos_x = pos_x;
						chunk.pos_y = pos_y;
						chunk.size_x = Mathf.Min(settings.chunk_size_x, tile_map.size_x - pos_x);
						chunk.size_y = Mathf.Min(settings.chunk_size_y, tile_map.size_y - pos_y);
						chunk.data_layers = new List<TileChunkDataLayer>();

						foreach (TileChunkDataLayer data_layer in layers)
						{
							TileChunkDataLayer chunk_layer = new TileChunkDataLayer(chunk.size_x, chunk.size_y);

							for (int y = 0; y < chunk.size_y; ++y)
								for (int x = 0; x < chunk.size_x; ++x)
									chunk_layer.ids[y * chunk.size_x + x] = data_layer.ids[(chunk.pos_y + y) * tile_map.size_x + chunk.pos_x + x];

							chunk.data_layers.Add(chunk_layer);
						}

						string chunk_dest = chunks_dir + "/" + (pos_x / settings.chunk_size_x) + "_" + (pos_y / settings.chunk_size_y) + ".asset";

						if (File.Exists(chunk_dest))
							AssetDatabase.DeleteAsset(chunk_dest);

						AssetDatabase.CreateAsset(chunk, chunk_dest);

						pos_x += settings.chunk_size_x;
					}

					pos_x = 0;
					pos_y += settings.chunk_size_y;
				}
			}

			// Imports a TMX file into a format compatible with the 2D toolkit
			public static void ImportTMX(TMXImportSettings settings)
			{
				string input_dir = Path.GetDirectoryName(settings.input_path);

				TileMap tile_map = ScriptableObject.CreateInstance<TileMap>();

				tile_map.size_x = 0;
				tile_map.size_y = 0;
				tile_map.chunk_size_x = 0;
				tile_map.chunk_size_y = 0;
				tile_map.library = new TileLibrary();
				tile_map.layer_info = new List<TileMapLayerInfo>();
				tile_map.resources_dir = settings.resources_dir.Remove(0, 17).Replace('\\', '/');

				List<TileChunkDataLayer> data_layers = new List<TileChunkDataLayer>();

				XmlReader reader = XmlReader.Create(settings.input_path);

				while (reader.Read())
				{
					if (reader.NodeType == XmlNodeType.Element)
					{
						switch (reader.Name)
						{
							case "map":
								tile_map.size_x = int.Parse(reader.GetAttribute("width"));
								tile_map.size_y = int.Parse(reader.GetAttribute("height"));

								int tile_size_x = int.Parse(reader.GetAttribute("tilewidth"));
								int tile_size_y = int.Parse(reader.GetAttribute("tileheight"));

								switch (reader.GetAttribute("orientation"))
								{
									case "orthogonal":
										tile_map.major_delta_x = tile_size_x;
										tile_map.major_delta_y = 0;
										tile_map.minor_delta_x = 0;
										tile_map.minor_delta_y = tile_size_y;
										tile_map.odd_delta_x = 0;
										tile_map.odd_delta_y = 0;
										break;
									case "isometric":
										tile_map.major_delta_x = tile_size_x / 2;
										tile_map.major_delta_y = -tile_size_y / 2;
										tile_map.minor_delta_x = tile_size_x / 2;
										tile_map.minor_delta_y = tile_size_y / 2;
										tile_map.odd_delta_x = 0;
										tile_map.odd_delta_y = 0;
										break;
									case "staggered":
										tile_map.major_delta_x = tile_size_x;
										tile_map.major_delta_y = 0;
										tile_map.minor_delta_x = 0;
										tile_map.minor_delta_y = tile_size_y / 2;
										tile_map.odd_delta_x = tile_size_x / 2;
										tile_map.odd_delta_y = 0;
										break;
									default:
										return;
								}

								break;
							case "tileset":
								int slice_size_x = int.Parse(reader.GetAttribute("tilewidth"));
								int slice_size_y = int.Parse(reader.GetAttribute("tileheight"));
								int first_id = int.Parse(reader.GetAttribute("firstgid"));

								string image_path = "";

								while (reader.Read())
								{
									if (reader.NodeType == XmlNodeType.EndElement && reader.Name == "tileset")
										break;

									if (reader.NodeType == XmlNodeType.Element && reader.Name == "image")
										image_path = reader.GetAttribute("source");
								}

								image_path = input_dir + "/" + image_path;

								TileSet tile_set = MakeTileSet(image_path, slice_size_x, slice_size_y, settings);

								tile_map.library.AddTileSet(tile_set, first_id);

								break;
							case "layer":
								TileMapLayerInfo layer_info = new TileMapLayerInfo();

								int size_x = int.Parse(reader.GetAttribute("width"));
								int size_y = int.Parse(reader.GetAttribute("height"));

								layer_info.name = reader.GetAttribute("name");
								layer_info.size_x = size_x;
								layer_info.size_y = size_y;

								tile_map.size_x = size_x;
								tile_map.size_y = size_y;

								if (settings.rebuild_chunks)
								{
									TileChunkDataLayer data_layer = new TileChunkDataLayer(size_x, size_y);

									while (reader.Read())
									{
										if (reader.NodeType == XmlNodeType.EndElement && reader.Name == "layer")
											break;

										if (reader.NodeType == XmlNodeType.Element && reader.Name == "data")
										{
											if (reader.GetAttribute("encoding") == "base64")
											{
												int length = size_x * size_y;
												byte[] buffer = new byte[length * 4];
												reader.ReadElementContentAsBase64(buffer, 0, length * 4);

												for (int i = 0; i < length; ++i)
												{
													int x = i % size_x;
													int y = size_y - i / size_x - 1;
													data_layer.ids[y * size_x + x] =
														buffer[4 * i] |
														(buffer[4 * i + 1] << 8) |
														(buffer[4 * i + 2] << 16) |
														(buffer[4 * i + 3] << 24);
												}
											}
											else
											{
												int index = 0;

												while (reader.Read())
												{
													if (reader.NodeType == XmlNodeType.EndElement && reader.Name == "data")
														break;

													if (reader.NodeType == XmlNodeType.Element && reader.Name == "tile")
													{
														int x = index % size_x;
														int y = size_y - index / size_x - 1;
														data_layer.ids[y * size_x + x] = int.Parse(reader.GetAttribute("gid"));
														++index;
													}
												}
											}
										}
									}

									data_layers.Add(data_layer);
								}

								tile_map.layer_info.Add(layer_info);

								break;
							default:
								break;
						}
					}
				}

				if (settings.chunk_size_x == 0)
					settings.chunk_size_x = tile_map.size_x;
				if (settings.chunk_size_y == 0)
					settings.chunk_size_y = tile_map.size_y;
				
				tile_map.chunk_size_x = settings.chunk_size_x;
				tile_map.chunk_size_y = settings.chunk_size_y;

				if (settings.rebuild_chunks)
					BuildChunks(tile_map, data_layers, settings);

				string output_path = settings.output_dir + "/" + settings.output_name + ".asset";

				if (File.Exists(output_path))
					AssetDatabase.DeleteAsset(output_path);

				AssetDatabase.CreateAsset(tile_map, output_path);
			}
		}
	}
}
