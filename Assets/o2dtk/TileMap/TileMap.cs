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
			public int size_x;
			public int size_y;
			// The tiling pattern of the map
			public Tiling tiling;
			public int precedence_scale_x;
			public int precedence_scale_y;
			// The size of the tiles in pixels
			public int tile_size_x;
			public int tile_size_y;
			// The size of each chunk of the map in tiles
			public int chunk_size_x;
			public int chunk_size_y;
			// Any additionally specified properties
			public PropertyMap properties;

			// A library of the tile sets used by the tile map
			public TileLibrary library;
			// Information about each of the layers in the tile map
			public List<TileMapLayerInfo> layer_info;
			// Extra user data that should be loaded when the tile map is loaded
			public List<ScriptableObject> user_data;

			// The path to the resources folder used by the tile map
			//   Chunks are located at this/chunks/x_y.asset
			public string resources_dir;

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
				int x_max = (precedence_scale_x < 0 ? GetTilingXCoordinate(size_x, 0) * precedence_scale_x : 0);
				int y_max = (precedence_scale_y < 0 ? GetTilingYCoordinate(0, size_y) * precedence_scale_y : 0);
				return GetTilingXCoordinate(x, y) * precedence_scale_x + GetTilingYCoordinate(x, y) * precedence_scale_y - x_max - y_max;
			}

			// Gets the local position of a tile given its X and Y
			public Vector3 GetLocalCoordinates(int x, int y)
			{
				return new Vector3(GetLocalXCoordinate(x, y), GetLocalYCoordinate(x, y), GetLocalZCoordinate(x, y));
			}

			// Gets the index of a certain chunk given its coordinates
			public int GetIndex(int pos_x, int pos_y)
			{
				return pos_y * chunks_x + pos_x;
			}

			// Gets the path to a certain chunk of the tile map
			public string GetChunkPath(int pos_x, int pos_y)
			{
				return resources_dir + "/chunks/" + pos_x + "_" + pos_y;
			}

			// Loads a certain chunk of the tile map
			public TileChunk LoadChunk(int pos_x, int pos_y)
			{
				return Resources.Load<TileChunk>(GetChunkPath(pos_x, pos_y));
			}
		}
	}
}
