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
			public TileMapController map_controller;
			// The chunk the controller renders
			public TileChunk chunk;
			// The transform of the chunk controller
			public new Transform transform;
			// The sprites that must be updated regularly
			[System.Serializable]
			public class TileChunkEntryMap : Map<int, TileChunkUpdateEntry>
			{ }
			public TileChunkEntryMap update_entries;

			// Gets the index of the tile from its chunk x, y, and layer indices
			public int GetTileIndex(int x, int y, int layer_index = 0)
			{
				return ((layer_index * chunk.size_y) + y) * chunk.size_x + x;
			}

			// Gets the chunk x, y, and layer indices from the index of a tile
			public void ReverseTileIndex(int index, out int x, out int y, out int layer_index)
			{
				x = index % chunk.size_x;
				y = index / chunk.size_x % chunk.size_y;
				layer_index = index / chunk.size_x / chunk.size_y;
			}

			// Updates the chunk and its sprites
			public void Update()
			{
				int milliseconds = (int)(Time.time * 1000);

				int x = 0;
				int y = 0;
				int layer_index = 0;
				foreach (KeyValuePair<int, TileChunkUpdateEntry> pair in update_entries)
				{
					ReverseTileIndex(pair.Key, out x, out y, out layer_index);
					if (map_controller.update_intercept == null || !map_controller.update_intercept.InterceptTileUpdate(this, x, y, layer_index, pair.Value))
					{
						int id = chunk.data_layers[layer_index].ids[GetTileIndex(x, y)];
						TileSet tile_set = map_controller.tile_map.library.GetTileSetAndIndex(ref id);
						(pair.Value.user_data as SpriteRenderer).sprite = tile_set.tiles[tile_set.GetAnimatedTileIndex(id, milliseconds)];
					}
				}
			}

			// Adds a tile chunk tile entry to the update list
			public void AddUpdateEntry(int x, int y, int layer_index, TileChunkUpdateEntry entry)
			{
				update_entries.Add(GetTileIndex(x, y, layer_index), entry);
			}

			// Removes a tile chunk tile entry from the update list
			public void RemoveUpdateEntry(int x, int y, int layer_index)
			{
				update_entries.Remove(GetTileIndex(x, y, layer_index));
			}

			// Initializes the chunk controller
			public void Initialize(TileMapController tile_map_controller, TileChunk tile_map_chunk)
			{
				map_controller = tile_map_controller;
				chunk = tile_map_chunk;

				transform = GetComponent<Transform>();
				update_entries = new TileChunkEntryMap();

				for (int l = 0; l < chunk.data_layers.Count; ++l)
				{
					GameObject layer_root = new GameObject(map_controller.tile_map.layer_info[l].name);
					Transform layer_transform = layer_root.GetComponent<Transform>();
					layer_transform.parent = transform;
					layer_transform.localPosition = Vector3.zero;
					layer_transform.localScale = Vector3.one;

					for (int y = 0; y < chunk.size_y; ++y)
					{
						for (int x = 0; x < chunk.size_x; ++x)
						{
							int global_id = chunk.data_layers[l].ids[y * chunk.size_x + x];
							bool flip_horiz = ((uint)global_id & 0x80000000) == 0x80000000;
							bool flip_vert = ((uint)global_id & 0x40000000) == 0x40000000;
							bool flip_diag = ((uint)global_id & 0x20000000) == 0x20000000;
							if (flip_diag)
							{
								bool temp = flip_horiz;
								flip_horiz = !flip_vert;
								flip_vert = temp;
							}
							global_id &= 0x1FFFFFFF;

							Vector3 scale = new Vector3((flip_horiz ? -1.0f : 1.0f), (flip_vert ? -1.0f : 1.0f), 1.0f) / map_controller.pixels_per_unit;
							Quaternion rotation = Quaternion.Euler(0, 0, (flip_diag ? 90 : 0));

							if (map_controller.render_intercept == null || !map_controller.render_intercept.InterceptTileRender(this, layer_transform, rotation, scale, x, y, l, global_id))
								RenderTile(this, layer_transform, rotation, scale, x, y, l, global_id);
						}
					}
				}

				transform.parent = map_controller.chunk_root_transform;
				transform.localPosition = Vector3.zero;
				transform.localScale = Vector3.one;
			}

			// The default method in which tiles are rendered
			public static GameObject RenderTile(
				TileChunkController chunk_controller,
				Transform layer_transform,
				Quaternion rotation,
				Vector3 scale,
				int local_x,
				int local_y,
				int layer_index,
				int global_id,
				bool update_animated_tiles = true
				)
			{
				TileMapController mc = chunk_controller.map_controller;

				int local_id = global_id;

				TileSet tile_set = mc.tile_map.library.GetTileSetAndIndex(ref local_id);

				if (tile_set == null)
					return null;

				Sprite use_sprite = tile_set.tiles[local_id];

				GameObject new_sprite = new GameObject(local_x + "_" + local_y);
				float offset_x = tile_set.offset_x;
				float offset_y = tile_set.offset_y;

				Transform sprite_transform = new_sprite.GetComponent<Transform>();
				sprite_transform.parent = layer_transform;
				sprite_transform.localPosition =
					mc.mapToLocalMatrix.MultiplyPoint(
						mc.normalToMapMatrix.MultiplyPoint(
							mc.TileToNormalSpace(local_x + chunk_controller.chunk.pos_x, local_y + chunk_controller.chunk.pos_y) + new Vector3(0.5f, 0.5f, 0.0f)
						) + new Vector3(offset_x, offset_y, 0.0f)
					);
				sprite_transform.localScale = scale;
				sprite_transform.localRotation = rotation;

				SpriteRenderer sr = new_sprite.AddComponent<SpriteRenderer>();
				sr.sprite = use_sprite;
				sr.sortingOrder = layer_index;
				sr.color = new Color(1.0f, 1.0f, 1.0f, mc.tile_map.layer_info[layer_index].default_alpha);

				if (update_animated_tiles && tile_set.IsTileAnimated(local_id))
					chunk_controller.AddUpdateEntry(local_x, local_y, layer_index, new TileChunkUpdateEntry(new_sprite, sr as object));

				return new_sprite;
			}
		}
	}
}
