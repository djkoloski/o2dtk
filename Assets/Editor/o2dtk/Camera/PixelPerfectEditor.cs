using UnityEditor;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace o2dtk
{
	namespace Camera
	{
		[CustomEditor(typeof(PixelPerfect))]
		public class PixelPerfectEditor : Editor
		{
			// The pixel perfect component to edit
			public PixelPerfect perfect = null;

			void OnEnable()
			{
				perfect = (PixelPerfect)target;
			}

			public override void OnInspectorGUI()
			{
				perfect.pixels_per_unit = Utility.GUI.LabeledIntField("Pixels per Unit:", perfect.pixels_per_unit);
			}
		}
	}
}
