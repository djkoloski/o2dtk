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
			// The tile map the controller will use
			public TileMap tile_map = null;

			// The number of pixels per unit the controller should render at
			public float pixels_per_unit = 32.0f;

			// The transform of the controller
			public new Transform transform = null;

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

			// Gets the X coordinate of a tile in the space relative to the parent of the controller
			public float GetXCoordinate(int x, int y)
			{
				return tile_map.GetLocalXCoordinate(x, y) / pixels_per_unit + transform.localPosition.x;
			}

			// Gets the Y coordinate of a tile in the space relative to the parent of the controller
			public float GetYCoordinate(int x, int y)
			{
				return tile_map.GetLocalYCoordinate(x, y) / pixels_per_unit + transform.localPosition.y;
			}

			// Gets the coordinates of a tile in the space relative to the parent of the controller
			public Vector2 GetCoordinates(int x, int y)
			{
				return new Vector2(GetXCoordinate(x, y), GetYCoordinate(x, y));
			}

			public void Awake()
			{
				if (initialized)
					Debug.LogWarning("Map initialized before play mode entered! You probably didn't mean to do this.");
				Begin();
			}

			// Initializes the tile map controller
			public void Begin()
			{
				if (initialized || tile_map == null)
					return;

				transform = GetComponent<Transform>();

				chunk_controllers = new ChunkControllerMap();

				render_root = new GameObject("render_root");
				render_transform = render_root.GetComponent<Transform>();
				render_transform.parent = transform;
				render_transform.localPosition = Vector3.zero;
				render_transform.localScale = new Vector3(1.0f / pixels_per_unit, 1.0f / pixels_per_unit, 1.0f);

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
