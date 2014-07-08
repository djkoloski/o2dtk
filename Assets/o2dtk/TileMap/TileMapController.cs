using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using o2dtk.Collections;

namespace o2dtk
{
	namespace TileMap
	{
		public class TileMapController : MonoBehaviour
		{
			// The colors to draw the gizmos with
			public static Color gizmos_color_tile = new Color(0.5f, 0.5f, 0.5f, 1.0f);
			public static Color gizmos_color_chunk = new Color(0.75f, 0.75f, 0.75f, 1.0f);
			
			// The tile map the controller will use
			public TileMap tile_map = null;

			// The number of pixels per unit the controller should render at
			public float pixels_per_unit = 32.0f;

			// The transform of the controller
			[SerializeField]
			private Transform transform_ = null;
			public new Transform transform
			{
				get
				{
					if (transform_ == null)
						transform_ = GetComponent<Transform>();
					return transform_;
				}
			}

			// The chunks that are currently loaded
			[System.Serializable]
			public class ChunkControllerMap : Map<int, TileChunkController>
			{ }
			public ChunkControllerMap chunk_controllers = new ChunkControllerMap();

			// The rendering root for the controller
			public GameObject render_root = null;
			public Transform render_transform = null;
			// The chunk root for rendering
			public GameObject chunk_root = null;
			public Transform chunk_transform = null;

			// Whether the controller has been initialized
			[SerializeField]
			private bool is_initialized = false;
			public bool initialized
			{
				get
				{
					return is_initialized;
				}
				private set
				{
					is_initialized = value;
				}
			}

			// Whether to draw gridlines for the chunks
			public bool draw_chunk_gridlines = true;
			public bool draw_tile_gridlines = true;
			// When to draw gridlines (always, selected, or never)
			public enum GridlinesDrawTime
			{
				Always,
				Selected,
				Never
			}
			public GridlinesDrawTime when_draw_gridlines = GridlinesDrawTime.Always;
			// The 4x4 matrix that transforms world space into map space
			public Matrix4x4 worldToMapMatrix
			{
				get
				{
					return Matrix4x4.Scale(Vector3.one * pixels_per_unit) * transform.worldToLocalMatrix;
				}
			}
			// The 4x4 matrix that transforms map space into world space
			public Matrix4x4 mapToWorldMatrix
			{
				get
				{
					return transform.localToWorldMatrix * Matrix4x4.Scale(Vector3.one / pixels_per_unit);
				}
			}

			// Gets the coordinates of a tile in the space relative to the parent of the controller
			public Vector3 GetWorldCoordinates(int x, int y)
			{
				return mapToWorldMatrix.MultiplyPoint(tile_map.GetLocalCoordinates(x, y));
			}

			// Gets the tile coordinates of the tile the given point lies in (the point is specified in world space)
			public void GetTileCoordinates(Vector3 world_point, out int x, out int y)
			{
				Vector3 point = worldToMapMatrix.MultiplyPoint(world_point);
				switch (tile_map.tiling)
				{
					case TileMap.Tiling.Rectangular:
					{
						point += new Vector3(tile_map.tile_size_x, tile_map.tile_size_y, 0.0f) / 2.0f;
						x = (int)Mathf.Round(point.x) / tile_map.tile_size_x;
						y = (int)Mathf.Round(point.y) / tile_map.tile_size_y;
						break;
					}
					case TileMap.Tiling.Isometric:
					{
						x = (int)Mathf.Round(point.x / tile_map.tile_size_x - point.y / tile_map.tile_size_y);
						y = (int)Mathf.Round(point.x / tile_map.tile_size_x + point.y / tile_map.tile_size_y);
						break;
					}
					case TileMap.Tiling.Staggered:
					{
						bool even = (tile_map.size_y % 2 == 0);
						if (even)
						{
							point.y += tile_map.tile_size_y / 2.0f;
							int even_x = (int)Mathf.Round(point.x) / tile_map.tile_size_x;
							int even_y = (int)Mathf.Round(point.y) / tile_map.tile_size_y;
							float diff_x = point.x - (even_x + 0.5f) * tile_map.tile_size_x;
							float diff_y = point.y - (even_y + 0.5f) * tile_map.tile_size_y;
							if (Mathf.Abs(diff_x) / tile_map.tile_size_x + Mathf.Abs(diff_y) / tile_map.tile_size_y > 0.5f)
							{
								x = even_x + (diff_x > 0.0f ? 1 : 0);
								y = even_y * 2 + (diff_y > 0.0f ? 1 : -1);
							}
							else
							{
								x = even_x;
								y = even_y * 2;
							}
						}
						else
						{
							point.x += tile_map.tile_size_x / 2.0f;
							point.y += tile_map.tile_size_y / 2.0f;
							int odd_x = (int)Mathf.Round(point.x) / tile_map.tile_size_x;
							int odd_y = (int)Mathf.Round(point.y) / tile_map.tile_size_y;
							float diff_x = point.x - (odd_x + 0.5f) * tile_map.tile_size_x;
							float diff_y = point.y - (odd_y + 0.5f) * tile_map.tile_size_y;
							if (Mathf.Abs(diff_x) / tile_map.tile_size_x + Mathf.Abs(diff_y) / tile_map.tile_size_y > 0.5f)
							{
								x = odd_x + (diff_x > 0.0f ? 0 : -1);
								y = odd_y * 2 + (diff_y > 0.0f ? 1 : -1);
							}
							else
							{
								x = odd_x;
								y = odd_y * 2;
							}
						}
						break;
					}
					default:
					{
						Debug.LogWarning("Unsupported tiling on tile map!");
						x = 0;
						y = 0;
						break;
					}
				}
			}

			public void Awake()
			{
				if (initialized)
					Debug.LogWarning("Map initialized before play mode entered! You probably didn't mean to do this.");
				Begin();
			}

			public void OnDrawGizmos()
			{
				if (when_draw_gridlines == GridlinesDrawTime.Always)
					DrawGridlineGizmos();
			}

			public void OnDrawGizmosSelected()
			{
				if (when_draw_gridlines == GridlinesDrawTime.Selected)
					DrawGridlineGizmos();
			}

			void DrawStaggeredGizmoRow(float start_x, float start_y, float size_x, float size_y, int tiles, bool down)
			{
				float cur_x = start_x;
				float jump = (down ? size_y / 2 : -size_y / 2);

				for (int i = 0; i < tiles; ++i)
				{
					Gizmos.DrawLine(new Vector3(cur_x, start_y, 0.0f), new Vector3(cur_x + size_x / 2, start_y - jump, 0.0f));
					Gizmos.DrawLine(new Vector3(cur_x + size_x / 2, start_y - jump, 0.0f), new Vector3(cur_x + size_x, start_y, 0.0f));
					cur_x += size_x;
				}
			}

			void DrawStaggeredGizmoCol(float start_x, float start_y, float size_x, float size_y, int tiles, bool left)
			{
				float cur_y = start_y;
				float jump = (left ? size_x / 2 : -size_x / 2);

				for (int i = 0; i < tiles; ++i)
				{
					Gizmos.DrawLine(new Vector3(start_x, cur_y, 0.0f), new Vector3(start_x - jump, cur_y + size_y / 2, 0.0f));
					++i;
					if (i >= tiles)
						break;
					Gizmos.DrawLine(new Vector3(start_x - jump, cur_y + size_y / 2, 0.0f), new Vector3(start_x, cur_y + size_y, 0.0f));
					cur_y += size_y;
				}
			}

			public void DrawGridlineGizmos()
			{
				if (tile_map == null || !(draw_chunk_gridlines || draw_tile_gridlines))
					return;

				Matrix4x4 gizmat = new Matrix4x4();
				Vector3 offset = new Vector3(tile_map.tile_size_x, (tile_map.tiling == TileMap.Tiling.Rectangular ? tile_map.tile_size_y : 0.0f), 0.0f) / -2.0f;
				gizmat.SetTRS(offset, Quaternion.identity, Vector3.one);
				Gizmos.matrix = mapToWorldMatrix * gizmat;

				switch (tile_map.tiling)
				{
					case TileMap.Tiling.Rectangular:
					{
						float x_max = tile_map.GetLocalXCoordinate(tile_map.size_x, 0);
						float y_max = tile_map.GetLocalYCoordinate(0, tile_map.size_y);

						float coord = 0.0f;

						for (int x = 0; x <= tile_map.size_x; ++x)
						{
							if (x % tile_map.chunk_size_x == 0 && draw_chunk_gridlines)
								Gizmos.color = gizmos_color_chunk;
							else
							{
								if (!draw_tile_gridlines)
									continue;
								Gizmos.color = gizmos_color_tile;
							}
							coord = tile_map.GetLocalXCoordinate(x, 0);
							Gizmos.DrawLine(new Vector3(coord, 0.0f, 0.0f), new Vector3(coord, y_max, 0.0f));
						}

						for (int y = 0; y <= tile_map.size_y; ++y)
						{
							if (y % tile_map.chunk_size_y == 0 && draw_chunk_gridlines)
								Gizmos.color = gizmos_color_chunk;
							else
							{
								if (!draw_tile_gridlines)
									continue;
								Gizmos.color = gizmos_color_tile;
							}
							coord = tile_map.GetLocalYCoordinate(0, y);
							Gizmos.DrawLine(new Vector3(0.0f, coord, 0.0f), new Vector3(x_max, coord, 0.0f));
						}

						break;
					}
					case TileMap.Tiling.Isometric:
					{
						float x_max_x = tile_map.GetLocalXCoordinate(tile_map.size_x, 0);
						float x_max_y = tile_map.GetLocalYCoordinate(tile_map.size_x, 0);
						float y_max_x = tile_map.GetLocalXCoordinate(0, tile_map.size_y);
						float y_max_y = tile_map.GetLocalYCoordinate(0, tile_map.size_y);

						float coord_x = 0.0f;
						float coord_y = 0.0f;

						for (int x = 0; x <= tile_map.size_x; ++x)
						{
							if (x % tile_map.chunk_size_x == 0 && draw_chunk_gridlines)
								Gizmos.color = gizmos_color_chunk;
							else
							{
								if (!draw_tile_gridlines)
									continue;
								Gizmos.color = gizmos_color_tile;
							}
							coord_x = tile_map.GetLocalXCoordinate(x, 0);
							coord_y = tile_map.GetLocalYCoordinate(x, 0);
							Gizmos.DrawLine(new Vector3(coord_x, coord_y, 0.0f), new Vector3(coord_x + y_max_x, coord_y + y_max_y, 0.0f));
						}

						for (int y = 0; y <= tile_map.size_y; ++y)
						{
							if (y % tile_map.chunk_size_y == 0 && draw_chunk_gridlines)
								Gizmos.color = gizmos_color_chunk;
							else
							{
								if (!draw_tile_gridlines)
									continue;
								Gizmos.color = gizmos_color_tile;
							}
							coord_x = tile_map.GetLocalXCoordinate(0, y);
							coord_y = tile_map.GetLocalYCoordinate(0, y);
							Gizmos.DrawLine(new Vector3(coord_x, coord_y, 0.0f), new Vector3(coord_x + x_max_x, coord_y + x_max_y, 0.0f));
						}

						break;
					}
					// WOW this is complicated. Not sure if there's an easier way.
					case TileMap.Tiling.Staggered:
					{
						bool even = (tile_map.size_y % 2 == 0);

						for (int y = 0; y <= tile_map.size_y; ++y)
						{
							float start_x = 0.0f;
							float start_y = (even ? ((y - 1) / 2 * 2 + 1) * tile_map.tile_size_y / 2 : y / 2 * tile_map.tile_size_y);
							bool down = (even ? y % 2 == 1 : y % 2 == 0);
							if (y == 0 && even)
							{
								start_x = tile_map.tile_size_x / 2;
								start_y = 0;
								down = true;
							}

							if (y % tile_map.chunk_size_y == 0 && draw_chunk_gridlines)
								Gizmos.color = gizmos_color_chunk;
							else
							{
								if (!draw_tile_gridlines)
									continue;
								Gizmos.color = gizmos_color_tile;
							}

							DrawStaggeredGizmoRow(start_x, start_y, tile_map.tile_size_x, tile_map.tile_size_y, tile_map.size_x, down);
						}

						if (draw_chunk_gridlines)
						{
							Gizmos.color = gizmos_color_chunk;
							for (int x = 0; x <= tile_map.chunks_x && x * tile_map.chunk_size_x <= tile_map.size_x; ++x)
							{
								float start_x = x * tile_map.chunk_size_x * tile_map.tile_size_x + (even ? tile_map.tile_size_x / 2 : 0.0f);
								float start_y = 0.0f;

								DrawStaggeredGizmoCol(start_x, start_y, tile_map.tile_size_x, tile_map.tile_size_y, tile_map.size_y, even);
							}
						}

						break;
					}
					default:
						Debug.LogWarning("Unsupported tiling on tile map!");
						break;
				}
			}

			// Initializes the tile map controller
			public void Begin()
			{
				if (initialized || tile_map == null)
					return;

				chunk_controllers = new ChunkControllerMap();

				render_root = new GameObject("render_root");
				render_transform = render_root.GetComponent<Transform>();
				render_transform.parent = transform;
				render_transform.localPosition = Vector3.zero;
				render_transform.localScale = Vector3.one / pixels_per_unit;

				chunk_root = new GameObject("chunk_root");
				chunk_transform = chunk_root.GetComponent<Transform>();
				chunk_transform.parent = render_transform;
				chunk_transform.localPosition = Vector3.zero;
				chunk_transform.localScale = Vector3.one;

				initialized = true;
			}

			// Returns the tile map controller to its initial state
			public void End()
			{
				if (!initialized)
					return;

				Utility.GameObject.Destroy(render_root);
				Utility.GameObject.Destroy(chunk_root);

				chunk_controllers = null;
				render_root = null;
				render_transform = null;
				chunk_root = null;
				chunk_transform = null;

				initialized = false;
			}

			// Loads the chunk at the given indices
			public void LoadChunk(int index_x, int index_y)
			{
				int index = tile_map.GetIndex(index_x, index_y);

				if (chunk_controllers.ContainsKey(index))
					return;

				TileChunk chunk = tile_map.LoadChunk(index_x, index_y);

				if (chunk == null)
					return;

				chunk_controllers[index] = CreateChunk(chunk, index_x, index_y);
			}

			// Unloads the chunk at the given coordinates
			public void UnloadChunk(int index_x, int index_y)
			{
				int index = tile_map.GetIndex(index_x, index_y);

				if (chunk_controllers.ContainsKey(index))
				{
					Utility.GameObject.Destroy(chunk_controllers[index].gameObject);
					chunk_controllers.Remove(index);
				}
			}

			// Makes a chunk and returns it
			public TileChunkController CreateChunk(TileChunk chunk, int index_x, int index_y)
			{
				GameObject chunk_go = new GameObject(index_x + "_" + index_y);
				TileChunkController controller = chunk_go.AddComponent<TileChunkController>();
				controller.LoadChunk(tile_map, chunk, index_x, index_y, chunk_transform);
				return controller;
			}
		}
	}
}
