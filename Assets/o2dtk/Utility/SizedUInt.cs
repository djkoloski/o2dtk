using UnityEngine;
using UnityEditor;
using System.IO;

namespace o2dtk
{
	namespace Utility
	{
		class SizedUInt
		{
			// Chooses an indexing type based on the width and height of a chunk
			public static uint GetIndexSize(uint max)
			{
				if (max <= 0xFF)
					return 1;
				if (max <= 0xFFFF)
					return 2;
				return 4;
			}

			// Reads a 32-, 16-, or 8-bit unsigned integer from the binary reader depending on the index size
			public static uint ReadSizedUInt(BinaryReader input, uint index_size)
			{
				switch (index_size)
				{
					case 1:
						return input.ReadByte();
					case 2:
						return input.ReadUInt16();
					case 4:
						return input.ReadUInt32();
					default:
						return input.ReadUInt32();
				}
			}

			// Writes a 32-, 16-, or 8-bit unsigned integer to the binary writer depending on the index size
			public static void WriteSizedUInt(uint value, BinaryWriter output, uint index_size)
			{
				switch (index_size)
				{
					case 1:
						output.Write((byte)value);
						break;
					case 2:
						output.Write((byte)(value & 0xFF));
						output.Write((byte)(value >> 8));
						break;
					case 4:
						output.Write((uint)value);
						break;
					default:
						output.Write((uint)value);
						break;
				}
			}
		}
	}
}
