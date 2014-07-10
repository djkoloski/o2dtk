using UnityEngine;
using UnityEditor;
using System.Collections;
using o2dtk.TileMap;

public class BasicUpdateIntercept : TileMapUpdateIntercept
{
	public float seconds_per_rainbow = 3.0f;

	public override bool InterceptTileUpdate(TileChunkController chunk_controller, TileChunkUpdateEntry entry)
	{
		SpriteRenderer sr = entry.user_data as SpriteRenderer;

		sr.color = EditorGUIUtility.HSVToRGB((Time.time % seconds_per_rainbow) / seconds_per_rainbow, 1.0f, 1.0f);

		return true;
	}
}
