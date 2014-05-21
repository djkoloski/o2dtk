using UnityEngine;
using System.Collections;

public class TiledLayer
{
	// The 2D array of GIDs
	public uint[,] gids;
	
	// Name of the Layer
	public string name;

	// Width of Layer
	public int width;
	// Height of Layer
	public int height;

	// Constructor
	public TiledLayer(string n, int w, int h)
	{
		name = n;
		width = w;
		height = h;
		gids = new uint[w,h];
	}
}
