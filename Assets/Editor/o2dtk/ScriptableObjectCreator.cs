using UnityEngine;
using UnityEditor;
using System.IO;

namespace o2dtk
{
	namespace Utility
	{
		public class ScriptableObjectCreator : EditorWindow
		{
			// The file to make an instance of a scriptable object from
			public MonoScript object_file = null;
			// The directory to put the created object in
			public Object output_dir = null;
			// The name of the scriptable object
			public string output_name = "";

			[MenuItem("Open 2D/Utility/Scriptable Object Creator")]
			public static void OpenSOCreator()
			{
				EditorWindow.GetWindow(typeof(ScriptableObjectCreator), false, "Scriptable Object Creator", true);
			}

			public void OnGUI()
			{
				MonoScript temp = Utility.GUI.LabeledMonoScriptField("Scriptable Object File:", object_file);
				if (temp != object_file)
				{
					object_file = temp;
					output_name = Path.GetFileNameWithoutExtension(AssetDatabase.GetAssetPath(object_file));
				}

				output_dir = Utility.GUI.LabeledDirectoryField("Output directory:", output_dir);

				output_name = Utility.GUI.LabeledTextField("Output name:", output_name);

				if (Utility.GUI.Button("Create"))
				{
					System.Type object_type = object_file.GetClass();
					if (object_type == null)
						EditorUtility.DisplayDialog("Invalid Scriptable Object File", "File does not contain a scriptable object as the main class.", "OK");
					else
					{
						if (!object_type.IsSubclassOf(typeof(ScriptableObject)))
							EditorUtility.DisplayDialog("Invalid Scriptable Object Class", "Main class of file does not implement ScriptableObject.", "OK");
						else
						{
							ScriptableObject new_asset = ScriptableObject.CreateInstance(object_type);
							AssetDatabase.CreateAsset(new_asset, Path.Combine(AssetDatabase.GetAssetPath(output_dir), output_name + ".asset"));
						}
					}
				}
			}
		}
	}
}
