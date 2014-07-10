using UnityEngine;
using System.Collections;
using o2dtk.TileMap;

public class BasicRenderIntercept : TileMapRenderIntercept
{
	public Color tint_color = new Color(1.0f, 1.0f, 1.0f, 1.0f);

	public override bool InterceptTileRender(
		TileChunkController chunk_controller,
		Transform layer_transform,
		Vector3 local_position,
		Quaternion local_rotation,
		Vector3 local_scale,
		int layer_index,
		int local_x,
		int local_y,
		int global_id
		)
	{
		int local_id = global_id;

		TileSet tile_set = chunk_controller.controller.tile_map.library.GetTileSetAndIndex(ref local_id);

		if (tile_set == null)
			return true;

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
		sr.color = new Color(tint_color.r, tint_color.g, tint_color.b, chunk_controller.controller.tile_map.layer_info[layer_index].default_alpha);

		chunk_controller.AddUpdateEntry(new TileChunkUpdateEntry(local_x, local_y, new_sprite, global_id, sr as object));

		return true;
	}
}
