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
			// The index of the chunk
			public int index_x;
			public int index_y;
			// The tile map the chunk is rendering from
			public TileMap tile_map;
			// The chunk the controller renders
			public TileChunk chunk;
			// The transform of the chunk root
			public new Transform transform;
			// The sprites that must be updated regularly
			[System.Serializable]
			public class TileChunkEntryMap : Map<int, TileChunkTileEntry>
			{ }
			public TileChunkEntryMap entries;

			// Gets the index of a tile given its coordinates
			public int GetTileIndex(int x, int y)
			{
				return y * chunk.size_x + x;
			}

			// Updates the chunk and its sprites
			public void Update()
			{
				int milliseconds = (int)(Time.time * 1000);

				foreach (KeyValuePair<int, TileChunkTileEntry> pair in entries)
				{
					int id = pair.Value.original_id;
					TileSet tile_set = tile_map.library.GetTileSetAndIndex(ref id);
					pair.Value.sprite_renderer.sprite = tile_set.tiles[tile_set.GetAnimatedTileIndex(id, milliseconds)];
				}
			}

			// Loads and initializes the chunk
			public void LoadChunk(TileMap source_tile_map, TileChunk tile_map_chunk, int tile_map_index_x, int tile_map_index_y, Transform tile_map_chunk_transform)
			{
				tile_map = source_tile_map;
				chunk = tile_map_chunk;
				index_x = tile_map_index_x;
				index_y = tile_map_index_y;

				transform = GetComponent<Transform>();
				entries = new TileChunkEntryMap();

				for (int l = 0; l < chunk.data_layers.Count; ++l)
				{
					GameObject layer_root = new GameObject(tile_map.layer_info[l].name);
					Transform layer_transform = layer_root.GetComponent<Transform>();
					layer_transform.parent = transform;
					layer_transform.localPosition = new Vector3(0.0f, 0.0f, chunk.data_layers.Count - l - 1);

					for (int y = 0; y < chunk.size_y; ++y)
					{
						for (int x = 0; x < chunk.size_x; ++x)
						{
							int map_pos_x = chunk.pos_x + x;
							int map_pos_y = chunk.pos_y + y;

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
							int local_id = global_id;

							TileSet tile_set = tile_map.library.GetTileSetAndIndex(ref local_id);

							if (tile_set == null)
								continue;

							Sprite use_sprite = tile_set.tiles[local_id];
							int offset_x = tile_set.offset_x;
							int offset_y = tile_set.offset_y;

							GameObject new_sprite = new GameObject(x + "_" + y);

							Transform sprite_transform = new_sprite.GetComponent<Transform>();
							sprite_transform.parent = layer_transform;
							sprite_transform.localPosition = tile_map.GetLocalCoordinates(map_pos_x, map_pos_y) + new Vector2(offset_x, offset_y);
							sprite_transform.localScale = new Vector3((flip_horiz ? -1.0f : 1.0f), (flip_vert ? -1.0f : 1.0f), 1.0f);
							sprite_transform.localRotation = Quaternion.Euler(0, 0, (flip_diag ? 90 : 0));

							SpriteRenderer sr = new_sprite.AddComponent<SpriteRenderer>();
							sr.sprite = use_sprite;
							sr.sortingOrder = tile_map.GetPrecedence(map_pos_x, map_pos_y);
							sr.color = new Color(1.0f, 1.0f, 1.0f, tile_map.layer_info[l].default_alpha);

							if (tile_set.IsTileAnimated(local_id))
								entries.Add(GetTileIndex(x, y), new TileChunkTileEntry(x, y, new_sprite, global_id, sr));
						}
					}
				}

				transform.parent = tile_map_chunk_transform;
				transform.localPosition = Vector3.zero;
				transform.localScale = Vector3.one;
			}
		}
	}
}
