using UnityEngine;
using UnityEditor;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
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

			// Whether the tile sets should be rebuilt regardless of their existence
			public bool force_rebuild_tile_sets;
			// Whether the chunks of the tile set should be rebuilt
			public bool rebuild_chunks;
			// The size of each chunk
			public int chunk_size_x;
			public int chunk_size_y;

			// Whether to flip Z precedence along the major axis
			public bool flip_major_precedence;
			// Whether to flip Z precedence along the minor axis
			public bool flip_minor_precedence;

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
				flip_major_precedence = false;
				flip_minor_precedence = false;
			}
		}

		public class TMXImporter
		{
			// Reads the information out of a tileset XML node
			private static void ReadTilesetNode(XmlReader reader, ref int slice_size_x, ref int slice_size_y, ref int spacing, ref int margin, ref int offset_x, ref int offset_y, ref string image_path, ref int transparent_color, ref TileAnimation[] animations)
			{
				slice_size_x = int.Parse(reader.GetAttribute("tilewidth"));
				slice_size_y = int.Parse(reader.GetAttribute("tileheight"));
				string spacing_attr = reader.GetAttribute("spacing");
				if (spacing_attr != null)
					spacing = int.Parse(spacing_attr);
				string margin_attr = reader.GetAttribute("margin");
				if (margin_attr != null)
					margin = int.Parse(margin_attr);

				while (reader.Read())
				{
					if (reader.NodeType == XmlNodeType.EndElement && reader.Name == "tileset")
						break;

					if (reader.NodeType == XmlNodeType.Element)
					{
						switch (reader.Name)
						{
							case "tileoffset":
								offset_x = int.Parse(reader.GetAttribute("x"));
								offset_y = int.Parse(reader.GetAttribute("y"));
								break;
							case "image":
								image_path = reader.GetAttribute("source");
								string transparent_attr = reader.GetAttribute("trans");
								if (transparent_attr != null)
									transparent_color = int.Parse(transparent_attr, System.Globalization.NumberStyles.HexNumber);
								int tiles_x = int.Parse(reader.GetAttribute("width")) / slice_size_x;
								int tiles_y = int.Parse(reader.GetAttribute("height")) / slice_size_y;
								animations = new TileAnimation[tiles_x * tiles_y];
								break;
							case "tile":
								int id = int.Parse(reader.GetAttribute("id"));

								if (!reader.IsEmptyElement)
								{
									while (reader.Read())
									{
										if (reader.NodeType == XmlNodeType.EndElement && reader.Name == "tile")
											break;

										if (reader.NodeType == XmlNodeType.Element && reader.Name == "animation")
										{
											animations[id] = new TileAnimation();

											while (reader.Read())
											{
												if (reader.NodeType == XmlNodeType.EndElement && reader.Name == "animation")
													break;

												if (reader.NodeType == XmlNodeType.Element && reader.Name == "frame")
													animations[id].AddKey(new TileAnimationKey(int.Parse(reader.GetAttribute("tileid")), int.Parse(reader.GetAttribute("duration"))));
											}
										}
									}
								}
								break;
							default:
								break;
						}
					}
				}
			}

			// Imports a TMX file into a format compatible with the 2D toolkit
			public static void ImportTMX(TMXImportSettings settings)
			{
				string progress_bar_title = "Importing " + Path.GetFileName(settings.input_path);
				EditorUtility.DisplayProgressBar(progress_bar_title, "Initializing", 0.0f);

				string input_dir = Path.GetDirectoryName(settings.input_path);
				string output_path = settings.output_dir + "/" + settings.output_name + ".asset";

				TileMap tile_map = Utility.Asset.LoadAndEdit<TileMap>(output_path);

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
								EditorUtility.DisplayProgressBar(progress_bar_title, "Reading map data", 0.0f);

								tile_map.size_x = int.Parse(reader.GetAttribute("width"));
								tile_map.size_y = int.Parse(reader.GetAttribute("height"));

								if (settings.chunk_size_x == 0)
									settings.chunk_size_x = tile_map.size_x;
								if (settings.chunk_size_y == 0)
									settings.chunk_size_y = tile_map.size_y;

								tile_map.chunk_size_x = settings.chunk_size_x;
								tile_map.chunk_size_y = settings.chunk_size_y;

								int tile_size_x = int.Parse(reader.GetAttribute("tilewidth"));
								int tile_size_y = int.Parse(reader.GetAttribute("tileheight"));

								switch (reader.GetAttribute("orientation"))
								{
									case "orthogonal":
										tile_map.major_delta_x = tile_size_x;
										tile_map.major_delta_y = 0;
										tile_map.major_delta_z = (settings.flip_major_precedence ? 1 : -1);
										tile_map.minor_delta_x = 0;
										tile_map.minor_delta_y = tile_size_y;
										tile_map.minor_delta_z = (settings.flip_minor_precedence ? 1 : -1);
										tile_map.odd_delta_x = 0;
										tile_map.odd_delta_y = 0;
										tile_map.odd_delta_z = 0;
										break;
									case "isometric":
										tile_map.major_delta_x = tile_size_x / 2;
										tile_map.major_delta_y = -tile_size_y / 2;
										tile_map.major_delta_z = (settings.flip_major_precedence ? -1 : 1);
										tile_map.minor_delta_x = tile_size_x / 2;
										tile_map.minor_delta_y = tile_size_y / 2;
										tile_map.minor_delta_z = (settings.flip_minor_precedence ? 1 : -1);
										tile_map.odd_delta_x = 0;
										tile_map.odd_delta_y = 0;
										tile_map.odd_delta_z = 0;
										break;
									case "staggered":
										tile_map.major_delta_x = tile_size_x;
										tile_map.major_delta_y = 0;
										tile_map.major_delta_z = 0;
										tile_map.minor_delta_x = 0;
										tile_map.minor_delta_y = tile_size_y / 2;
										tile_map.minor_delta_z = (settings.flip_minor_precedence ? 1 : -1);
										tile_map.odd_delta_x = (tile_map.size_y % 2 == 0 ? -tile_size_x / 2 : tile_size_x / 2);
										tile_map.odd_delta_y = 0;
										tile_map.odd_delta_z = 0;
										break;
									default:
										return;
								}

								break;
							case "tileset":
								int first_id = int.Parse(reader.GetAttribute("firstgid"));
								int slice_size_x = 0;
								int slice_size_y = 0;
								int spacing_x = 0;
								int spacing_y = 0;
								int margin_x = 0;
								int margin_y = 0;
								int offset_x = 0;
								int offset_y = 0;
								int transparent_color = -1;
								string source = reader.GetAttribute("source");
								string image_path = "";
								TileAnimation[] animations = null;

								if (source == null)
								{
									ReadTilesetNode(reader, ref slice_size_x, ref slice_size_y, ref spacing_x, ref margin_x, ref offset_x, ref offset_y, ref image_path, ref transparent_color, ref animations);

									spacing_y = spacing_x;
									margin_y = margin_x;
									image_path = Path.Combine(input_dir, image_path);
								}
								else
								{
									string tsx_path = Path.Combine(input_dir, source);
									string tsx_dir = Path.GetDirectoryName(tsx_path);
									XmlReader tsx_reader = XmlReader.Create(tsx_path);

									while (tsx_reader.Read())
										if (tsx_reader.NodeType == XmlNodeType.Element)
											if (tsx_reader.Name == "tileset")
													ReadTilesetNode(tsx_reader, ref slice_size_x, ref slice_size_y, ref spacing_x, ref margin_x, ref offset_x, ref offset_y, ref image_path, ref transparent_color, ref animations);

									spacing_y = spacing_x;
									margin_y = margin_x;
									image_path = Path.Combine(tsx_dir, image_path);
								}

								EditorUtility.DisplayProgressBar(progress_bar_title, "Importing tile set '" + Path.GetFileName(image_path) + "' with " + spacing_x + " by " + spacing_y + " tiles", 0.0f);

								TileSet tile_set =
									Converter.MakeTileSet(
										image_path,
										margin_x, margin_y,
										spacing_x, spacing_y,
										slice_size_x, slice_size_y,
										offset_x, offset_y,
										transparent_color,
										animations,
										settings.tile_sets_dir,
										settings.force_rebuild_tile_sets
									);

								tile_map.library.AddTileSet(tile_set, first_id);

								break;
							case "layer":
								TileMapLayerInfo layer_info = new TileMapLayerInfo();

								int size_x = int.Parse(reader.GetAttribute("width"));
								int size_y = int.Parse(reader.GetAttribute("height"));

								layer_info.name = reader.GetAttribute("name");
								layer_info.size_x = size_x;
								layer_info.size_y = size_y;
								string opacity_attr = reader.GetAttribute("opacity");
								if (opacity_attr != null)
									layer_info.default_alpha = float.Parse(opacity_attr);
								else
									layer_info.default_alpha = 1.0f;

								tile_map.size_x = size_x;
								tile_map.size_y = size_y;

								EditorUtility.DisplayProgressBar(progress_bar_title, "Reading data for layer '" + layer_info.name + "'", 0.0f);

								if (settings.rebuild_chunks)
								{
									TileChunkDataLayer data_layer = new TileChunkDataLayer(size_x, size_y);

									while (reader.Read())
									{
										if (reader.NodeType == XmlNodeType.EndElement && reader.Name == "layer")
											break;

										if (reader.NodeType == XmlNodeType.Element && reader.Name == "data")
										{
											string encoding = reader.GetAttribute("encoding");
											string compression = reader.GetAttribute("compression");

											if (encoding == "base64")
											{
												int length = size_x * size_y;
												byte[] buffer = new byte[length * 4];
												string base64 = reader.ReadElementContentAsString();
												byte[] input = System.Convert.FromBase64String(base64);

												if (compression == "zlib")
													Utility.Decompress.Zlib(input, buffer, length * 4);
												else if (compression == "gzip")
													Utility.Decompress.Gzip(input, buffer, length * 4);
												else
													buffer = input;

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
											else if (encoding == "csv")
											{
												string[] indices = reader.ReadElementContentAsString().Split(new char[]{','});

												for (int index = 0; index < size_x * size_y; ++index)
												{
													int x = index % size_x;
													int y = size_y - index / size_x - 1;
													data_layer.ids[y * size_x + x] = (int)uint.Parse(indices[index]);
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
														data_layer.ids[y * size_x + x] = (int)uint.Parse(reader.GetAttribute("gid"));
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

				if (settings.rebuild_chunks)
					Converter.BuildChunks(tile_map, data_layers, settings.chunk_size_x, settings.chunk_size_y, settings.resources_dir, progress_bar_title);

				EditorUtility.ClearProgressBar();
			}
		}
	}
}
