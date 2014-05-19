using UnityEngine;
using UnityEditor;
using System.Collections;

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
	}
}
