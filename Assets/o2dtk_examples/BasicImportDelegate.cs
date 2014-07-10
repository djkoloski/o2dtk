using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using o2dtk.TileMap;

public class BasicImportDelegate : TileMapImportDelegate
{
	public int random_int = 4;
	public override void ImportTileMapObject(TileMapObject obj, TileMap tile_map)
	{
		Debug.Log("Object '" + obj.name + "'");
		Debug.Log("Layer: '" + tile_map.layer_info[obj.layer_index].name + "'");
		Debug.Log("Position: (" + obj.position.x + "," + obj.position.y + ")");
		switch (obj.shape.type)
		{
			case TileMapShape.Type.Rectangle:
				Debug.Log("Rectangle: " + obj.shape.size.x + " x " + obj.shape.size.y);
				break;
			case TileMapShape.Type.Ellipse:
				Debug.Log("Ellipse: (" + obj.shape.size.x + " x " + obj.shape.size.y);
				break;
			case TileMapShape.Type.Polyline:
				Debug.Log("Polyline:");
				foreach (Vector2 point in obj.shape.points)
					Debug.Log("(" + point.x + "," + point.y + ")");
				break;
			case TileMapShape.Type.Polygon:
				Debug.Log("Polygon:");
				foreach (Vector2 point in obj.shape.points)
					Debug.Log("(" + point.x + "," + point.y + ")");
				break;
			default:
				Debug.Log("Mystery!");
				break;
		}
		Debug.Log("Properties:");
		foreach (KeyValuePair<string, string> property in obj.properties)
			Debug.Log(property.Key + ": " + property.Value);
	}
}
