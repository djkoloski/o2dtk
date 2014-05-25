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
		public TileLibrary library = null;

		// A list of the layers in the tiled map
		public List<TiledLayer> layers = null;

		// The rendering layers in the tiled map
		public List<TileRenderLayer> renderLayers = null;
		// The GameObject the rendering layers are attached to
		public GameObject render_root = null;

		public void LoadTiledMap(bool force_reload)
		{
			ClearTileMap();
			
			string tiled_map_path = AssetDatabase.GetAssetPath(tiledMapFile);
			string tiled_map_dir = Path.GetDirectoryName(tiled_map_path);

			XmlReader reader = XmlReader.Create(tiled_map_path);

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
						case "tileset":
							TileSet tileset = new TileSet();

							tileset.name = reader.GetAttribute("name");
							tileset.first_gid = uint.Parse(reader.GetAttribute("firstgid"));
							tileset.tile_width = int.Parse(reader.GetAttribute("tilewidth"));
							tileset.tile_height = int.Parse(reader.GetAttribute("tileheight"));

							string image_path = "";

							while (reader.Read())
							{
								if (reader.NodeType == XmlNodeType.EndElement && reader.Name == "tileset")
									break;

								if (reader.NodeType == XmlNodeType.Element && reader.Name == "image")
									image_path = Path.Combine(tiled_map_dir, reader.GetAttribute("source"));
							}

							tileset.MakeTilesFromImage(image_path, force_reload);

							library.AddTileSet(tileset);

							break;
						case "layer":
							string layer_name = reader.GetAttribute("name");
							int layer_width = int.Parse(reader.GetAttribute("width"));
							int layer_height = int.Parse(reader.GetAttribute("height"));
							TiledLayer layer = new TiledLayer(layer_name, layer_width, layer_height);

							while (reader.Read())
							{
								if (reader.NodeType == XmlNodeType.EndElement && reader.Name == "layer")
									break;
								
								if (reader.NodeType == XmlNodeType.Element && reader.Name == "data")
								{
									if(reader.GetAttribute("encoding") == "base64")
									{
										int length = layer_width*layer_height;
										byte[] buffer = new byte[length*4];
										reader.ReadElementContentAsBase64(buffer,0,length*4);
										for(int index=0;index<length;++index)
										{
											layer.gids[index % layer_width, layer_height - (index / layer_width) - 1] = (uint)buffer[index*4];
										}
									}
									else
									{
										uint index = 0;									
										while (reader.Read())
										{
											if (reader.NodeType == XmlNodeType.EndElement)
												if (reader.Name == "data")
													break;
										
											if (reader.NodeType == XmlNodeType.Element && reader.Name == "tile")
											{
												layer.gids[index % layer_width, layer_height - (index / layer_width) - 1] = uint.Parse(reader.GetAttribute("gid"));
												++index;
											}
										}
									}
								}
							}
							
							layers.Add(layer);
																				
							break;
						default:
							break;
					}
				}
			}

			BuildTiles();
		}

		void BuildTiles()
		{
			string progress_bar_title = "Rendering tile map";
			
			for (int i = 0; i < layers.Count; ++i)
			{
				EditorUtility.DisplayProgressBar(progress_bar_title, "Rendering '" + layers[i].name + "'", (float)(i + 1) / layers.Count);
				TileRenderLayer render = new TileRenderLayer();

				render.BuildFromLayer(library, layers[i]);
				render.ParentLayer(render_root, layers.Count - i - 1);
			}

			EditorUtility.ClearProgressBar();
		}

		void ClearTiles()
		{
			if (renderLayers != null)
				foreach (TileRenderLayer renderLayer in renderLayers)
					renderLayer.Clear();
		}

		void ClearTileMap()
		{
			ClearTiles();

			width = height = tile_width = tile_height = 0;

			library = new TileLibrary();
			layers = new List<TiledLayer>();
			renderLayers = new List<TileRenderLayer>();

			if (render_root != null)
				GameObject.DestroyImmediate(render_root);
			
			render_root = new GameObject("render_root");

			Transform render_root_transform = render_root.GetComponent<Transform>();
			render_root_transform.parent = GetComponent<Transform>();
			render_root_transform.localPosition = Vector3.zero;
		}
	}
}
