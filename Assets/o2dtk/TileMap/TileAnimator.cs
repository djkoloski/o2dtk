using UnityEngine;
using System.Collections;

namespace o2dtk
{
	namespace TileMap
	{
		public class TileAnimator : MonoBehaviour
		{
			public int pos_x;
			public int pos_y;
			public int layer_index;
			public TileChunkController chunk_controller;
			private SpriteRenderer sprite_renderer = null;

			public void Awake()
			{
				sprite_renderer = GetComponent<SpriteRenderer>();
			}

			public void Update()
			{
				int milliseconds = (int)(Time.time * 1000);
				int id = chunk_controller.GetTileID(pos_x, pos_y, layer_index);
				TileSet tile_set = chunk_controller.map_controller.tile_map.library.GetTileSetAndIndex(ref id);
				sprite_renderer.sprite = tile_set.tiles[tile_set.GetAnimatedTileIndex(id, milliseconds)];
			}
		}
	}
}
