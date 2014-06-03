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
			private bool ready = false;
			public bool is_ready
			{
				get
				{
					return ready;
				}
			}

			public void Awake()
			{
				Begin();
			}

			// Initializes the tile map controller
			public void Begin()
			{
				if (ready)
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

				ready = true;
			}

			// Returns the tile map controller to its initial state
			public void End()
			{
				if (!ready)
					return;

#if UNITY_EDITOR
				GameObject.DestroyImmediate(render_root);
				GameObject.DestroyImmediate(chunk_root);
#else
				GameObject.Destroy(render_root);
				GameObject.Destroy(chunk_root);
#endif

				chunks = null;
				render_root = null;
				render_transform = null;
				chunk_root = null;
				chunk_transform = null;

				ready = false;
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
				root_transform.parent = render_root.GetComponent<Transform>();
				root_transform.localPosition = Vector3.zero;
				root_transform.localScale = Vector3.one;

				for (int l = 0; l < chunk.data_layers.Count; ++l)
				{
					GameObject layer_root = new GameObject(tile_map.layer_info[l].name);
					Transform layer_transform = layer_root.GetComponent<Transform>();
					layer_transform.parent = root_transform;
					layer_transform.localPosition = new Vector3(0.0f, 0.0f, chunk.data_layers.Count - l - 1);
					layer_transform.localScale = Vector3.one;

					for (int y = 0; y < chunk.size_y; ++y)
					{
						for (int x = 0; x < chunk.size_x; ++x)
						{
							Sprite use_sprite = tile_map.library.GetTileSprite(chunk.data_layers[l].ids[y * chunk.size_x + x]);

							if (use_sprite == null)
								continue;

							GameObject new_sprite = new GameObject(x + "_" + y);

							Transform sprite_transform = new_sprite.GetComponent<Transform>();
							sprite_transform.parent = layer_transform;
							sprite_transform.localPosition =
								new Vector3(
									tile_map.GetXCoordinate(chunk.pos_x + x, chunk.pos_y + y),
									tile_map.GetYCoordinate(chunk.pos_x + x, chunk.pos_y + y),
									0.0f
								);
							sprite_transform.localScale = Vector3.one;

							SpriteRenderer sr = new_sprite.AddComponent<SpriteRenderer>();
							sr.sprite = use_sprite;
						}
					}
				}
			}

			// Destroys a previously-built render chunk
			public void DestroyChunk(int pos_x, int pos_y)
			{
				Transform target_transform = render_transform.Find(pos_x + "_" + pos_y);

				if (target_transform == null)
					return;

				GameObject target = target_transform.gameObject;

#if UNITY_EDITOR
				GameObject.DestroyImmediate(target);
#else
				GameObject.Destroy(target);
#endif
			}
		}
	}
}
