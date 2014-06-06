using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.IO;

namespace o2dtk
{
	namespace TileMap
	{
		[System.Serializable]
		public class TileMap : ScriptableObject
		{
			// The major delta of the tile map in pixels
			// This is the amount advanced for each addition to the X coordinate of a tile
			public int major_delta_x;
			public int major_delta_y;
			public int major_delta_z;
			// The minor delta of the tile map in pixels
			// This is the amount advanced for each addition to the Y coordinate of a tile
			public int minor_delta_x;
			public int minor_delta_y;
			public int minor_delta_z;
			// The even delta of the tile map in pixels
			// This is the amount advanced if the Y coordinate of a tile is odd
			public int odd_delta_x;
			public int odd_delta_y;
			public int odd_delta_z;

			// The size of the tile map in tiles
			public int size_x;
			public int size_y;
			// The size of each chunk of the map in tiles
			public int chunk_size_x;
			public int chunk_size_y;

			// A library of the tile sets used by the tile map
			public TileLibrary library;
			// Information about each of the layers in the tile map
			public List<TileMapLayerInfo> layer_info;

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

			// Gets the X coordinate of a tile given its X and Y
			public int GetLocalXCoordinate(int x, int y)
			{
				return x * major_delta_x + y * minor_delta_x + (y % 2 == 1 ? odd_delta_x : 0);
			}

			// Gets the Y coordinate of a tile given its X and Y
			public int GetLocalYCoordinate(int x, int y)
			{
				return x * major_delta_y + y * minor_delta_y + (y % 2 == 1 ? odd_delta_y : 0);
			}

			// Gets the Z coordinate of a tile given its X and Y
			public int GetLocalZCoordinate(int x, int y)
			{
				return x * major_delta_z + y * minor_delta_z + (y % 2 == 1 ? odd_delta_z : 0);
			}

			// Gets the local position of a tile given its X and Y
			public Vector2 GetLocalCoordinates(int x, int y)
			{
				return new Vector2(GetLocalXCoordinate(x, y), GetLocalYCoordinate(x, y));
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
