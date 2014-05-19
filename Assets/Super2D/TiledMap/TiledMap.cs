using UnityEngine;
using UnityEditor;
using System.Collections;

public class TiledMap : MonoBehaviour
{
	public Object tiledMapFile = null;

	public void LoadTiledMap()
	{
		Debug.Log(AssetDatabase.GetAssetPath(tiledMapFile));
	}

	void ClearTileMap()
	{
		// ...
	}
}
