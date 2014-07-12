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
			// Static properties

			// The colors to draw the gizmos with
			public static Color gizmos_color_tile = new Color(0.5f, 0.5f, 0.5f, 1.0f);
			public static Color gizmos_color_chunk = new Color(0.75f, 0.75f, 0.75f, 1.0f);

			// User properties

			// The tile map the controller will use
			public TileMap tile_map = null;
			// The number of pixels per unit the controller should render at
			public float pixels_per_unit = 32.0f;
			// The render intercept to use (if any)
			public TileMapRenderIntercept render_intercept = null;
			// The update intercept to use (if any)
			public TileMapUpdateIntercept update_intercept = null;

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

			// Public properties

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

			// The 4x4 matrix that transforms local space into map space
			public Matrix4x4 localToMapMatrix
			{
				get
				{
					return Matrix4x4.Scale(Vector3.one * pixels_per_unit);
				}
			}
			// The 4x4 matrix that transforms world space into local space
			public Matrix4x4 worldToLocalMatrix
			{
				get
				{
					return transform.worldToLocalMatrix;
				}
			}
			// The 4x4 matrix that transforms map space into world space
			public Matrix4x4 mapToLocalMatrix
			{
				get
				{
					return Matrix4x4.Scale(Vector3.one / pixels_per_unit);
				}
			}
			// The 4x4 matrix that transforms local space into world space
			public Matrix4x4 localToWorldMatrix
			{
				get
				{
					return transform.localToWorldMatrix;
				}
			}

			// The compound 4x4 matrix that transforms map space into world space
			public Matrix4x4 mapToWorldMatrix
			{
				get
				{
					return localToWorldMatrix * mapToLocalMatrix;
				}
			}
			// The compound 4x4 matrix that transforms world space into map space
			public Matrix4x4 worldToMapMatrix
			{
				get
				{
					return localToMapMatrix * worldToLocalMatrix;
				}
			}

			// Private properties

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
			public class ChunkControllerMap : Map<IPair, TileChunkController>
			{ }
			public ChunkControllerMap chunk_controllers = new ChunkControllerMap();

			// The rendering root for the controller
			public GameObject render_root = null;
			public Transform render_root_transform = null;
			// The chunk root for rendering
			public GameObject chunk_root = null;
			public Transform chunk_root_transform = null;

			// Gets the coordinates of a tile in normal space
			public Vector3 TileToNormalPoint(int x, int y)
			{
				return tile_map.GetNormalCoordinates(x, y);
			}

			// Gets the coordinates of a tile in map space
			public Vector3 TileToMapPoint(int x, int y)
			{
				return NormalToMapPoint(TileToNormalPoint(x, y));
			}

			// Gets the coordinates of a tile in local space
			public Vector3 TileToLocalPoint(int x, int y)
			{
				return MapToLocalPoint(TileToMapPoint(x, y));
			}

			// Gets the coordinates of a tile in world space
			public Vector3 TileToWorldPoint(int x, int y)
			{
				return LocalToWorldPoint(TileToLocalPoint(x, y));
			}

			// Gets the coordinates of a normal space point in map space
			public Vector3 NormalToMapPoint(Vector3 normal_point)
			{
				return tile_map.normalToMapMatrix.MultiplyPoint(normal_point);
			}

			// Gets the coordinates of a normal space point in local space
			public Vector3 NormalToLocalPoint(Vector3 normal_point)
			{
				return MapToLocalPoint(NormalToMapPoint(normal_point));
			}

			// Gets the coordinates of a normal space point in world space
			public Vector3 NormalToWorldPoint(Vector3 normal_point)
			{
				return LocalToWorldPoint(NormalToLocalPoint(normal_point));
			}

			// Gets the coordinates of a map space point in local space
			public Vector3 MapToLocalPoint(Vector3 map_point)
			{
				return mapToLocalMatrix.MultiplyPoint(map_point);
			}

			// Gets the coordinates of a map space point in world space
			public Vector3 MapToWorldPoint(Vector3 map_point)
			{
				return LocalToWorldPoint(MapToLocalPoint(map_point));
			}

			// Gets the coordinates of a local space point in world space
			public Vector3 LocalToWorldPoint(Vector3 local_point)
			{
				return localToWorldMatrix.MultiplyPoint(local_point);
			}

			// Gets the coordinates of a world space point in local space
			public Vector3 WorldToLocalPoint(Vector3 world_point)
			{
				return worldToLocalMatrix.MultiplyPoint(world_point);
			}

			// Gets the coordinates of a world space point in map space
			public Vector3 WorldToMapPoint(Vector3 world_point)
			{
				return LocalToMapPoint(WorldToLocalPoint(world_point));
			}

			// Gets the coordinates of a world space point in normal space
			public Vector3 WorldToNormalPoint(Vector3 world_point)
			{
				return MapToNormalPoint(WorldToMapPoint(world_point));
			}

			// Gets the coordinates of a tile from a point in world space
			public void WorldToTile(Vector3 world_point, out int x, out int y)
			{
				NormalToTile(WorldToNormalPoint(world_point), out x, out y);
			}

			// Gets the coordinates of a local space point in map space
			public Vector3 LocalToMapPoint(Vector3 local_point)
			{
				return localToMapMatrix.MultiplyPoint(local_point);
			}

			// Gets the coordinates of a local space point in normal space
			public Vector3 LocalToNormalPoint(Vector3 local_point)
			{
				return MapToNormalPoint(LocalToMapPoint(local_point));
			}

			// Gets the coordinates of a tile from a point in local space
			public void LocalToTile(Vector3 local_point, out int x, out int y)
			{
				NormalToTile(LocalToNormalPoint(local_point), out x, out y);
			}

			// Gets the coordinates of a map space point in normal space
			public Vector3 MapToNormalPoint(Vector3 map_point)
			{
				return tile_map.mapToNormalMatrix.MultiplyPoint(map_point);
			}

			// Gets the coordinates of a tile from a point in map space
			public void MapToTile(Vector3 map_point, out int x, out int y)
			{
				NormalToTile(MapToNormalPoint(map_point), out x, out y);
			}

			// Gets the coordinates of a tile from a point in normal space
			public void NormalToTile(Vector3 normal_point, out int x, out int y)
			{
				tile_map.GetTileFromNormalPoint(normal_point, out x, out y);
			}

			public void Awake()
			{
				if (initialized)
					throw new System.InvalidOperationException("Tile map initialized before play mode was entered");
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

			bool ChooseGizmosColor(bool user_choice)
			{
				if (user_choice && draw_chunk_gridlines)
					Gizmos.color = gizmos_color_chunk;
				else
				{
					if (!draw_tile_gridlines)
						return false;
					Gizmos.color = gizmos_color_tile;
				}
				return true;
			}

			public void DrawGridlineGizmos()
			{
				if (tile_map == null || !(draw_chunk_gridlines || draw_tile_gridlines))
					return;
				
				Gizmos.matrix = mapToWorldMatrix * tile_map.normalToMapMatrix;

				switch (tile_map.tiling)
				{
					case TileMap.Tiling.Rectangular:
					case TileMap.Tiling.Isometric:
					{
						for (int x = tile_map.left; x <= tile_map.right + 1; ++x)
						{
							if (!ChooseGizmosColor(x % tile_map.chunk_size_x == 0))
								continue;
							Gizmos.DrawLine(new Vector3(x - 0.5f, tile_map.bottom - 0.5f, 0.0f), new Vector3(x - 0.5f, tile_map.top + 0.5f, 0.0f));
						}

						for (int y = tile_map.bottom; y <= tile_map.top + 1; ++y)
						{
							if (!ChooseGizmosColor(y % tile_map.chunk_size_y == 0))
								continue;
							Gizmos.DrawLine(new Vector3(tile_map.left - 0.5f, y - 0.5f, 0.0f), new Vector3(tile_map.right + 0.5f, y - 0.5f, 0.0f));
						}

						break;
					}
					case TileMap.Tiling.StaggeredEven:
					{
						for (int x = tile_map.left; x <= tile_map.right; ++x)
						{
							for (int y = tile_map.bottom; y <= tile_map.top; ++y)
							{
								bool border_chunk_down = (y % tile_map.chunk_size_y == 0);
								bool border_chunk_up = ((y + 1) % tile_map.chunk_size_y == 0);
								bool border_chunk_left = (x % tile_map.chunk_size_x == 0) && (y % 2 != 0);
								bool border_chunk_right = ((x + 1) % tile_map.chunk_size_x == 0) && (y % 2 == 0);

								if (ChooseGizmosColor(border_chunk_down || border_chunk_left))
									Gizmos.DrawLine(new Vector3(x + (y % 2 == 0 ? 0.5f : 0.0f), y / 2.0f - 0.5f, 0.0f), new Vector3(x - (y % 2 == 0 ? 0.0f : 0.5f), y / 2.0f, 0.0f));
								if (ChooseGizmosColor(border_chunk_left || border_chunk_up))
									Gizmos.DrawLine(new Vector3(x - (y % 2 == 0 ? 0.0f : 0.5f), y / 2.0f, 0.0f), new Vector3(x + (y % 2 == 0 ? 0.5f : 0.0f), y / 2.0f + 0.5f, 0.0f));
								if (ChooseGizmosColor(border_chunk_up || border_chunk_right))
									Gizmos.DrawLine(new Vector3(x + (y % 2 == 0 ? 0.5f : 0.0f), y / 2.0f + 0.5f, 0.0f), new Vector3(x + (y % 2 == 0 ? 1.0f : 0.5f), y / 2.0f, 0.0f));
								if (ChooseGizmosColor(border_chunk_right || border_chunk_down))
									Gizmos.DrawLine(new Vector3(x + (y % 2 == 0 ? 1.0f : 0.5f), y / 2.0f, 0.0f), new Vector3(x + (y % 2 == 0 ? 0.5f : 0.0f), y / 2.0f - 0.5f, 0.0f));
							}
						}

						break;
					}
					case TileMap.Tiling.StaggeredOdd:
					{
						for (int x = tile_map.left; x <= tile_map.right; ++x)
						{
							for (int y = tile_map.bottom; y <= tile_map.top; ++y)
							{
								bool border_chunk_down = (y % tile_map.chunk_size_y == 0);
								bool border_chunk_up = ((y + 1) % tile_map.chunk_size_y == 0);
								bool border_chunk_left = (x % tile_map.chunk_size_x == 0) && (y % 2 == 0);
								bool border_chunk_right = ((x + 1) % tile_map.chunk_size_x == 0) && (y % 2 != 0);

								if (ChooseGizmosColor(border_chunk_down || border_chunk_left))
									Gizmos.DrawLine(new Vector3(x + (y % 2 == 0 ? 0.0f : 0.5f), y / 2.0f - 0.5f, 0.0f), new Vector3(x - (y % 2 == 0 ? 0.5f : 0.0f), y / 2.0f, 0.0f));
								if (ChooseGizmosColor(border_chunk_left || border_chunk_up))
									Gizmos.DrawLine(new Vector3(x - (y % 2 == 0 ? 0.5f : 0.0f), y / 2.0f, 0.0f), new Vector3(x + (y % 2 == 0 ? 0.0f : 0.5f), y / 2.0f + 0.5f, 0.0f));
								if (ChooseGizmosColor(border_chunk_up || border_chunk_right))
									Gizmos.DrawLine(new Vector3(x + (y % 2 == 0 ? 0.0f : 0.5f), y / 2.0f + 0.5f, 0.0f), new Vector3(x + (y % 2 == 0 ? 0.5f : 1.0f), y / 2.0f, 0.0f));
								if (ChooseGizmosColor(border_chunk_right || border_chunk_down))
									Gizmos.DrawLine(new Vector3(x + (y % 2 == 0 ? 0.5f : 1.0f), y / 2.0f, 0.0f), new Vector3(x + (y % 2 == 0 ? 0.0f : 0.5f), y / 2.0f - 0.5f, 0.0f));
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
				render_root_transform = render_root.GetComponent<Transform>();
				render_root_transform.parent = transform;
				render_root_transform.localPosition = Vector3.zero;
				render_root_transform.localScale = Vector3.one;

				chunk_root = new GameObject("chunk_root");
				chunk_root_transform = chunk_root.GetComponent<Transform>();
				chunk_root_transform.parent = render_root_transform;
				chunk_root_transform.localPosition = Vector3.zero;
				chunk_root_transform.localScale = Vector3.one;

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
				render_root_transform = null;
				chunk_root = null;
				chunk_root_transform = null;

				initialized = false;
			}

			// Determines whether the given chunk is loaded or not
			public bool IsChunkLoaded(int index_x, int index_y)
			{
				return chunk_controllers.ContainsKey(new IPair(index_x, index_y));
			}

			// Loads the chunk at the given indices
			public void LoadChunk(int index_x, int index_y)
			{
				IPair index = new IPair(index_x, index_y);

				if (chunk_controllers.ContainsKey(index))
					return;

				TileChunk chunk = tile_map.GetChunk(index_x, index_y);

				if (chunk == null)
					return;

				chunk_controllers[index] = CreateChunk(chunk, index_x, index_y);
			}

			// Unloads the chunk at the given coordinates
			public void UnloadChunk(int index_x, int index_y)
			{
				IPair index = new IPair(index_x, index_y);

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
				controller.Initialize(this, chunk);
				return controller;
			}
		}
	}
}
