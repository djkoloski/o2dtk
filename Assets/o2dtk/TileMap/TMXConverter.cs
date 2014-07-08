using UnityEngine;
using UnityEditor;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using System.Xml;
using o2dtk.Collections;

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
			// Whether object layers should be rebuilt
			public bool rebuild_object_layers;
			// The size of each chunk
			public int chunk_size_x;
			public int chunk_size_y;

			// Whether to flip precedence along the X axis
			public bool flip_precedence_x;
			// Whether to flip Z precedence along the Y axis
			public bool flip_precedence_y;

			// The custom importer to use while importing objects
			public TileMapImportDelegate importer;

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
				flip_precedence_x = false;
				flip_precedence_y = false;
			}
		}

		public class TMXImporter
		{
			// Reads the information out of a tileset XML node
			private static void ReadTilesetNode(XmlReader reader, ref int slice_size_x, ref int slice_size_y, ref int spacing, ref int margin, ref int offset_x, ref int offset_y, ref string image_path, ref int transparent_color, ref TileAnimation[] animations, ref PropertyMap[] properties)
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
								properties = new PropertyMap[tiles_x * tiles_y];
								break;
							case "tile":
								int id = int.Parse(reader.GetAttribute("id"));

								if (!reader.IsEmptyElement)
								{
									while (reader.Read())
									{
										if (reader.NodeType == XmlNodeType.EndElement && reader.Name == "tile")
											break;

										if (reader.NodeType == XmlNodeType.Element)
										{
											switch (reader.Name)
											{
												case "animation":
													animations[id] = new TileAnimation();

													while (reader.Read())
													{
														if (reader.NodeType == XmlNodeType.EndElement && reader.Name == "animation")
															break;

														if (reader.NodeType == XmlNodeType.Element && reader.Name == "frame")
															animations[id].AddKey(new TileAnimationKey(int.Parse(reader.GetAttribute("tileid")), int.Parse(reader.GetAttribute("duration"))));
													}

													break;
												case "properties":
													properties[id] = new PropertyMap();

													while (reader.Read())
													{
														if (reader.NodeType == XmlNodeType.EndElement && reader.Name == "properties")
															break;

														if (reader.NodeType == XmlNodeType.Element && reader.Name == "property")
														{
															string name = reader.GetAttribute("name");
															string val = reader.GetAttribute("value");
															if (val == null)
																val = reader.ReadElementContentAsString();
															properties[id][name] = val;
														}
													}

													break;
												default:
													break;
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
				tile_map.properties = new PropertyMap();
				tile_map.library = new TileLibrary();
				tile_map.layer_info = new List<TileMapLayerInfo>();
				tile_map.resources_dir = Converter.GetRelativeResourcesPath(settings.resources_dir);

				List<TileChunkDataLayer> data_layers = new List<TileChunkDataLayer>();
				List<TileMapObject> objects = new List<TileMapObject>();

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

								tile_map.tile_size_x = int.Parse(reader.GetAttribute("tilewidth"));
								tile_map.tile_size_y = int.Parse(reader.GetAttribute("tileheight"));

								tile_map.precedence_scale_x = (settings.flip_precedence_x ? 1 : -1);
								tile_map.precedence_scale_y = (settings.flip_precedence_y ? -1 : 1);

								switch (reader.GetAttribute("orientation"))
								{
									case "orthogonal":
										tile_map.tiling = TileMap.Tiling.Rectangular;
										break;
									case "isometric":
										tile_map.tiling = TileMap.Tiling.Isometric;
										break;
									case "staggered":
										tile_map.tiling = TileMap.Tiling.Staggered;
										break;
									default:
										return;
								}

								break;
							case "properties":
								while (reader.Read())
								{
									if (reader.NodeType == XmlNodeType.EndElement && reader.Name == "properties")
										break;

									if (reader.NodeType == XmlNodeType.Element && reader.Name == "property")
									{
										string name = reader.GetAttribute("name");
										string val = reader.GetAttribute("value");
										if (val == null)
											val = reader.ReadElementContentAsString();
										tile_map.properties[name] = val;
									}
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
								PropertyMap[] properties = null;

								if (source == null)
								{
									ReadTilesetNode(reader, ref slice_size_x, ref slice_size_y, ref spacing_x, ref margin_x, ref offset_x, ref offset_y, ref image_path, ref transparent_color, ref animations, ref properties);

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
													ReadTilesetNode(tsx_reader, ref slice_size_x, ref slice_size_y, ref spacing_x, ref margin_x, ref offset_x, ref offset_y, ref image_path, ref transparent_color, ref animations, ref properties);

									spacing_y = spacing_x;
									margin_y = margin_x;
									image_path = Path.Combine(tsx_dir, image_path);
								}

								EditorUtility.DisplayProgressBar(progress_bar_title, "Importing tile set '" + Path.GetFileName(image_path) + "' with " + slice_size_x + " by " + slice_size_y + " tiles", 0.0f);

								TileSet tile_set =
									Converter.MakeTileSet(
										image_path,
										margin_x, margin_y,
										spacing_x, spacing_y,
										slice_size_x, slice_size_y,
										offset_x, offset_y,
										transparent_color,
										animations,
										properties,
										settings.tile_sets_dir,
										settings.force_rebuild_tile_sets
									);

								tile_map.library.AddTileSet(tile_set, first_id);

								break;
							case "layer":
								TileMapLayerInfo layer_info = new TileMapLayerInfo();

								layer_info.name = reader.GetAttribute("name");
								string opacity_attr = reader.GetAttribute("opacity");
								if (opacity_attr != null)
									layer_info.default_alpha = float.Parse(opacity_attr);
								else
									layer_info.default_alpha = 1.0f;

								EditorUtility.DisplayProgressBar(progress_bar_title, "Reading data for layer '" + layer_info.name + "'", 0.0f);

								TileChunkDataLayer data_layer = new TileChunkDataLayer(tile_map.size_x, tile_map.size_y);

								while (reader.Read())
								{
									if (reader.NodeType == XmlNodeType.EndElement && reader.Name == "layer")
										break;

									if (reader.NodeType == XmlNodeType.Element)
									{
										switch (reader.Name)
										{
											case "data":
												if (!settings.rebuild_chunks)
													break;

												string encoding = reader.GetAttribute("encoding");
												string compression = reader.GetAttribute("compression");

												if (encoding == "base64")
												{
													int bytes_total = 4 * tile_map.tiles_total;
													byte[] buffer = new byte[bytes_total];
													string base64 = reader.ReadElementContentAsString();
													byte[] input = System.Convert.FromBase64String(base64);

													if (compression == "zlib")
														Utility.Decompress.Zlib(input, buffer, bytes_total);
													else if (compression == "gzip")
														Utility.Decompress.Gzip(input, buffer, bytes_total);
													else
														buffer = input;

													for (int i = 0; i < tile_map.tiles_total; ++i)
														data_layer.ids[tile_map.FlipTileIndex(i)] =
															buffer[4 * i] |
															(buffer[4 * i + 1] << 8) |
															(buffer[4 * i + 2] << 16) |
															(buffer[4 * i + 3] << 24);
												}
												else if (encoding == "csv")
												{
													string[] indices = reader.ReadElementContentAsString().Split(new char[]{','});

													for (int index = 0; index < tile_map.tiles_total; ++index)
														data_layer.ids[tile_map.FlipTileIndex(index)] = (int)uint.Parse(indices[index]);
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
															data_layer.ids[tile_map.FlipTileIndex(index)] = (int)uint.Parse(reader.GetAttribute("gid"));
															++index;
														}
													}
												}

												break;
											case "properties":
												layer_info.properties = new PropertyMap();

												while (reader.Read())
												{
													if (reader.NodeType == XmlNodeType.EndElement && reader.Name == "properties")
														break;

													if (reader.NodeType == XmlNodeType.Element && reader.Name == "property")
													{
														string name = reader.GetAttribute("name");
														string val = reader.GetAttribute("value");
														if (val == null)
															val = reader.ReadElementContentAsString();
														layer_info.properties[name] = val;
													}
												}

												break;
											default:
												break;
										}
									}
								}

								data_layers.Add(data_layer);

								tile_map.layer_info.Add(layer_info);

								break;
							case "objectgroup":
								TileMapLayerInfo object_group_info = new TileMapLayerInfo();

								object_group_info.name = reader.GetAttribute("name");

								EditorUtility.DisplayProgressBar(progress_bar_title, "Reading data for object group '" + object_group_info.name + "'", 0.0f);

								while (reader.Read())
								{
									if (reader.NodeType == XmlNodeType.EndElement && reader.Name == "objectgroup")
										break;

									if (reader.NodeType == XmlNodeType.Element)
									{
										switch (reader.Name)
										{
											case "object":
												TileMapObject obj = new TileMapObject();
												obj.name = reader.GetAttribute("name");
												obj.layer_index = tile_map.layer_info.Count;
												obj.position = new Vector2(int.Parse(reader.GetAttribute("x")), int.Parse(reader.GetAttribute("y")));
												string width = reader.GetAttribute("width");
												string height = reader.GetAttribute("height");
												if (width != null && height != null)
												{
													obj.shape.type = TileMapShape.Type.Rectangle;
													obj.shape.points.Add(new Vector2(int.Parse(width), int.Parse(height)));
												}
												obj.properties = new PropertyMap();

												if (!reader.IsEmptyElement)
												{
													while (reader.Read())
													{
														if (reader.NodeType == XmlNodeType.EndElement && reader.Name == "object")
															break;

														if (reader.NodeType == XmlNodeType.Element)
														{
															switch (reader.Name)
															{
																case "ellipse":
																	obj.shape.type = TileMapShape.Type.Ellipse;
																	break;
																case "polyline":
																{
																	obj.shape.type = TileMapShape.Type.Polyline;
																	string[] points = reader.GetAttribute("points").Split(new char[]{' '});

																	foreach (string point in points)
																	{
																		string[] coords = point.Split(new char[]{','});
																		obj.shape.points.Add(new Vector2(int.Parse(coords[0]), int.Parse(coords[1])));
																	}
																	break;
																}
																case "polygon":
																{
																	obj.shape.type = TileMapShape.Type.Polygon;
																	string[] points = reader.GetAttribute("points").Split(new char[]{' '});

																	foreach (string point in points)
																	{
																		string[] coords = point.Split(new char[]{','});
																		obj.shape.points.Add(new Vector2(int.Parse(coords[0]), int.Parse(coords[1])));
																	}
																	break;
																}
																case "properties":
																	while (reader.Read())
																	{
																		if (reader.NodeType == XmlNodeType.EndElement && reader.Name == "properties")
																			break;

																		if (reader.NodeType == XmlNodeType.Element && reader.Name == "property")
																		{
																			string name = reader.GetAttribute("name");
																			string val = reader.GetAttribute("value");
																			if (val == null)
																				val = reader.ReadElementContentAsString();
																			obj.properties[name] = val;
																		}
																	}
																	break;
																default:
																	break;
															}
														}
													}
												}

												objects.Add(obj);

												break;
											case "properties":
												object_group_info.properties = new PropertyMap();

												while (reader.Read())
												{
													if (reader.NodeType == XmlNodeType.EndElement && reader.Name == "properties")
														break;

													if (reader.NodeType == XmlNodeType.Element && reader.Name == "property")
													{
														string name = reader.GetAttribute("name");
														string val = reader.GetAttribute("value");
														if (val == null)
															val = reader.ReadElementContentAsString();
														object_group_info.properties[name] = val;
													}
												}

												break;
											default:
												break;
										}
									}
								}

								tile_map.layer_info.Add(object_group_info);

								break;
							default:
								break;
						}
					}
				}

				if (settings.rebuild_chunks)
					Converter.BuildChunks(tile_map, data_layers, settings.chunk_size_x, settings.chunk_size_y, settings.resources_dir, progress_bar_title);

				if (settings.importer != null)
					foreach (TileMapObject obj in objects)
						settings.importer.ImportTileMapObject(obj, tile_map);

				EditorUtility.ClearProgressBar();
			}
		}
	}
}
