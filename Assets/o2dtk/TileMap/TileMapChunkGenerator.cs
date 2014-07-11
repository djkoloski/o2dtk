using UnityEditor;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace o2dtk
{
	namespace TileMap
	{
		[System.Serializable]
		public class TileMapChunkGenerator : ScriptableObject
		{
			public virtual TileChunk GetChunk(TileMap map, int pos_x, int pos_y)
			{
				return null;
			}
		}
	}
}
