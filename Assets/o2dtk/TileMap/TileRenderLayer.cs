using UnityEngine;
using System.Collections;

namespace o2dtk
{
	public class TileRenderLayer
	{
		public GameObject render_root;
		public Transform render_root_transform;

		public TileRenderLayer(string name, GameObject chunk_root)
		{
			render_root = new GameObject(name);
			render_root_transform = render_root.GetComponent<Transform>();
			render_root_transform.parent = chunk_root.GetComponent<Transform>();
			render_root_transform.localPosition = Vector3.zero;
		}

		public void AddQuad(float lx, float ly, float ux, float uy, float z, Material mat)
		{
			GameObject new_quad = GameObject.CreatePrimitive(PrimitiveType.Quad);
			Transform quad_transform = new_quad.GetComponent<Transform>();

			quad_transform.parent = render_root_transform;
			quad_transform.localPosition = new Vector3((lx + ux + 1.0f) / 2.0f, (ly + uy + 1.0f) / 2.0f, z);
			quad_transform.localScale = new Vector3(ux - lx + 1.0f, uy - ly + 1.0f, 1.0f);

			new_quad.GetComponent<MeshRenderer>().material = mat;
		}
	}
}
