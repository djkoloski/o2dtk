using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace o2dtk_kits
{
	namespace Basic
	{
		public class BasicMovement : MonoBehaviour
		{
			// The transform of the object
			public new Transform transform = null;
			// The speed of the camera in units per second;
			public float speed = 1.0f;

			void Start()
			{
				transform = GetComponent<Transform>();
			}

			void Update()
			{
				transform.position += new Vector3(Input.GetAxis("Horizontal") * speed, Input.GetAxis("Vertical") * speed, 0.0f) * Time.deltaTime;
			}
		}
	}
}
