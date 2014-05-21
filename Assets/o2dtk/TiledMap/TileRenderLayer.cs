using UnityEngine;
using System.Collections;

namespace o2dtk
{
	public class TileRenderLayer
	{
		public GameObject go_layer;
		
		public TileRenderLayer()
		{
			go_layer = null;
		}

		public void ParentLayer(GameObject parent)
		{
			Transform go_transform = go_layer.GetComponent<Transform>();
			go_transform.parent = parent.GetComponent<Transform>();
			go_transform.localPosition = Vector3.zero;
		}

		public void BuildFromLayer(TileLibrary library, TiledLayer layer)
		{
			Clear();

			go_layer = new GameObject(layer.name);

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
					quad_transform.parent = go_layer.GetComponent<Transform>();
					quad_transform.localPosition = new Vector3(x + 0.5f, layer.height - y - 0.5f, 0);
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
