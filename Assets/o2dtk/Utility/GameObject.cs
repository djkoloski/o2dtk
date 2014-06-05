using System.Collections;

namespace o2dtk
{
	namespace Utility
	{
		public class GameObject
		{
			// Destroy the object depending on whether the application is in play mode or edit mode
			public static void Destroy(UnityEngine.GameObject obj)
			{
				if (UnityEngine.Application.isPlaying)
					UnityEngine.GameObject.Destroy(obj);
				else
					UnityEngine.GameObject.DestroyImmediate(obj);
			}
		}
	}
}
