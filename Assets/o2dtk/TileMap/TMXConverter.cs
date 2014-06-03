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
				tile_map.resources_dir = Converter.GetRelativeResourcesPath(settings.resources_dir);

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

								TileSet tile_set = Converter.MakeTileSet(image_path, slice_size_x, slice_size_y, settings.tile_sets_dir);

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
					Converter.BuildChunks(tile_map, data_layers, settings.chunk_size_x, settings.chunk_size_y, settings.resources_dir);

				string output_path = settings.output_dir + "/" + settings.output_name + ".asset";

				if (File.Exists(output_path))
					AssetDatabase.DeleteAsset(output_path);

				AssetDatabase.CreateAsset(tile_map, output_path);
			}
		}
	}
}
