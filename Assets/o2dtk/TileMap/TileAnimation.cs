using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace o2dtk
{
	namespace TileMap
	{
		[System.Serializable]
		public class TileAnimationKey
		{
			// The ID at the key
			public int id;
			// The duration of the key
			public int duration;

			// Default constructor
			public TileAnimationKey(int i = 0, int d = 0)
			{
				id = i;
				duration = d;
			}
		}

		[System.Serializable]
		public class TileAnimation
		{
			// The total length of the animation
			public int length;
			// The keyframes of the animations
			public List<TileAnimationKey> keys;

			// Default constructor
			public TileAnimation()
			{
				length = 0;
				keys = new List<TileAnimationKey>();
			}

			// Adds a new keyframe to the animation
			public void AddKey(TileAnimationKey key)
			{
				length += key.duration;
				keys.Add(key);
			}

			// Gets the keyframe given the current time in milliseconds
			public TileAnimationKey GetKeyByTime(int milliseconds)
			{
				milliseconds %= length;
				foreach (TileAnimationKey key in keys)
				{
					if (milliseconds >= key.duration)
						milliseconds -= key.duration;
					else
						return key;
				}
				return null;
			}
		}
	}
}
