using UnityEngine;
using System.Collections;

public class BasicController : MonoBehaviour
{
	public int left = 0;
	public int right = 0;
	public int bottom = 0;
	public int top = 0;

	private o2dtk.TileMap.TileMapController controller = null;

	void Awake()
	{
		controller = GetComponent<o2dtk.TileMap.TileMapController>();
	}

	void Start()
	{
		controller.Begin();

		for (int y = bottom; y <= top; ++y)
		{
			for (int x = left; x <= right; ++x)
			{
				controller.LoadChunk(x, y);
			}
		}
	}
}
