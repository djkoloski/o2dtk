using UnityEngine;
using System.Collections;
using o2dtk.TileMap;

public class BasicTileAnimator : MonoBehaviour
{
	public static float tile_mod = 8.0f;
	public static float seconds_per_rainbow = 3.0f;

	public int pos_x;
	public int pos_y;

	private SpriteRenderer sprite_renderer = null;

	void Awake()
	{
		sprite_renderer = GetComponent<SpriteRenderer>();
	}

	void Update()
	{
		sprite_renderer.color = o2dtk.Utility.Math.HSVToRGB(
			(Time.time / seconds_per_rainbow + (pos_x + pos_y) / tile_mod) % 1.0f,
			1.0f, 1.0f
			);
	}
}
