using UnityEngine;
using UnityEditor;
using System.Collections;
using System.Collections.Generic;
using System.Xml;
using System.IO;

namespace o2dtk
{
	public class TiledMap : MonoBehaviour
	{
		// The tiled map file to load data from
		public Object tiledMapFile = null;

		// The width of the map in tiles
		public int width;
		// The height of the map in tiles
		public int height;
		// The width of each tile in pixels
		public int tile_width;
		// The height of each tile in pixels
		public int tile_height;

		// A list of the tile sets used by the tiled map
		public List<TileSet> tileSets = null;

		public void LoadTiledMap()
		{
			ClearTileMap();
			
			string tiled_map_path = AssetDatabase.GetAssetPath(tiledMapFile);
			string tiled_map_dir = Path.GetDirectoryName(tiled_map_path);

			XmlReader reader = XmlReader.Create(tiled_map_path);

			string current_layer;

			while (reader.Read())
			{
				if (reader.NodeType == XmlNodeType.Element)
				{
					switch (reader.Name)
					{
						// Get the map attributes and check to make sure that it's orthogonal
						case "map":
							if (reader.GetAttribute("orientation") != "orthogonal")
							{
								Debug.LogError("Attempted to load tile map that was not oriented orthogonally.");
								return;
							}
							
							width = int.Parse(reader.GetAttribute("width"));
							height = int.Parse(reader.GetAttribute("height"));
							tile_width = int.Parse(reader.GetAttribute("tilewidth"));
							tile_height = int.Parse(reader.GetAttribute("tileheight"));

							break;
						case "layer":
							// TODO pick up here
							current_layer = reader.GetAttribute("name");
							break;
						case "data":
							break;
						default:
							break;
					}
				}

				if (reader.NodeType == XmlNodeType.EndElement)
				{
					// ...
				}
			}
		}

		void ClearTiles()
		{
			// ...
		}

		void ClearTileMap()
		{
			ClearTiles();

			width = height = tile_width = tile_height = 0;

			tileSets = new List<TileSet>();
		}
	}
}
