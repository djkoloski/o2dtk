namespace o2dtk
{
	public class TiledMapImportSettings
	{
		public bool slice_tilesets;
		public bool rebuild_tilesets;
		public bool load_map;
		public bool chunk_map;
		public int chunk_width;
		public int chunk_height;

		public TiledMapImportSettings()
		{
			slice_tilesets = false;
			rebuild_tilesets = false;
			load_map = false;
			chunk_map = false;
			chunk_width = 0;
			chunk_height = 0;
		}
	}
}
