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
				Rectangle,
				Ellipse,
				Polyline,
				Polygon
			};

			// The type of shape; indicates how to interpret the attribute data
			public Type type;
			// The points of the shape:
			//   If it's a polygon or polyline, its vertices
			//   If it's an ellipse, first its position, then width and height
			public List<Vector2> points;
			// Assuming the shape is an ellipse or rectangle, gets the dimensions of the shape
			public Vector2 size
			{
				get
				{
					if (type != Type.Ellipse && type != Type.Rectangle)
						throw new System.InvalidOperationException();
					if (points.Count < 1)
						throw new System.IndexOutOfRangeException();
					return points[0];
				}
			}

			// Basic constructor
			public TileMapShape()
			{
				type = Type.Rectangle;
				points = new List<Vector2>();
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
			// The shape of the object
			public TileMapShape shape;
			// The properties of the object
			public PropertyMap properties;

			// Basic constructor
			public TileMapObject()
			{
				name = "";
				layer_index = 0;
				position = new Vector2();
				shape = new TileMapShape();
				properties = new PropertyMap();
			}
		}
	}
}
