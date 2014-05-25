using UnityEngine;
using System.Collections;

namespace o2dtk
{
	public class TileRenderLayer
	{
		public GameObject go_layer;
		public Transform go_transform;
		
		public TileRenderLayer()
		{
			go_layer = null;
			go_transform = null;
		}

		public void ParentLayer(GameObject parent, float z_depth)
		{
			if (!go_transform)
				return;
			
			go_transform.parent = parent.GetComponent<Transform>();
			go_transform.localPosition = new Vector3(0.0f, 0.0f, z_depth);
		}

		public void BuildFromLayer(TileLibrary library, TiledLayer layer, int chunk_width, int chunk_height)
		{
			Clear();

			if(chunk_width == 0 || chunk_height == 0)
			{
				chunk_width = layer.width;
				chunk_height = layer.height;
			}

			go_layer = new GameObject(layer.name);
			go_transform = go_layer.GetComponent<Transform>();

			//Create all of our chunks
			int chunk_x = layer.width / chunk_width; 
			int chunk_y = layer.height / chunk_height;
			if (layer.width % chunk_width != 0)
				++chunk_x;
			if (layer.height % chunk_height != 0)
				++chunk_y;
				
			GameObject[,] chunks = new GameObject[chunk_x, chunk_y];
			for (int y = 0; y < chunk_y; ++y)
			{
				for (int x = 0; x < chunk_x; ++x)
				{
					chunks[x,y] = new GameObject("Chunk_" + x + '_' + y);
					chunks[x,y].GetComponent<Transform>().parent = go_transform;
				}
			}

			// TODO optimize building quads

			for (int y = 0; y < layer.height; ++y)
			{
				for (int x = 0; x < layer.width; ++x)
				{
					uint gid = layer.gids[x,y];

					Material mat = library.GetMaterialByGID(gid);

					if (!mat)
						continue;

					GameObject quad = GameObject.CreatePrimitive(PrimitiveType.Quad);
					MeshRenderer mr = quad.GetComponent<MeshRenderer>();
					mr.material = mat;
					Transform quad_transform = quad.GetComponent<Transform>();
					quad_transform.parent = chunks[x / chunk_width, y / chunk_height].GetComponent<Transform>();
					quad_transform.localPosition = new Vector3(x + 0.5f, y + 0.5f, 0.0f);
				}
			}
		}

		public void Clear()
		{
			if (go_layer != null)
				GameObject.DestroyImmediate(go_layer);
		}
	}
}
