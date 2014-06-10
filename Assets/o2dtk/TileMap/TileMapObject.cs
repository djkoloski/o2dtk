using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using o2dtk.Collections;

namespace o2dtk
{
	namespace TileMap
	{
		public class TileMapShape
		{
			// The types of shapes available
			public enum Type
			{
				Polygon,
				Polyline,
				Ellipse
			};

			// The type of shape; indicates how to interpret the attribute data
			public Type type;
			// The points of the shape:
			//   If it's a polygon or polyline, its vertices
			//   If it's an ellipse, first its position, then width and height
			public List<Vector2> points;
			// Assuming the shape is an ellipse, gets the position of the ellipse
			public Vector2 ellipse_pos
			{
				get
				{
					if (type != Type.Ellipse)
						throw new System.InvalidOperationException();
					if (points.Count < 1)
						throw new System.IndexOutOfRangeException();
					return points[0];
				}
			}
			// Assuming the shape is an ellipse, gets the dimensions of the ellipse
			public Vector2 ellipse_size
			{
				get
				{
					if (type != Type.Ellipse)
						throw new System.InvalidOperationException();
					if (points.Count < 2)
						throw new System.IndexOutOfRangeException();
					return points[1];
				}
			}
		}

		public class TileMapObject
		{
			// The name of the object
			public string name;
			// The index of the layer the object is on
			public int layer_index;
			// The position of the object
			public Vector2 position;
			// The shapes in the object
			public List<TileMapShape> shapes;
			// The properties of the object
			public PropertyMap properties;
		}

		public interface TileMapImportDelegate
		{
			// Takes in information about the objects in the tile map and creates
			//   user data. The given tile map object belongs to the passed tile map
			void ImportTileMapObject(TileMapObject obj, TileMap tile_map);
		}
	}
}
