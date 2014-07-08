using UnityEditor;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace o2dtk_kits
{
	namespace Basic
	{
		[CustomEditor(typeof(BasicMovement))]
		public class BasicMovementEditor : Editor
		{
			// The BasicMovement component being edited
			public BasicMovement movement = null;

			void OnEnable()
			{
				movement = (BasicMovement)target;
			}

			public override void OnInspectorGUI()
			{
				movement.speed = o2dtk.Utility.GUI.LabeledFloatField("Speed:", movement.speed);
			}
		}
	}
}
