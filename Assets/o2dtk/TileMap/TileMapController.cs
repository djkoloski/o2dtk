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

			// The 4x4 matrix that transforms world space into local space
			public Matrix4x4 worldToLocalMatrix
			{
				get
				{
					return transform.worldToLocalMatrix;
				}
			}
			// The 4x4 matrix that transforms world space into map space
			public Matrix4x4 worldToMapMatrix
			{
				get
				{
					return localToMapMatrix * worldToLocalMatrix;
				}
			}
			// The 4x4 matrix that transforms world space into normal space
			public Matrix4x4 worldToNormalMatrix
			{
				get
				{
					return tile_map.mapToNormalMatrix * worldToMapMatrix;
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
			// The 4x4 matrix that transforms local space into map space
			public Matrix4x4 localToMapMatrix
			{
				get
				{
					return Matrix4x4.Scale(new Vector3(pixels_per_unit, pixels_per_unit, 1.0f));
				}
			}
			// The 4x4 matrix that tarnsforms local space into normal space
			public Matrix4x4 localToNormalMatrix
			{
				get
				{
					return tile_map.mapToNormalMatrix * localToMapMatrix;
				}
			}

			// The 4x4 matrix that transforms map space into world space
			public Matrix4x4 mapToWorldMatrix
			{
				get
				{
					return localToWorldMatrix * mapToLocalMatrix;
				}
			}
			// The 4x4 matrix that transforms map space into world space
			public Matrix4x4 mapToLocalMatrix
			{
				get
				{
					return Matrix4x4.Scale(new Vector3(1.0f / pixels_per_unit, 1.0f / pixels_per_unit, 1.0f));
				}
			}
			// The 4x4 matrix that transforms map space into normal space
			public Matrix4x4 mapToNormalMatrix
			{
				get
				{
					return tile_map.mapToNormalMatrix;
				}
			}

			// The compound 4x4 matrix that transforms normal space into world space
			public Matrix4x4 normalToWorldMatrix
			{
				get
				{
					return mapToWorldMatrix * tile_map.normalToMapMatrix;
				}
			}
			// The compound 4x4 matrix that transforms normal space into local space
			public Matrix4x4 normalToLocalMatrix
			{
				get
				{
					return mapToLocalMatrix * tile_map.normalToMapMatrix;
				}
			}
			// The compound 4x4 matrix that transforms normal space into map space
			public Matrix4x4 normalToMapMatrix
			{
				get
				{
					return tile_map.normalToMapMatrix;
				}
			}

			// Gets the coordinates of a tile in normal space
			public Vector3 TileToNormalSpace(int x, int y)
			{
				return tile_map.GetNormalCoordinates(x, y);
			}
			// Gets the tile of coordinates in normal space
			public void NormalToTile(Vector3 normal_point, out int x, out int y)
			{
				tile_map.GetTileFromNormalPoint(normal_point, out x, out y);
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
				
				Gizmos.matrix = normalToWorldMatrix;

				switch (tile_map.tiling)
				{
					case TileMap.Tiling.Rectangular:
					case TileMap.Tiling.Isometric:
					{
						for (int x = tile_map.left; x <= tile_map.right + 1; ++x)
						{
							if (!ChooseGizmosColor(x % tile_map.chunk_size_x == 0))
								continue;
							Gizmos.DrawLine(new Vector3(x, tile_map.bottom, 0.0f), new Vector3(x, tile_map.top + 1.0f, 0.0f));
						}

						for (int y = tile_map.bottom; y <= tile_map.top + 1; ++y)
						{
							if (!ChooseGizmosColor(y % tile_map.chunk_size_y == 0))
								continue;
							Gizmos.DrawLine(new Vector3(tile_map.left, y, 0.0f), new Vector3(tile_map.right + 1.0f, y, 0.0f));
						}

						break;
					}
					case TileMap.Tiling.StaggeredOdd:
					case TileMap.Tiling.StaggeredEven:
					{
						float even_stagger = (tile_map.tiling == TileMap.Tiling.StaggeredOdd ? 0.0f : 0.5f);
						float odd_stagger = (tile_map.tiling == TileMap.Tiling.StaggeredOdd ? 0.5f : 0.0f);
						bool border_even = (tile_map.tiling == TileMap.Tiling.StaggeredOdd);

						for (int x = tile_map.left; x <= tile_map.right; ++x)
						{
							for (int y = tile_map.bottom; y <= tile_map.top; ++y)
							{
								bool border_chunk_down = (y % tile_map.chunk_size_y == 0);
								bool border_chunk_up = ((y + 1) % tile_map.chunk_size_y == 0);
								bool border_chunk_left = (x % tile_map.chunk_size_x == 0) && ((y % 2 == 0) == border_even);
								bool border_chunk_right = ((x + 1) % tile_map.chunk_size_x == 0) && ((y % 2 == 0) != border_even);
								float dx = x + (y % 2 == 0 ? even_stagger : odd_stagger);
								float dy = y / 2.0f;

								if (ChooseGizmosColor(border_chunk_down || border_chunk_left))
									Gizmos.DrawLine(new Vector3(dx + 0.5f, dy, 0.0f), new Vector3(dx, dy + 0.5f, 0.0f));
								if (ChooseGizmosColor(border_chunk_left || border_chunk_up))
									Gizmos.DrawLine(new Vector3(dx, dy + 0.5f, 0.0f), new Vector3(dx + 0.5f, dy + 1.0f, 0.0f));
								if (ChooseGizmosColor(border_chunk_up || border_chunk_right))
									Gizmos.DrawLine(new Vector3(dx + 0.5f, dy + 1.0f, 0.0f), new Vector3(dx + 1.0f, dy + 0.5f, 0.0f));
								if (ChooseGizmosColor(border_chunk_right || border_chunk_down))
									Gizmos.DrawLine(new Vector3(dx + 1.0f, dy + 0.5f, 0.0f), new Vector3(dx + 0.5f, dy, 0.0f));
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
