using UnityEngine;
using System.Collections;

namespace o2dtk
{
	namespace Utility
	{
		public class Math
		{
			public static int FloorDivide(int a, int b)
			{
				if (b < 0)
				{
					a = -a;
					b = -b;
				}

				if (a < 0)
					return (a - b + 1) / b;
				else
					return a / b;
			}

			public static Color HSVToRGB(float h, float s = 1.0f, float v = 1.0f)
			{
				h = h * 360.0f % 360.0f;

				float c = v * s;
				float x = c * (1.0f - Mathf.Abs(h / 60.0f % 2.0f - 1.0f));
				float m = v - c;

				float rp = 0.0f;
				float gp = 0.0f;
				float bp = 0.0f;

				if (h >= 0.0f && h < 60.0f)
				{
					rp = c;
					gp = x;
				}
				else if (h >= 60.0f && h < 120.0f)
				{
					rp = x;
					gp = c;
				}
				else if (h >= 120.0f && h < 180.0f)
				{
					gp = c;
					bp = x;
				}
				else if (h >= 180.0f && h < 240.0f)
				{
					gp = x;
					bp = c;
				}
				else if (h >= 240.0f && h < 300.0f)
				{
					rp = x;
					bp = c;
				}
				else
				{
					rp = c;
					bp = x;
				}

				return new Color(rp + m, gp + m, bp + m);
			}
		}
	}
}
