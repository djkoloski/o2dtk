using System.Collections;
using System.Collections.Generic;
using System.IO;
using Ionic.Zlib;

namespace o2dtk
{
	namespace Utility
	{
		class Decompress
		{
			// ZLib-decompresses the requested number of bytes from the input array to the output array
			//   and returns the number of bytes decompressed
			public static int Zlib(byte[] input, byte[] output, int request)
			{
				MemoryStream stream = new MemoryStream(input);
				ZlibStream zlib = new ZlibStream(stream, CompressionMode.Decompress);
				return zlib.Read(output, 0, request);
			}

			// Gzip-decompresses the requested number of bytes from the input array to the output array
			//   and returns the number of bytes decompressed
			public static int Gzip(byte[] input, byte[] output, int request)
			{
				MemoryStream stream = new MemoryStream(input);
				GZipStream gzip = new GZipStream(stream, CompressionMode.Decompress);
				return gzip.Read(output, 0, request);
			}
		}
	}
}
