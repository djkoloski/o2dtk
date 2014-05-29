using UnityEngine;
using UnityEditor;
using System.Collections;
using System.Collections.Generic;
using System.IO;

namespace o2dtk
{
	public class TileMap : MonoBehaviour
	{
		// The magic number for tile maps
		public static uint magic_number = 0x5444324F;
		
		// The tile map file containing the tile map data
		public Object tilemap_file;
		// The width of the map in tiles
		public uint width;
		// The height of the map in tiles
		public uint height;
		// The width of each tile in pixels
		public uint tile_width;
		// The height of each tile in pixels
		public uint tile_height;
		// The chunk width
		public uint chunk_width;
		// The chunk height
		public uint chunk_height;

		// The number of chunks horizontally
		public uint chunks_x
		{
			get
			{
				return ((width + chunk_width - 1) / chunk_width);
			}
		}
		// The number of chunks vertically
		public uint chunks_y
		{
			get
			{
				return ((height + chunk_height - 1) / chunk_height);
			}
		}
		// The total number of chunks in the tile map
		public uint chunk_total
		{
			get
			{
				return chunks_x * chunks_y;
			}
		}
		
		// The tile map library
		public TileLibrary library;
		// The chunks currently loaded
		private Dictionary<uint, TileChunk> chunks;
		// The tile layers in the tile map
		public List<TileLayerInfo> tile_layers;
		// The object layers in the tile map
		public List<TileLayerInfo> object_layers;

		// Determines whether the tile map is loaded yet
		public bool loaded
		{
			get
			{
				return (library != null);
			}
		}

		// The game object chunks attach themselves to
		public GameObject chunks_root;

		void Awake()
		{
			Clear();

			LoadFromFile(tilemap_file);
		}

		public void Clear()
		{
			tilemap_file = null;

			width = 0;
			height = 0;
			tile_width = 0;
			tile_height = 0;
			chunk_width = 0;
			chunk_height = 0;

			library = null;
			chunks = null;
			tile_layers = null;
			object_layers = null;

			GameObject.DestroyImmediate(chunks_root);
		}

		public void LoadChunk(uint chunk_x, uint chunk_y)
		{
			if (chunk_x >= chunks_x || chunk_y >= chunks_y)
				return;

			uint chunk_index = chunk_y * chunks_x + chunk_x;

			if (chunks.ContainsKey(chunk_index))
				return;
			
			string file_path = AssetDatabase.GetAssetPath(tilemap_file);
			string file_dir = Path.GetDirectoryName(file_path);
			string chunks_dir = Path.Combine(file_dir, "chunks");

			TileChunk chunk = TileChunk.Load(Path.Combine(chunks_dir, chunk_x + "_" + chunk_y + ".chunk"), TileChunk.LoadMode.DataRenderObjects, this, new TileChunk());

			chunks[chunk_index] = chunk;
		}

		public void UnloadChunk(uint chunk_x, uint chunk_y)
		{
			if (chunk_x >= chunks_x || chunk_y >= chunks_y)
				return;

			uint chunk_index = chunk_y * chunks_x + chunk_x;

			if (chunks.ContainsKey(chunk_index))
			{
				chunks[chunk_index].Unload();
				chunks.Remove(chunk_index);
			}
		}

		public void LoadFromFile(Object map_file_obj)
		{
			Clear();

			string file_path = AssetDatabase.GetAssetPath(map_file_obj);
			string file_dir = Path.GetDirectoryName(file_path);

			FileStream map_file = File.OpenRead(file_path);

			BinaryReader input = new BinaryReader(map_file);

			uint magic_check = input.ReadUInt32();

			if (magic_check != magic_number)
			{
				Debug.LogError("Magic number of tile map '" + file_path + "' (0x" + magic_check.ToString("X") + ") does not match correct magic number (0x" + magic_number.ToString("X") + ")");
				return;
			}

			library = new TileLibrary();
			chunks = new Dictionary<uint, TileChunk>();
			tile_layers = new List<TileLayerInfo>();
			object_layers = new List<TileLayerInfo>();

			chunks_root = new GameObject("chunks");
			chunks_root.GetComponent<Transform>().parent = GetComponent<Transform>();

			tilemap_file = map_file_obj;

			width = input.ReadUInt32();
			height = input.ReadUInt32();
			tile_width = input.ReadUInt32();
			tile_height = input.ReadUInt32();
			chunk_width = input.ReadUInt32();
			chunk_height = input.ReadUInt32();

			uint num_tilesets = input.ReadUInt32();

			string tilesets_dir = Open2D.settings.tilesets_root;

			for (uint i = 0; i < num_tilesets; ++i)
			{
				TileSet tileset = new TileSet();
				
				tileset.name = input.ReadString();
				tileset.slice_width = input.ReadUInt32();
				tileset.slice_height = input.ReadUInt32();
				tileset.offset_x = input.ReadInt32();
				tileset.offset_y = input.ReadInt32();
				tileset.first_gid = input.ReadUInt32();
				uint num_tiles = input.ReadUInt32();

				string path = Path.Combine(tilesets_dir, tileset.name);

				tileset.LoadFromDir(path, num_tiles);

				library.AddTileSet(tileset);
			}

			uint num_layers = input.ReadUInt32();

			for (uint i = 0; i < num_layers; ++i)
				tile_layers.Add(new TileLayerInfo(input.ReadString(), input.ReadUInt32(), input.ReadUInt32()));

			uint num_object_groups = input.ReadUInt32();

			for (uint i = 0; i < num_object_groups; ++i)
				object_layers.Add(new TileLayerInfo(input.ReadString(), input.ReadUInt32(), input.ReadUInt32()));
		}
	}
}
