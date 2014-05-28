using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.IO;

namespace o2dtk
{
	public class TileChunk
	{
		// The magic number for chunks
		public static uint magic_number = 0x4843415A;

		// The size, in bytes, of a .chunk file header
		public static uint header_size = 40;

		// The different modes for loading data from a .chunk file
		public enum LoadMode
		{
			Data = 1,
			Render,
			DataRender,
			Objects,
			DataObjects,
			RenderObjects,
			DataRenderObjects
		};

		// Basic data about the chunk
		public uint x_pos;
		public uint y_pos;
		public uint width;
		public uint height;

		// The root of the chunk
		// This is positioned at the correct location for the chunk;
		//   locations in the render layer should be local to the chunk.
		public GameObject chunk_root;

		// The tile data layers in the chunk
		// These basically just have GIDs
		public List<TileDataLayer> data_layers;
		// The render data layers in the chunk
		// These basically just have gameobjects (the layers) filled with quads
		//   and the optimized quad representation.
		public List<TileRenderLayer> render_layers;
		// The object layers in the chunk
		// These are not implemented yet!
		public List<TileObjectLayer> object_layers;

		// Default constructor
		public TileChunk()
		{
			x_pos = 0;
			y_pos = 0;
			width = 0;
			height = 0;
			
			chunk_root = null;

			data_layers = new List<TileDataLayer>();
			render_layers = new List<TileRenderLayer>();
			object_layers = new List<TileObjectLayer>();
		}

		public void Unload()
		{
			GameObject.DestroyImmediate(chunk_root);
		}

		// Chooses an indexing type based on the width and height of a chunk
		public static uint GetIndexType(uint chunk_width, uint chunk_height)
		{
			uint tiles = chunk_width * chunk_height;

			if (tiles <= 0xFF)
				return 1;
			if (tiles <= 0xFFFF)
				return 2;
			return 4;
		}

		// Reads a 32-, 16-, or 8-bit unsigned integer from the binary reader depending on the index type
		public static uint ReadSizedUInt(BinaryReader input, uint index_type)
		{
			switch (index_type)
			{
				case 1:
					return input.ReadByte();
				case 2:
					return input.ReadUInt16();
				case 4:
					return input.ReadUInt32();
				default:
					return input.ReadUInt32();
			}
		}

		// Writes a 32-, 16-, or 8-bit unsigned integer to the binary writer depending on the index type
		public static void WriteSizedUInt(uint value, BinaryWriter output, uint index_type)
		{
			switch (index_type)
			{
				case 1:
					output.Write((byte)value);
					break;
				case 2:
					output.Write((byte)(value & 0xFF));
					output.Write((byte)(value >> 8));
					break;
				case 4:
					output.Write((uint)value);
					break;
				default:
					output.Write((uint)value);
					break;
			}
		}
		
		public static TileChunk Load(string path, LoadMode mode, TileMap map, TileChunk chunk = null)
		{
			FileStream chunk_file = null;

			try
			{
				chunk_file = File.OpenRead(path);
			}
			catch
			{
				Debug.LogError("Encountered exception while opening chunk file '" + path + "' for reading.");
				return null;
			}

			BinaryReader input = new BinaryReader(chunk_file);

			uint magic_check = input.ReadUInt32();

			if (magic_check != magic_number)
			{
				Debug.LogError("Magic number of chunk '" + path + "' (0x" + magic_check.ToString("X") + ") does not match correct magic number (0x" + magic_number.ToString("X") + ")");
				return null;
			}

			if (chunk == null)
				chunk = new TileChunk();

			chunk.x_pos = input.ReadUInt32();
			chunk.y_pos = input.ReadUInt32();
			chunk.width = input.ReadUInt32();
			chunk.height = input.ReadUInt32();

			uint index_type = GetIndexType(chunk.width, chunk.height);
			
			uint num_tile_layers = input.ReadUInt32();
			uint num_object_layers = input.ReadUInt32();

			uint data_offset = input.ReadUInt32();
			uint render_offset = input.ReadUInt32();
			uint object_offset = input.ReadUInt32();

			List<TileDataLayer> chunk_data = new List<TileDataLayer>();

			if ((mode & LoadMode.Data) != 0 || (mode & LoadMode.Render) != 0)
			{
				chunk_file.Seek(data_offset, SeekOrigin.Begin);

				for (uint i = 0; i < num_tile_layers; ++i)
					chunk_data.Add(new TileDataLayer(chunk.width, chunk.height, input.ReadBytes((int)(chunk.width * chunk.height * 4))));
			}

			if ((mode & LoadMode.Data) != 0)
				chunk.data_layers = chunk_data;

			if ((mode & LoadMode.Render) != 0)
			{
				if (chunk.chunk_root != null)
					GameObject.DestroyImmediate(chunk.chunk_root);
				chunk.chunk_root = new GameObject(Path.GetFileNameWithoutExtension(path));
				Transform chunk_transform = chunk.chunk_root.GetComponent<Transform>();
				chunk_transform.parent = map.chunks_root.GetComponent<Transform>();
				chunk_transform.localPosition = new Vector3(chunk.x_pos, chunk.y_pos, 0.0f);

				chunk.render_layers.Clear();

				chunk_file.Seek(render_offset, SeekOrigin.Begin);

				BitArray mask = new BitArray((int)(chunk.width * chunk.height));

				for (int i = 0; i < num_tile_layers; ++i)
				{
					TileRenderLayer layer = new TileRenderLayer(map.tile_layers[i].name, chunk.chunk_root);

					chunk_file.Seek(render_offset + 4 * i, SeekOrigin.Begin);
					uint quad_count = input.ReadUInt32();

					uint[] quads = new uint[quad_count * 2];

					// Read in and build the optimized quads
					for (uint q = 0; q < quad_count * 2; ++q)
						quads[q] = ReadSizedUInt(input, index_type);

					for (uint q = 0; q < quad_count * 2;)
					{
						uint quad_ll = quads[q++];
						uint quad_ur = quads[q++];

						uint quad_x = quad_ll % chunk.width;
						uint quad_y = quad_ll / chunk.width;
						uint quad_ux = quad_ur % chunk.width;
						uint quad_uy = quad_ur / chunk.width;

						uint gid = chunk_data[i].gids[quad_x, quad_y];

						if (gid == 0)
							continue;

						layer.AddQuad(quad_x, quad_y, quad_ux, quad_uy, num_tile_layers - i - 1, map.library.GetMaterialByGID(gid));

						for (uint y = quad_y; y < quad_uy; ++y)
							for (uint x = quad_x; x < quad_ux; ++x)
								mask.Set((int)((quad_y + y) * chunk.width + quad_x + x), true);
					}

					// Fill in the unfilled spots with 1x1 quads
					for (uint y = 0; y < chunk.height; ++y)
					{
						for (uint x = 0; x < chunk.width; ++x)
						{
							if (!mask.Get((int)(y * chunk.width + x)))
							{
								uint gid = chunk_data[i].gids[x, y];

								if (gid == 0)
									continue;

								layer.AddQuad(x, y, x, y, num_tile_layers - i - 1, map.library.GetMaterialByGID(gid));
							}
						}
					}

					chunk.render_layers.Add(layer);

					mask.SetAll(false);
				}
			}

			// TODO eventually, add support for loading objects

			return chunk;
		}
	}
}
