using UnityEngine;
using UnityEditor;
using System.Collections;

namespace o2dtk
{
	[CustomEditor(typeof(TiledMap))]
	public class TiledMapEditor : Editor
	{
		private TiledMap tiledMap_;

		private string[] gui_names = new string[] { "Info", "Load" };
		private int current_gui;
		private bool force_reload;

		public void OnEnable()
		{
			tiledMap_ = (TiledMap)target;

			current_gui = 0;
			force_reload = false;
		}

		void SpacedLabel(string label, string value)
		{
			GUILayout.BeginHorizontal();
			GUILayout.Label(label);
			GUILayout.FlexibleSpace();
			GUILayout.Label(value);
			GUILayout.EndHorizontal();
		}

		void InfoGUI()
		{
			SpacedLabel("Width:", tiledMap_.width + " tiles");
			SpacedLabel("Height:", tiledMap_.height + " tiles");
			SpacedLabel("Tile Width:", tiledMap_.tile_width + " pixels");
			SpacedLabel("Tile Height:", tiledMap_.tile_height + " pixels");
		}

		void LoadGUI()
		{
			GUILayout.BeginHorizontal();
			GUILayout.Label("Tiled map:");
			GUILayout.FlexibleSpace();
			tiledMap_.tiledMapFile = EditorGUILayout.ObjectField(tiledMap_.tiledMapFile, typeof(Object), true);
			GUILayout.EndHorizontal();

			GUILayout.BeginHorizontal();
			force_reload = GUILayout.Toggle(force_reload, "Force reload");
			GUILayout.FlexibleSpace();
			if (GUILayout.Button("Load tiled map"))
			{
				bool load = true;

				if (force_reload)
					if (!EditorUtility.DisplayDialog("Force tile map reload", "This may take a while. Force a full reload of all tiles?", "Reload", "Cancel"))
						load = false;
				
				if (load)
					tiledMap_.LoadTiledMap(force_reload);
			}
			GUILayout.EndHorizontal();
		}

		public override void OnInspectorGUI()
		{
			current_gui = GUILayout.SelectionGrid(current_gui, gui_names, gui_names.Length);

			switch (current_gui)
			{
				case 0:
					InfoGUI();
					break;
				case 1:
					LoadGUI();
					break;
				default:
					break;
			}
		}
	}
}
