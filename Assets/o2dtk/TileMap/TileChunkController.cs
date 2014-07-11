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
			public class TileChunkEntryMap : Map<ITriple, TileChunkUpdateEntry>
			{ }
			public TileChunkEntryMap update_entries;

			// Updates the chunk and its sprites
			public void Update()
			{
				int milliseconds = (int)(Time.time * 1000);

				foreach (KeyValuePair<ITriple, TileChunkUpdateEntry> pair in update_entries)
				{
					if (controller.update_intercept == null || !controller.update_intercept.InterceptTileUpdate(this, pair.Key, pair.Value))
					{
						int id = chunk.data_layers[pair.Key.third].ids[pair.Key.second * chunk.size_x + pair.Key.first];
						TileSet tile_set = controller.tile_map.library.GetTileSetAndIndex(ref id);
						(pair.Value.user_data as SpriteRenderer).sprite = tile_set.tiles[tile_set.GetAnimatedTileIndex(id, milliseconds)];
					}
				}
			}

			// Adds a tile chunk tile entry to the update list
			public void AddUpdateEntry(int x, int y, int layer_index, TileChunkUpdateEntry entry)
			{
				update_entries.Add(new ITriple(x, y, layer_index), entry);
			}

			// Removes a tile chunk tile entry from the update list
			public void RemoveUpdateEntry(int x, int y, int layer_index)
			{
				update_entries.Remove(new ITriple(x, y, layer_index));
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

							if (controller.render_intercept == null || !controller.render_intercept.InterceptTileRender(this, layer_transform, local_position, local_rotation, local_scale, x, y, l, global_id))
								RenderTile(this, layer_transform, local_position, local_rotation, local_scale, x, y, l, global_id);
						}
					}
				}

				transform.parent = controller.chunk_root_transform;
				transform.localPosition = Vector3.zero;
				transform.localScale = Vector3.one;
			}

			// The default method in which tiles are rendered
			public static GameObject RenderTile(
				TileChunkController chunk_controller,
				Transform layer_transform,
				Vector3 local_position,
				Quaternion local_rotation,
				Vector3 local_scale,
				int local_x,
				int local_y,
				int layer_index,
				int global_id,
				bool update_animated_tiles = true
				)
			{
				int local_id = global_id;

				TileSet tile_set = chunk_controller.controller.tile_map.library.GetTileSetAndIndex(ref local_id);

				if (tile_set == null)
					return null;

				Sprite use_sprite = tile_set.tiles[local_id];
				int offset_x = tile_set.offset_x;
				int offset_y = tile_set.offset_y;

				GameObject new_sprite = new GameObject(local_x + "_" + local_y);

				Transform sprite_transform = new_sprite.GetComponent<Transform>();
				sprite_transform.parent = layer_transform;
				sprite_transform.localPosition = local_position + new Vector3(offset_x, offset_y, 0.0f);
				sprite_transform.localScale = local_scale;
				sprite_transform.localRotation = local_rotation;

				SpriteRenderer sr = new_sprite.AddComponent<SpriteRenderer>();
				sr.sprite = use_sprite;
				sr.sortingOrder = layer_index;
				sr.color = new Color(1.0f, 1.0f, 1.0f, chunk_controller.controller.tile_map.layer_info[layer_index].default_alpha);

				if (update_animated_tiles && tile_set.IsTileAnimated(local_id))
					chunk_controller.AddUpdateEntry(local_x, local_y, layer_index, new TileChunkUpdateEntry(new_sprite, sr as object));

				return new_sprite;
			}
		}
	}
}
