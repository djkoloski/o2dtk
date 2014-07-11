using UnityEngine;
using System.Collections;
using o2dtk.TileMap;
using o2dtk.Collections;

public class BasicChunkGenerator : TileMapChunkGenerator
{
	[System.Serializable]
	public class ChunkMap : Map<IPair, TileChunk>
	{ }

	private ChunkMap chunks = new ChunkMap();

	public override TileChunk GetChunk(TileMap tile_map, int pos_x, int pos_y)
	{
		IPair index = new IPair(pos_x, pos_y);

		if (!chunks.Contains(index))
		{
			TileChunk chunk = new TileChunk();
			chunks.Add(index, chunk);
		}

		return chunks[index];
	}
}
