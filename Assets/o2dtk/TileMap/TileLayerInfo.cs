using UnityEngine;
using System.Collections;

namespace o2dtk
{
	public class TileLayerInfo
	{
		// The name of the layer
		public string name;
		// The width of the layer
		public uint width;
		// The height of the layer
		public uint height;

		// Basic constructor
		public TileLayerInfo(string n, uint w, uint h)
		{
			name = n;
			width = w;
			height = h;
		}
	}
}
