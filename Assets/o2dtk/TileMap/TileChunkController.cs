using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using o2dtk.Collections;

namespace o2dtk
{
	namespace TileMap
	{
		public class TileChunkController : MonoBehaviour
		{
			// The tile map controller this chunk controller is under
			public TileMapController controller;
			// The chunk the controller renders
			public TileChunk chunk;
			// The transform of the chunk controller
			public new Transform transform;
			// The sprites that must be updated regularly
			[System.Serializable]
			public class TileChunkEntryMap : Map<int, TileChunkUpdateEntry>
			{ }
			public TileChunkEntryMap update_entries;

			// Gets the index of a tile given its coordinates
			public int GetTileIndex(int x, int y)
			{
				return y * chunk.size_x + x;
			}

			// Updates the chunk and its sprites
			public void Update()
			{
				int milliseconds = (int)(Time.time * 1000);

				foreach (KeyValuePair<int, TileChunkUpdateEntry> pair in update_entries)
				{
					if (controller.update_intercept == null || !controller.update_intercept.InterceptTileUpdate(this, pair.Value))
					{
						int id = pair.Value.global_id;
						TileSet tile_set = controller.tile_map.library.GetTileSetAndIndex(ref id);
						(pair.Value.user_data as SpriteRenderer).sprite = tile_set.tiles[tile_set.GetAnimatedTileIndex(id, milliseconds)];
					}
				}
			}

			// Adds a tile chunk tile entry to the update list
			public void AddUpdateEntry(TileChunkUpdateEntry entry)
			{
				update_entries.Add(GetTileIndex(entry.pos_x, entry.pos_y), entry);
			}

			// Initializes the chunk controller
			public void Initialize(TileMapController tile_map_controller, TileChunk tile_map_chunk)
			{
				controller = tile_map_controller;
				chunk = tile_map_chunk;

				transform = GetComponent<Transform>();
				update_entries = new TileChunkEntryMap();

				for (int l = 0; l < chunk.data_layers.Count; ++l)
				{
					GameObject layer_root = new GameObject(controller.tile_map.layer_info[l].name);
					Transform layer_transform = layer_root.GetComponent<Transform>();
					layer_transform.parent = transform;
					layer_transform.localPosition = new Vector3(0.0f, 0.0f, chunk.data_layers.Count - l - 1);

					for (int y = 0; y < chunk.size_y; ++y)
					{
						for (int x = 0; x < chunk.size_x; ++x)
						{
							int pos_x = chunk.pos_x + x;
							int pos_y = chunk.pos_y + y;

							int global_id = chunk.data_layers[l].ids[y * chunk.size_x + x];
							bool flip_horiz = ((uint)global_id & 0x80000000) == 0x80000000;
							bool flip_vert = ((uint)global_id & 0x40000000) == 0x40000000;
							bool flip_diag = ((uint)global_id & 0x20000000) == 0x20000000;
							if (flip_diag)
							{
								bool temp = flip_horiz;
								flip_horiz = flip_vert ^ true;
								flip_vert = temp;
							}
							global_id &= 0x1FFFFFFF;

							Vector3 local_position = controller.tile_map.GetLocalCoordinates(pos_x, pos_y);
							Vector3 local_scale = new Vector3((flip_horiz ? -1.0f : 1.0f), (flip_vert ? -1.0f : 1.0f), 1.0f);
							Quaternion local_rotation = Quaternion.Euler(0, 0, (flip_diag ? 90 : 0));

							if (controller.render_intercept == null || !controller.render_intercept.InterceptTileRender(this, layer_transform, local_position, local_rotation, local_scale, l, x, y, global_id))
							{
								int local_id = global_id;

								TileSet tile_set = controller.tile_map.library.GetTileSetAndIndex(ref local_id);

								if (tile_set == null)
									continue;

								Sprite use_sprite = tile_set.tiles[local_id];
								int offset_x = tile_set.offset_x;
								int offset_y = tile_set.offset_y;

								GameObject new_sprite = new GameObject(x + "_" + y);

								Transform sprite_transform = new_sprite.GetComponent<Transform>();
								sprite_transform.parent = layer_transform;
								sprite_transform.localPosition = local_position + new Vector3(offset_x, offset_y, 0.0f);
								sprite_transform.localScale = local_scale;
								sprite_transform.localRotation = local_rotation;

								SpriteRenderer sr = new_sprite.AddComponent<SpriteRenderer>();
								sr.sprite = use_sprite;
								sr.sortingOrder = l;
								sr.color = new Color(1.0f, 1.0f, 1.0f, controller.tile_map.layer_info[l].default_alpha);

								if (tile_set.IsTileAnimated(local_id))
									AddUpdateEntry(new TileChunkUpdateEntry(x, y, new_sprite, global_id, sr as object));
							}
						}
					}
				}

				transform.parent = controller.chunk_root_transform;
				transform.localPosition = Vector3.zero;
				transform.localScale = Vector3.one;
			}
		}
	}
}
