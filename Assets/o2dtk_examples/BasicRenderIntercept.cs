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

		BasicTileAnimator anim = new_sprite.AddComponent<BasicTileAnimator>();
		anim.pos_x = local_x + chunk_controller.chunk.pos_x;
		anim.pos_y = local_y + chunk_controller.chunk.pos_y;

		return true;
	}
}
