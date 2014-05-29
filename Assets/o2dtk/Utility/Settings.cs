using UnityEngine;
using System.Collections;
using System.IO;
using System.Xml;

namespace o2dtk
{
	// A number of settings for the Open 2D toolkit
	public class Open2DSettings
	{
		// The directory where the tile sets are saved
		public string tilesets_root;

		// Default constructor
		public Open2DSettings()
		{
			tilesets_root = "Assets/TileSets";
		}
	}
	
	// Holds essential data about the Open 2D toolkit
	public class Open2D
	{
		// The directory where the settings file is saved
		public static string settings_path = "Assets/o2dtk/o2dtk.config";

		// The settings for the 2D toolkit
		// These are saved on a per-project basis
		public static Open2DSettings settings_ = null;
		public static Open2DSettings settings
		{
			get
			{
				if (settings_ == null)
					LoadSettings();
				return settings_;
			}
		}

		// Loads the settings for the toolkit
		// If no settings file is found, makes a new settings file and loads that
		public static void LoadSettings()
		{
			settings_ = new Open2DSettings();

			if (!File.Exists(settings_path))
				SaveSettings();

			XmlReader input = XmlReader.Create(settings_path);

			while (input.Read())
			{
				if (input.NodeType == XmlNodeType.Element)
				{
					switch (input.Name)
					{
						case "tilesetsroot":
							settings_.tilesets_root = input.GetAttribute("path");
							break;
						default:
							break;
					}
				}
			}

			input.Close();
		}

		// Saves the settings for the toolkit
		public static void SaveSettings()
		{
			if (settings_ == null)
				settings_ = new Open2DSettings();

			XmlWriter output = XmlWriter.Create(settings_path);

			output.WriteStartElement("tilesetsroot");
			output.WriteAttributeString("path", settings_.tilesets_root);
			output.WriteEndElement();

			output.Close();
		}
	}
}
