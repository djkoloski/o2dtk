using UnityEngine;
using System.Collections;
using o2dtk.TileMap;

public class BasicRenderIntercept : TileMapRenderIntercept
{
	public Color tint_color = new Color(1.0f, 1.0f, 1.0f, 1.0f);

	public override bool InterceptTileRender(
		TileChunkController chunk_controller,
		Transform layer_transform,
		Quaternion rotation,
		Vector3 scale,
		int local_x,
		int local_y,
		int layer_index,
		int global_id
		)
	{
		GameObject new_sprite = TileChunkController.RenderTile(chunk_controller, layer_transform, rotation, scale, local_x, local_y, layer_index, global_id, false);
		SpriteRenderer sr = new_sprite.GetComponent<SpriteRenderer>();
		sr.color = new Color(tint_color.r, tint_color.g, tint_color.b, sr.color.a);

		chunk_controller.AddUpdateEntry(local_x, local_y, layer_index, new TileChunkUpdateEntry(new_sprite, sr as object));

		return true;
	}
}
