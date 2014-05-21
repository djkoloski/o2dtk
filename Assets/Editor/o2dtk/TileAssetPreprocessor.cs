using UnityEngine;
using UnityEditor;
using System.Collections;

namespace o2dtk
{
	public class TileAssetPreprocessor : AssetPostprocessor
	{
		void OnPreprocessTexture()
		{
			if (assetPath.ToLower().IndexOf("_tiles/") != -1)
			{
				TextureImporter tile_imp = assetImporter as TextureImporter;

				tile_imp.textureType = TextureImporterType.Advanced;
				tile_imp.textureFormat = TextureImporterFormat.AutomaticTruecolor;
				tile_imp.mipmapEnabled = false;
				tile_imp.filterMode = FilterMode.Point;
			}
		}
	}
}
