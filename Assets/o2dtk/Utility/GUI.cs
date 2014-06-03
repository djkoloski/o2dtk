using UnityEngine;
using UnityEditor;
using System.Collections;
using System.IO;

namespace o2dtk
{
	namespace Utility
	{
		public class GUI
		{
			// Ends the last horizontal and begins a new one
			public static void BreakHorizontal()
			{
				GUILayout.EndHorizontal();
				GUILayout.BeginHorizontal();
			}

			// Puts a label on a horizontal line
			public static void Label(string label)
			{
				GUILayout.BeginHorizontal();

				GUILayout.Label(label);

				GUILayout.EndHorizontal();
			}

			// Puts a two-valued label on a horizontal line
			public static void Label(string label, string value)
			{
				GUILayout.BeginHorizontal();

				GUILayout.Label(label);
				GUILayout.FlexibleSpace();
				GUILayout.Label(value);

				GUILayout.EndHorizontal();
			}

			// Puts a button on a horizontal line
			public static bool Button(string label)
			{
				GUILayout.BeginHorizontal();

				bool value = GUILayout.Button(label);

				GUILayout.EndHorizontal();

				return value;
			}

			// Puts a toggle on a horizontal line
			public static bool LabeledToggle(string label, bool value)
			{
				GUILayout.BeginHorizontal();

				value = GUILayout.Toggle(value, label);

				GUILayout.EndHorizontal();

				return value;
			}

			// Puts a toggle on a horizontal line
			public static bool BeginToggleGroup(string label, bool value)
			{
				return EditorGUILayout.BeginToggleGroup(label, value);
			}

			// Puts a toggle on a horizontal line
			public static void EndToggleGroup()
			{
				EditorGUILayout.EndToggleGroup();
			}

			// Draws a labeled integer field on a single horizontal line
			public static int LabeledIntField(string label, int value)
			{
				GUILayout.BeginHorizontal();

				GUILayout.Label(label);
				GUILayout.FlexibleSpace();
				value = EditorGUILayout.IntField(value);

				GUILayout.EndHorizontal();

				return value;
			}

			// Draws a labeled floating point number field on single horizontal line
			public static float LabeledFloatField(string label, float value)
			{
				GUILayout.BeginHorizontal();

				GUILayout.Label(label);
				GUILayout.FlexibleSpace();
				value = EditorGUILayout.FloatField(value);

				GUILayout.EndHorizontal();

				return value;
			}

			// Draws a labeled text field on a single horitonztal line
			public static string LabeledTextField(string label, string value)
			{
				GUILayout.BeginHorizontal();

				GUILayout.Label(label);
				GUILayout.FlexibleSpace();
				value = EditorGUILayout.TextField(value);

				GUILayout.EndHorizontal();

				return value;
			}

			// Draws a labeled object field on a single horizontal line
			public static Object LabeledObjectField(string label, Object obj, System.Type type = null, bool allow_game_objects = false)
			{
				GUILayout.BeginHorizontal();

				GUILayout.Label(label);
				GUILayout.FlexibleSpace();
				obj = EditorGUILayout.ObjectField(obj, typeof(Object), allow_game_objects);

				GUILayout.EndHorizontal();

				return obj;
			}

			// Draws an object field in the GUI and returns a valid directory object
			// If an invalid object is given, an error is displayed
			public static Object FileField(Object file)
			{
				Object obj = EditorGUILayout.ObjectField(file, typeof(Object), false);

				if (obj != file)
				{
					string path = AssetDatabase.GetAssetPath(obj);
					FileAttributes attr = File.GetAttributes(path);

					if ((attr & FileAttributes.Directory) != FileAttributes.Directory)
						return obj;
					else
						EditorUtility.DisplayDialog("Invalid input", "The given object is not a file", "OK");
				}

				return file;
			}

			// Draws a labeled object field and returns a valid directory object
			public static Object LabeledFileField(string label, Object file)
			{
				GUILayout.BeginHorizontal();

				GUILayout.Label(label);
				GUILayout.FlexibleSpace();
				file = FileField(file);

				GUILayout.EndHorizontal();

				return file;
			}

			// Draws an object field in the GUI and returns a valid directory object
			// If an invalid object is given, an error is displayed
			public static Object DirectoryField(Object dir)
			{
				Object obj = EditorGUILayout.ObjectField(dir, typeof(Object), false);

				if (obj != dir)
				{
					string path = AssetDatabase.GetAssetPath(obj);
					FileAttributes attr = File.GetAttributes(path);

					if ((attr & FileAttributes.Directory) == FileAttributes.Directory)
						return obj;
					else
						EditorUtility.DisplayDialog("Invalid input", "The given object is not a directory", "OK");
				}

				return dir;
			}

			// Draws a labeled object field and returns a valid directory object
			public static Object LabeledDirectoryField(string label, Object dir)
			{
				GUILayout.BeginHorizontal();

				GUILayout.Label(label);
				GUILayout.FlexibleSpace();
				dir = DirectoryField(dir);

				GUILayout.EndHorizontal();

				return dir;
			}
		}
	}
}
