using UnityEngine;
using System.Collections;

namespace o2dtk
{
	public class TileDataLayer
	{
		public uint[,] gids;

		public TileDataLayer(uint width, uint height, byte[] bytes)
		{
			gids = new uint[width, height];
			
			for (uint i = 0; i < width * height; ++i)
				gids[i % width, i / width] =
					(uint)(
						bytes[i * 4] |
						(bytes[i * 4 + 1] << 8) |
						(bytes[i * 4 + 2] << 16) |
						(bytes[i * 4 + 3] << 24)
					);
		}
	}
}
