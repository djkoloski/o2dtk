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
				StaggeredOdd,
				StaggeredEven
			};
			// The available origins maps may have
			public enum Origin
			{
				// The default origin; x is right and y is up
				BottomLeft,
				// Tiled's default origin; x is right and y is down
				TopLeft,
				// Some wacky ones to round out the permutations
				TopRight,
				BottomRight
			};

			// The bounds of the tile map in tiles
			public int left = 0;
			public int right = 0;
			public int bottom = 0;
			public int top = 0;
			// Calculates the size of the tile map
			public int size_x
			{
				get
				{
					return right - left + 1;
				}
			}
			public int size_y
			{
				get
				{
					return top - bottom + 1;
				}
			}
			// The tiling pattern of the map
			public Tiling tiling = Tiling.Rectangular;
			// The origin of the map
			public Origin origin = Origin.BottomLeft;
			// The precedence scale of the map
			//   A positive value indicates that tiles with greater coordinates will be
			//   drawn on top of those with lower coordinates
			//   A negative value indicates that tiles with lesser coordinates will be
			//   drawn on top of those with higher coordinates
			public float precedence_scale_x = 1.0f;
			public float precedence_scale_y = 1.0f;
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

			// The 4x4 matrix that transforms normal space into map space
			public Matrix4x4 normalToMapMatrix
			{
				get
				{
					float scale_x = (origin == Origin.BottomRight || origin == Origin.TopRight ? -1.0f : 1.0f);
					float scale_y = (origin == Origin.TopLeft || origin == Origin.TopRight ? -1.0f : 1.0f);
					Matrix4x4 mat = new Matrix4x4();
					switch (tiling)
					{
						case Tiling.Rectangular:
							mat.SetColumn(0, scale_x * new Vector4(tile_size_x, 0, 0, 0));
							mat.SetColumn(1, scale_y * new Vector4(0, tile_size_y, 0, 0));
							break;
						case Tiling.Isometric:
							mat.SetColumn(0, scale_x * new Vector4(tile_size_x / 2.0f, -tile_size_y / 2.0f, 0, 0));
							mat.SetColumn(1, scale_y * new Vector4(tile_size_x / 2.0f, tile_size_y / 2.0f, 0, 0));
							break;
						case Tiling.StaggeredOdd:
						case Tiling.StaggeredEven:
							mat.SetColumn(0, scale_x * new Vector4(tile_size_x, 0, 0, 0));
							mat.SetColumn(1, scale_y * new Vector4(0, tile_size_y, 0, 0));
							break;
						default:
							throw new System.InvalidOperationException("Unsupported tiling on tile map");
					}
					mat.SetColumn(2, new Vector4(0, 0, 1, 0));
					mat.SetColumn(3, new Vector4(0, 0, 0, 1));
					return mat;
				}
			}
			// The 4x4 matrix that transforms map space into normal space
			public Matrix4x4 mapToNormalMatrix
			{
				get
				{
					return normalToMapMatrix.inverse;
				}
			}

			// Returns the total number of tiles in the map
			public int tiles_total
			{
				get
				{
					return size_x * size_y;
				}
			}

			// Returns the number of chunks in the map
			public int chunk_left
			{
				get
				{
					return Utility.Math.FloorDivide(left, chunk_size_x);
				}
			}
			public int chunk_right
			{
				get
				{
					return Utility.Math.FloorDivide(right, chunk_size_x);
				}
			}
			public int chunk_bottom
			{
				get
				{
					return Utility.Math.FloorDivide(bottom, chunk_size_y);
				}
			}
			public int chunk_top
			{
				get
				{
					return Utility.Math.FloorDivide(top, chunk_size_y);
				}
			}
			public int chunks_x
			{
				get
				{
					return chunk_right - chunk_left + 1;
				}
			}
			public int chunks_y
			{
				get
				{
					return chunk_top - chunk_bottom + 1;
				}
			}
			public int chunks_total
			{
				get
				{
					return chunks_x * chunks_y;
				}
			}

			// Gets the X coordinate of a tile in normal space given its X and Y
			private float GetNormalXCoordinate(int x, int y)
			{
				switch (tiling)
				{
					case Tiling.Rectangular:
					case Tiling.Isometric:
						return x;
					case Tiling.StaggeredOdd:
						return x + (y % 2 == 0 ? 0.0f : 0.5f);
					case Tiling.StaggeredEven:
						return x + (y % 2 == 0 ? 0.5f : 0.0f);
					default:
						throw new System.InvalidOperationException("Unsupported tiling value on tile map");
				}
			}

			// Gets the Y coordinate of a tile in normal space given its X and Y
			private float GetNormalYCoordinate(int x, int y)
			{
				switch (tiling)
				{
					case Tiling.Rectangular:
					case Tiling.Isometric:
						return y;
					case Tiling.StaggeredOdd:
						return y / 2.0f;
					case Tiling.StaggeredEven:
						return y / 2.0f;
					default:
						throw new System.InvalidOperationException("Unsupported tiling value on tile map");
				}
			}

			// Gets the Z coordinate of a tile in normal space given its X and Y
			public float GetNormalZCoordinate(int x, int y)
			{
				switch (tiling)
				{
					case Tiling.Rectangular:
					case Tiling.Isometric:
						return x * precedence_scale_x + y * precedence_scale_y;
					case Tiling.StaggeredOdd:
						return x * (precedence_scale_x + precedence_scale_y) + (y / 2) * (precedence_scale_y - precedence_scale_x) + (y > 0 ? 1 : -1) * (y % 2 == 0 ? 0 : precedence_scale_y);
					case Tiling.StaggeredEven:
						return x * (precedence_scale_x + precedence_scale_y) + (y / 2) * (precedence_scale_y - precedence_scale_x) - (y > 0 ? 1 : -1) * (y % 2 == 0 ? 0 : precedence_scale_x);
					default:
						throw new System.InvalidOperationException("Unsupported tiling value on tile map");
				}
			}

			public Vector3 GetNormalCoordinates(int x, int y)
			{
				return new Vector3(GetNormalXCoordinate(x, y), GetNormalYCoordinate(x, y), GetNormalZCoordinate(x, y));
			}

			public void GetTileFromNormalPoint(Vector3 normal_point, out int x, out int y)
			{
				switch (tiling)
				{
					case TileMap.Tiling.Rectangular:
					case TileMap.Tiling.Isometric:
					{
						x = Mathf.FloorToInt(normal_point.x);
						y = Mathf.FloorToInt(normal_point.y);
						break;
					}
					case TileMap.Tiling.StaggeredOdd:
					case TileMap.Tiling.StaggeredEven:
					{
						if (tiling == Tiling.StaggeredEven)
							normal_point.x -= 0.5f;
						int near_x = Mathf.FloorToInt(normal_point.x);
						int near_y = Mathf.FloorToInt(normal_point.y);
						float diff_x = normal_point.x - (near_x + 0.5f);
						float diff_y = normal_point.y - (near_y + 0.5f);
						if (Mathf.Abs(diff_x) + Mathf.Abs(diff_y) > 0.5f)
						{
							x = near_x + (diff_x > 0.0f ? 0 : -1) + (tiling == Tiling.StaggeredEven ? 1 : 0);
							y = near_y * 2 + (diff_y > 0.0f ? 1 : -1);
						}
						else
						{
							x = near_x;
							y = near_y * 2;
						}
						break;
					}
					default:
						throw new System.InvalidOperationException("Unsupported tiling value on tile map");
				}
			}

			// Gets the path to a certain chunk of the tile map
			public string GetChunkPath(int pos_x, int pos_y)
			{
				return resources_dir + "/chunks/" + pos_x + "_" + pos_y;
			}

			// Gets a certain chunk of the tile map
			//   If no generator is being used, it loads the chunk from the resources folder
			//   If a generator is being used, it requests the chunk from the generator
			public TileChunk GetChunk(int pos_x, int pos_y)
			{
				if (chunk_generator != null)
					return chunk_generator.GetChunk(this, pos_x, pos_y);
				else
					return Resources.Load<TileChunk>(GetChunkPath(pos_x, pos_y));
			}
		}
	}
}
