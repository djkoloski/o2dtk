using UnityEngine;
using UnityEditor;
using System.Collections;

namespace o2dtk
{
	[CustomEditor(typeof(TiledMap))]
	public class TiledMapEditor : Editor
	{
		private TiledMap tiledMap_;

		public void OnEnable()
		{
			tiledMap_ = (TiledMap)target;
		}

		public override void OnInspectorGUI()
		{
			GUILayout.BeginHorizontal();

			tiledMap_.tiledMapFile = EditorGUILayout.ObjectField(tiledMap_.tiledMapFile, typeof(Object), true);

			if (GUILayout.Button("Load tiled map"))
				tiledMap_.LoadTiledMap();

			GUILayout.EndHorizontal();
			GUILayout.BeginHorizontal();

			GUILayout.Label("Width: " + tiledMap_.width + " tiles");
			
			GUILayout.EndHorizontal();
			GUILayout.BeginHorizontal();
			
			GUILayout.Label("Height: " + tiledMap_.height + " tiles");
			
			GUILayout.EndHorizontal();
			GUILayout.BeginHorizontal();
			
			GUILayout.Label("Tile Width: " + tiledMap_.tile_width + " pixels");
			
			GUILayout.EndHorizontal();
			GUILayout.BeginHorizontal();
			
			GUILayout.Label("Tile Height: " + tiledMap_.tile_height + " pixels");

			GUILayout.EndHorizontal();
		}
	}
}
