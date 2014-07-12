using UnityEngine;
using UnityEditor;
using System.Collections;
using o2dtk.TileMap;
using o2dtk.Collections;

public class BasicUpdateIntercept : TileMapUpdateIntercept
{
	public float tile_mod = 8.0f;
	public float seconds_per_rainbow = 3.0f;

	public override bool InterceptTileUpdate(TileChunkController chunk_controller, int x, int y, int layer_index, TileChunkUpdateEntry entry)
	{
		SpriteRenderer sr = entry.user_data as SpriteRenderer;

		sr.color = EditorGUIUtility.HSVToRGB(
			((Time.time % seconds_per_rainbow) / seconds_per_rainbow + (x + y + chunk_controller.chunk.pos_x + chunk_controller.chunk.pos_y) / tile_mod) % 1.0f,
			1.0f,
			1.0f
		);

		return true;
	}
}
