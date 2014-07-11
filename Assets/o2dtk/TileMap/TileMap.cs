using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using o2dtk.Collections;

namespace o2dtk
{
	namespace TileMap
	{
		[System.Serializable]
		public class TileMap : ScriptableObject
		{
			// The available tiling patterns maps may have
			public enum Tiling
			{
				Rectangular,
				Isometric,
				Staggered
			};

			// The size of the tile map in tiles
			// A negative value indicates that the tile map is infinitely sized
			public int size_x = 0;
			public int size_y = 0;
			// The tiling pattern of the map
			public Tiling tiling = Tiling.Rectangular;
			public int precedence_scale_x = 0;
			public int precedence_scale_y = 0;
			// The size of the tiles in pixels
			public int tile_size_x = 0;
			public int tile_size_y = 0;
			// The size of each chunk of the map in tiles
			public int chunk_size_x = 0;
			public int chunk_size_y = 0;
			// Any additionally specified properties
			public PropertyMap properties = null;

			// A library of the tile sets used by the tile map
			public TileLibrary library = null;
			// Information about each of the layers in the tile map
			public List<TileMapLayerInfo> layer_info = null;
			// Extra user data that should be loaded when the tile map is loaded
			public List<ScriptableObject> user_data = null;

			// The path to the resources folder used by the tile map
			//   Chunks are located at this/chunks/x_y.asset
			public string resources_dir = "";
			// The generator to use to make chunks
			public TileMapChunkGenerator chunk_generator = null;

			// Returns the total number of tiles in the map
			public int tiles_total
			{
				get
				{
					return size_x * size_y;
				}
			}

			// Returns the number of chunks in the map
			public int chunks_x
			{
				get
				{
					return (size_x + chunk_size_x - 1) / chunk_size_x;
				}
			}
			public int chunks_y
			{
				get
				{
					return (size_y + chunk_size_y - 1) / chunk_size_y;
				}
			}
			public int chunks_total
			{
				get
				{
					return chunks_x * chunks_y;
				}
			}

			// Flips the index of a tile along the Y axis
			public int FlipTileIndex(int index)
			{
				return (size_y - index / size_x - 1) * size_x + index % size_x;
			}

			// Gets the X tiling coordinate of a tile given its X and Y
			// This corresponds to the physical X location of the tile by mutliplying
			// by the X size of the tile and dividing by 2
			public int GetTilingXCoordinate(int x, int y)
			{
				switch (tiling)
				{
					case Tiling.Rectangular:
						return 2 * x;
					case Tiling.Isometric:
						return y + x;
					case Tiling.Staggered:
						return 2 * x + (y % 2 == size_y % 2 ? 1 : 0);
					default:
						Debug.LogWarning("Unsupported tiling on tile map!");
						return 0;
				}
			}

			// Gets the Y tiling coordinate of a tile given its X and Y
			// This corresponds to the physical Y location of the tile by mutliplying
			// by the Y size of the tile and dividing by 2
			public int GetTilingYCoordinate(int x, int y)
			{
				switch (tiling)
				{
					case Tiling.Rectangular:
						return 2 * y;
					case Tiling.Isometric:
						return y - x;
					case Tiling.Staggered:
						return y;
					default:
						Debug.LogWarning("Unsupported tiling on tile map!");
						return 0;
				}
			}

			// Gets the X coordinate of a tile given its X and Y
			public int GetLocalXCoordinate(int x, int y)
			{
				return GetTilingXCoordinate(x, y) * tile_size_x / 2;
			}

			// Gets the Y coordinate of a tile given its X and Y
			public int GetLocalYCoordinate(int x, int y)
			{
				return GetTilingYCoordinate(x, y) * tile_size_y / 2;
			}

			// Gets the precedence of a tile given its X and Y
			public int GetLocalZCoordinate(int x, int y)
			{
				switch (tiling)
				{
					case Tiling.Rectangular:
						return x * precedence_scale_x + y * precedence_scale_y;
					case Tiling.Isometric:
						return x * precedence_scale_x + y * precedence_scale_y;
					case Tiling.Staggered:
						return x * (precedence_scale_x + precedence_scale_y) + (y / 2) * (precedence_scale_y - precedence_scale_x) + (y % 2 == 0 ? 0 : precedence_scale_y);
					default:
						Debug.LogWarning("Unsupported tiling on tile map!");
						return 0;
				}
			}

			// Gets the local position of a tile given its X and Y
			public Vector3 GetLocalCoordinates(int x, int y)
			{
				return new Vector3(GetLocalXCoordinate(x, y), GetLocalYCoordinate(x, y), GetLocalZCoordinate(x, y));
			}

			// Gets the path to a certain chunk of the tile map
			public string GetChunkPath(int pos_x, int pos_y)
			{
				return resources_dir + "/chunks/" + pos_x + "_" + pos_y;
			}

			// Loads a certain chunk of the tile map
			public TileChunk LoadChunk(int pos_x, int pos_y)
			{
				if (chunk_generator != null)
					return chunk_generator.GetChunk(this, pos_x, pos_y);
				else
					return Resources.Load<TileChunk>(GetChunkPath(pos_x, pos_y));
			}
		}
	}
}
