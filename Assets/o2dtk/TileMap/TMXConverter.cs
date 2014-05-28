using UnityEngine;
using UnityEditor;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Xml;

namespace o2dtk
{
	public class TMXImportSettings
	{
		// Whether the tile sets should be rebuilt (tiles sliced if they don't exist)
		public bool rebuild_tilesets;
		// Whether the tile sets should be rebuilt regardless of whether tiles exist
		public bool force_rebuild_tilesets;
		// Whether the chunks should be rebuilt
		public bool rebuild_chunks;
		// The width of each chunk
		public uint chunk_width;
		// The height of each chunk
		public uint chunk_height;

		// Default constructor
		public TMXImportSettings()
		{
			rebuild_tilesets = false;
			force_rebuild_tilesets = false;
			rebuild_chunks = false;
			chunk_width = 0;
			chunk_height = 0;
		}
	}

	public class TMXTileSetInfo
	{
		// The name of the tile set
		public string name;
		// The slice width of the tile set
		public uint slice_width;
		// The slice height of the tile set
		public uint slice_height;
		// The X offset of the tiles in the tile set
		public uint offset_x;
		// The Y offset of the tiles in the tile set
		public uint offset_y;
		// The first GID of the tile set
		public uint first_gid;
		// The number of tiles in the tile set
		public uint num_tiles;

		// Default constructor
		public TMXTileSetInfo()
		{
			name = "undefined";
			slice_width = 0;
			slice_height = 0;
			offset_x = 0;
			offset_y = 0;
			first_gid = 0;
			num_tiles = 0;
		}
	}

	public class TMXLayerInfo
	{
		// The name of the layer
		public string name;
		// The width of the layer in tiles
		public uint layer_width;
		// The height of the layer in tiles
		public uint layer_height;
		// The GIDs of the layer
		public uint[,] gids;

		// Default constructor
		public TMXLayerInfo()
		{
			name = "undefined";
			layer_width = 0;
			layer_height = 0;
			gids = null;
		}
	}

	public class TMXConverter
	{
		// Takes an image at a path and determines how many tiles of a given size are in it
		public static uint CountTilesInImage(string image_path, uint slice_width, uint slice_height)
		{
			Texture2D image = AssetDatabase.LoadAssetAtPath(image_path, typeof(Texture2D)) as Texture2D;
			return (uint)((image.width / slice_width) * (image.height / slice_height));
		}
		
		// Takes an image at a path and slices it into a set of tiles
		public static void SliceTileSet(string image_path, uint slice_width, uint slice_height, string tileset_dir, bool force)
		{
			string progress_bar_title = "Loading '" + Path.GetFileName(image_path) + "'";
			EditorUtility.DisplayProgressBar(progress_bar_title, "Preparing tile atlas", 0.0f);
			
			if (!Directory.Exists(tileset_dir))
				Directory.CreateDirectory(tileset_dir);

			// Prepare the image by allowing reading
			TextureImporter image_imp = AssetImporter.GetAtPath(image_path) as TextureImporter;

			image_imp.textureType = TextureImporterType.Advanced;
			image_imp.isReadable = true;

			AssetDatabase.ImportAsset(image_path);

			Texture2D image = AssetDatabase.LoadAssetAtPath(image_path, typeof(Texture2D)) as Texture2D;
			Color32[] image_pixels = image.GetPixels32();
			Texture2D cur_tile = new Texture2D((int)slice_width, (int)slice_height);
			Color32[] pixels = new Color32[slice_width * slice_height];

			uint total_tiles = (uint)((image.width / slice_width) * (image.height / slice_height));
			uint cur_x = 0;
			uint cur_y = 0;
			uint index = 0;

			while (cur_y + slice_height <= image.height)
			{
				while (cur_x + slice_width <= image.width)
				{
					EditorUtility.DisplayProgressBar(progress_bar_title, "Building tile " + (index + 1) + " / " + total_tiles, (float)index / total_tiles);
					string tile_tex_path = Path.Combine(tileset_dir, "tile_" + index + ".png");
					string tile_mat_path = Path.Combine(tileset_dir, "tile_" + index + ".mat");

					if (File.Exists(tile_tex_path))
					{
						if (force)
							File.Delete(tile_tex_path);
						else
						{
							cur_x += slice_width;
							continue;
						}
					}

					for (uint i = 0; i < slice_height; ++i)
						System.Array.Copy(image_pixels, (image.height - i - cur_y - 1) * image.width + cur_x, pixels, (slice_height - i - 1) * slice_width, slice_width);

					cur_tile.SetPixels32(pixels);

					byte[] bytes = cur_tile.EncodeToPNG();
					FileStream tex_fs = File.OpenWrite(tile_tex_path);
					BinaryWriter bw = new BinaryWriter(tex_fs);
					bw.Write(bytes);
					tex_fs.Close();

					AssetDatabase.ImportAsset(tile_tex_path);

					Material mat = new Material(Shader.Find("Transparent/Diffuse"));
					mat.mainTexture = AssetDatabase.LoadAssetAtPath(tile_tex_path, typeof(Texture2D)) as Texture2D;
					AssetDatabase.CreateAsset(mat, tile_mat_path);

					++index;
					
					cur_x += slice_width;
				}
				
				cur_x = 0;
				cur_y += slice_height;
			}

			EditorUtility.DisplayProgressBar(progress_bar_title, "Cleaning up", 1.0f);

			Texture2D.DestroyImmediate(cur_tile);
			AssetDatabase.SaveAssets();

			EditorUtility.ClearProgressBar();
		}

		// Takes a list of layers and builds the chunks out of them
		public static void WriteChunks(List<TMXLayerInfo> layers, uint map_width, uint map_height, uint width, uint height, string chunks_dir)
		{
			string progress_bar_title = "Building chunks";
			EditorUtility.DisplayProgressBar(progress_bar_title, "Preparing layers", 0.0f);
			
			if (!Directory.Exists(chunks_dir))
				Directory.CreateDirectory(chunks_dir);

			uint chunks_x = (map_width + width - 1) / width;
			uint chunks_y = (map_height + height - 1) / height;
			uint total_chunks = chunks_x * chunks_y;
			uint cur_x = 0;
			uint cur_y = 0;
			uint index = 0;

			while (cur_y < map_height)
			{
				while (cur_x < map_width)
				{
					EditorUtility.DisplayProgressBar(progress_bar_title, "Building chunk " + (index % width) + "_" + (index / width) + " (" + (index + 1) + " / " + total_chunks, (float)index / total_chunks);
					string chunk_path = Path.Combine(chunks_dir, (index % chunks_x) + "_" + (index / chunks_x) + ".chunk");

					FileStream chunk_stream = File.OpenWrite(chunk_path);
					BinaryWriter output = new BinaryWriter(chunk_stream);

					uint data_width = (width < map_width - cur_x ? width : map_width - cur_x);
					uint data_height = (height < map_height - cur_y ? height : map_height - cur_y);

					output.Write(TileChunk.magic_number);
					output.Write(cur_x);
					output.Write(cur_y);
					output.Write(data_width);
					output.Write(data_height);
					output.Write((uint)layers.Count);
					// TODO replace with number of object layers
					output.Write(0);

					// TODO optimize chunk rendering

					output.Write(TileChunk.header_size);
					output.Write((uint)(TileChunk.header_size + layers.Count * data_width * data_height * 4));
					output.Write((uint)(TileChunk.header_size + layers.Count * data_width * data_height * 4 + layers.Count * 4));

					for (int i = 0; i < layers.Count; ++i)
						for (uint y = 0; y < data_height; ++y)
							for (uint x = 0; x < data_width; ++x)
								output.Write(layers[i].gids[cur_x + x, cur_y + y]);

					// TODO use index_size for the quad sizing
					for (uint i = 0; i < layers.Count; ++i)
						output.Write(0);

					// TODO replace with object layers

					++index;

					cur_x += width;
				}

				cur_x = 0;
				cur_y += height;
			}

			EditorUtility.ClearProgressBar();
		}

		// Loads a TMX and converts its representation into a format
		//   compatible with the Open 2D Toolkit
		public static void LoadTMX(string tmx_path, TMXImportSettings settings)
		{
			string tmx_dir = Path.GetDirectoryName(tmx_path);
			string tilemap_name = Path.GetFileNameWithoutExtension(tmx_path);
			string tilemap_dir = Path.Combine(tmx_dir, tilemap_name);
			string tilemap_path = Path.Combine(tilemap_dir, tilemap_name + ".tilemap");
			string tilesets_dir = Path.Combine(tilemap_dir, "tilesets");
			string chunks_dir = Path.Combine(tilemap_dir, "chunks");

			XmlReader reader = XmlReader.Create(tmx_path);

			// Create a directory for the tile map
			Directory.CreateDirectory(tilemap_dir);

			uint map_width = 0;
			uint map_height = 0;
			uint map_tile_width = 0;
			uint map_tile_height = 0;
			uint map_chunk_width = settings.chunk_width;
			uint map_chunk_height = settings.chunk_height;

			List<TMXTileSetInfo> tilesets = new List<TMXTileSetInfo>();
			List<TMXLayerInfo> layers = new List<TMXLayerInfo>();

			while (reader.Read())
			{
				if (reader.NodeType == XmlNodeType.Element)
				{
					switch (reader.Name)
					{
						case "map":
							if (reader.GetAttribute("orientation") != "orthogonal")
							{
								Debug.LogError("Attempted to load a TMX file that was not oriented orthogonally.");
								return;
							}

							map_width = uint.Parse(reader.GetAttribute("width"));
							map_height = uint.Parse(reader.GetAttribute("height"));
							map_tile_width = uint.Parse(reader.GetAttribute("tilewidth"));
							map_tile_height = uint.Parse(reader.GetAttribute("tileheight"));

							break;
						case "tileset":
							TMXTileSetInfo tsi = new TMXTileSetInfo();

							tsi.slice_width = uint.Parse(reader.GetAttribute("tilewidth"));
							tsi.slice_height = uint.Parse(reader.GetAttribute("tileheight"));
							tsi.name = reader.GetAttribute("name") + "_" + tsi.slice_width + "x" + tsi.slice_height;
							tsi.first_gid = uint.Parse(reader.GetAttribute("firstgid"));

							string image_path = "";

							while (reader.Read())
							{
								if (reader.NodeType == XmlNodeType.EndElement && reader.Name == "tileset")
									break;

								if (reader.NodeType == XmlNodeType.Element && reader.Name == "image")
									image_path = reader.GetAttribute("source");
							}

							image_path = Path.Combine(tmx_dir, image_path);

							tsi.num_tiles = CountTilesInImage(image_path, tsi.slice_width, tsi.slice_height);

							if (settings.rebuild_tilesets)
								SliceTileSet(image_path, tsi.slice_width, tsi.slice_height, Path.Combine(tilesets_dir, tsi.name), settings.force_rebuild_tilesets);

							tilesets.Add(tsi);

							break;
						case "layer":
							TMXLayerInfo tli = new TMXLayerInfo();

							tli.name = reader.GetAttribute("name");
							uint width = uint.Parse(reader.GetAttribute("width"));
							uint height = uint.Parse(reader.GetAttribute("height"));
							tli.layer_width = width;
							tli.layer_height = height;

							if (!settings.rebuild_chunks)
							{
								layers.Add(tli);
								break;
							}

							tli.gids = new uint[width, height];

							if (map_width == 0)
								map_width = width;
							if (map_height == 0)
								map_height = height;

							while (reader.Read())
							{
								if (reader.NodeType == XmlNodeType.EndElement && reader.Name == "layer")
									break;

								if (reader.NodeType == XmlNodeType.Element && reader.Name == "data")
								{
									if (reader.GetAttribute("encoding") == "base64")
									{
										uint length = width * height;
										byte[] buffer = new byte[length * 4];
										reader.ReadElementContentAsBase64(buffer, 0, (int)(length * 4));

										for (uint i = 0; i < length; ++i)
											tli.gids[i % width, height - i / width - 1] =
												(uint)(
													buffer[4 * i] |
													(buffer[4 * i + 1] << 8) |
													(buffer[4 * i + 2] << 16) |
													(buffer[4 * i + 3] << 24)
												);
									}
									else
									{
										uint index = 0;

										while (reader.Read())
										{
											if (reader.NodeType == XmlNodeType.EndElement && reader.Name == "data")
												break;

											if (reader.NodeType == XmlNodeType.Element && reader.Name == "tile")
											{
												tli.gids[index % width, height - index / width - 1] = uint.Parse(reader.GetAttribute("gid"));
												++index;
											}
										}
									}
								}
							}

							layers.Add(tli);

							break;
						default:
							break;
					}
				}
			}

			if (map_chunk_width == 0)
				map_chunk_width = map_width;
			if (map_chunk_height == 0)
				map_chunk_height = map_height;

			if (settings.rebuild_chunks)
				WriteChunks(layers, map_width, map_height, map_chunk_width, map_chunk_height, chunks_dir);

			// Write the data to the tilemap file
			FileStream out_stream = File.OpenWrite(tilemap_path);
			BinaryWriter output = new BinaryWriter(out_stream);

			output.Write(TileMap.magic_number);
			output.Write(map_width);
			output.Write(map_height);
			output.Write(map_tile_width);
			output.Write(map_tile_height);
			output.Write(map_chunk_width);
			output.Write(map_chunk_height);

			output.Write(tilesets.Count);

			foreach (TMXTileSetInfo tileset in tilesets)
			{
				output.Write(tileset.name);
				output.Write(tileset.slice_width);
				output.Write(tileset.slice_height);
				output.Write(tileset.offset_x);
				output.Write(tileset.offset_y);
				output.Write(tileset.first_gid);
				output.Write(tileset.num_tiles);
			}

			output.Write(layers.Count);

			foreach (TMXLayerInfo layer in layers)
			{
				output.Write(layer.name);
				output.Write(layer.layer_width);
				output.Write(layer.layer_height);
			}

			// TODO replace this with the number of object layers being written
			output.Write(0);

			out_stream.Close();
		}
	}
}
