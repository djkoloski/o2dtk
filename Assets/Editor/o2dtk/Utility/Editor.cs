using UnityEngine;
using UnityEditor;
using System.Collections;
using System.Collections.Generic;

namespace o2dtk
{
	namespace Utility
	{
		public class Editor
		{
			// Casts the mouse position on the screen onto a plane and returns the world space hit point
			//   as well as whether or not the plane was hit
			public static bool ProjectMousePosition(Vector3 normal, Vector3 position, out Vector3 hit)
			{
				Plane plane = new Plane(normal, position);
				Ray ray = HandleUtility.GUIPointToWorldRay(Event.current.mousePosition);
				float dist;

				if (plane.Raycast(ray, out dist))
				{
					hit = ray.origin + (ray.direction.normalized * dist);
					return true;
				}

				hit = new Vector3();
				return false;
			}

			// Casts the mouse position using a transform and local normal
			public static bool ProjectMousePosition(Vector3 local_normal, Transform transform, out Vector3 hit)
			{
				return ProjectMousePosition(transform.TransformDirection(local_normal), transform.position, out hit);
			}
		}
	}
}
