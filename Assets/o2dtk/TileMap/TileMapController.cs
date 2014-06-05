using UnityEngine;
using System.Collections;
using System.Collections.Generic;

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
			private new Transform transform = null;

			// The chunks that are currently loaded
			private Dictionary<int, TileChunk> chunks = null;

			// The rendering root for the controller
			private GameObject render_root = null;
			private Transform render_transform = null;
			// The chunk root for rendering
			private GameObject chunk_root = null;
			private Transform chunk_transform = null;

			// Whether the controller has been initialized
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

			public void Awake()
			{
				Begin();
			}

			// Initializes the tile map controller
			public void Begin()
			{
				if (initialized || tile_map == null)
					return;

				transform = GetComponent<Transform>();

				chunks = new Dictionary<int, TileChunk>();

				render_root = new GameObject("render_root");
				render_transform = render_root.GetComponent<Transform>();
				render_transform.parent = transform;
				render_transform.localPosition = Vector3.zero;
				render_transform.localScale = new Vector3(1.0f / pixels_per_unit, 1.0f / pixels_per_unit, 1.0f);

				chunk_root = new GameObject("chunk_root");
				chunk_transform = chunk_root.GetComponent<Transform>();
				chunk_transform.parent = transform;
				chunk_transform.localPosition = Vector3.zero;

				initialized = true;
			}

			// Returns the tile map controller to its initial state
			public void End()
			{
				if (!initialized)
					return;

				Utility.GameObject.Destroy(render_root);
				Utility.GameObject.Destroy(chunk_root);

				chunks = null;
				render_root = null;
				render_transform = null;
				chunk_root = null;
				chunk_transform = null;

				initialized = false;
			}

			// Loads the chunk at the given coordinates
			public void LoadChunk(int pos_x, int pos_y)
			{
				int index = tile_map.GetIndex(pos_x, pos_y);

				if (chunks.ContainsKey(index))
					return;

				TileChunk chunk = tile_map.LoadChunk(pos_x, pos_y);

				if (chunk == null)
					return;

				chunks[index] = chunk;

				RenderChunk(chunk, pos_x, pos_y);
			}

			// Unloads the chunk at the given coordinates
			public void UnloadChunk(int pos_x, int pos_y)
			{
				int index = tile_map.GetIndex(pos_x, pos_y);

				if (chunks.ContainsKey(index))
				{
					chunks.Remove(index);
					DestroyChunk(pos_x, pos_y);
				}
			}

			// Builds renderable sprites out of a chunk's data
			public void RenderChunk(TileChunk chunk, int chunk_pos_x, int chunk_pos_y)
			{
				GameObject root = new GameObject(chunk_pos_x + "_" + chunk_pos_y);
				Transform root_transform = root.GetComponent<Transform>();

				for (int l = 0; l < chunk.data_layers.Count; ++l)
				{
					GameObject layer_root = new GameObject(tile_map.layer_info[l].name);
					Transform layer_transform = layer_root.GetComponent<Transform>();
					layer_transform.parent = root_transform;
					layer_transform.localPosition = new Vector3(0.0f, 0.0f, chunk.data_layers.Count - l - 1);

					for (int y = 0; y < chunk.size_y; ++y)
					{
						for (int x = 0; x < chunk.size_x; ++x)
						{
							int map_pos_x = chunk.pos_x + x;
							int map_pos_y = chunk.pos_y + y;

							int id = chunk.data_layers[l].ids[y * chunk.size_x + x];
							bool flip_horiz = ((uint)id & 0x80000000) == 0x80000000;
							bool flip_vert = ((uint)id & 0x40000000) == 0x40000000;
							bool flip_diag = ((uint)id & 0x20000000) == 0x20000000;
							if (flip_diag)
							{
								bool temp = flip_horiz;
								flip_horiz = flip_vert ^ true;
								flip_vert = temp;
							}
							id &= 0x1FFFFFFF;

							TileSet tile_set = tile_map.library.GetTileSetAndIndex(ref id);

							if (tile_set == null)
								continue;

							Sprite use_sprite = tile_set.tiles[id];
							int offset_x = tile_set.offset_x;
							int offset_y = tile_set.offset_y;

							GameObject new_sprite = new GameObject(x + "_" + y);

							Transform sprite_transform = new_sprite.GetComponent<Transform>();
							sprite_transform.parent = layer_transform;
							sprite_transform.localPosition =
								new Vector3(
									tile_map.GetXCoordinate(map_pos_x, map_pos_y) + offset_x,
									tile_map.GetYCoordinate(map_pos_x, map_pos_y) + offset_y,
									0.0f
								);
							sprite_transform.localScale = new Vector3((flip_horiz ? -1.0f : 1.0f), (flip_vert ? -1.0f : 1.0f), 1.0f);
							sprite_transform.localRotation = Quaternion.Euler(0, 0, (flip_diag ? 90 : 0));

							SpriteRenderer sr = new_sprite.AddComponent<SpriteRenderer>();
							sr.sprite = use_sprite;
							sr.sortingOrder = tile_map.GetZCoordinate(map_pos_x, map_pos_y);
							sr.color = new Color(1.0f, 1.0f, 1.0f, tile_map.layer_info[l].default_alpha);
						}
					}
				}

				root_transform.parent = render_root.GetComponent<Transform>();
				root_transform.localPosition = Vector3.zero;
				root_transform.localScale = Vector3.one;
			}

			// Destroys a previously-built render chunk
			public void DestroyChunk(int pos_x, int pos_y)
			{
				Transform target_transform = render_transform.Find(pos_x + "_" + pos_y);

				if (target_transform == null)
					return;

				GameObject target = target_transform.gameObject;

				Utility.GameObject.Destroy(target);
			}
		}
	}
}
