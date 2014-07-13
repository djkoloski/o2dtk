using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace o2dtk
{
	namespace Camera
	{
		public class PixelPerfect : MonoBehaviour
		{
			public int pixels_per_unit = 32;
			private Vector3 cache_position = new Vector3(0,0,0);

			void OnPreRender()
			{
				cache_position = transform.position;
				transform.position = new Vector3(Mathf.Round(transform.position.x * pixels_per_unit) / pixels_per_unit, Mathf.Round(transform.position.y * pixels_per_unit) / pixels_per_unit, transform.position.z);
			}

			void OnPostRender()
			{
				transform.position = cache_position;
			}

			void Start() 
			{	
				if (!camera.orthographic)
					camera.orthographic = true;

				camera.orthographicSize = Screen.height / 2.0f / pixels_per_unit;
			}
		}
	}
}
