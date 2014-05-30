using UnityEngine;
using UnityEditor;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Xml;

namespace o2dtk
{
	// A number of settings for the Open 2D toolkit
	public class Open2DSettings
	{
		// The settings in the 2D toolkit
		private Dictionary<string, string> entries_;
		// A readonly version of the entries
		public Dictionary<string, string> entries
		{
			get
			{
				return entries_;
			}
		}

		public string this[string key]
		{
			get
			{
				if (entries.ContainsKey(key))
					return entries[key];
				return "";
			}
			set
			{
				entries[key] = value;
			}
		}

		// Default constructor
		public Open2DSettings()
		{
			entries_ = new Dictionary<string, string>();
			
			entries_.Add("tilesets_root", "Assets/TileSets");
			entries_.Add("tilemaps_root", "Assets/TileMaps");
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

		// Updates the named entry in the settings and saves the settings if applicable
		public static void UpdateSettingsEntry(string key, string value)
		{
			if (settings[key] != value)
			{
				settings[key] = value;
				SaveSettings();
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
				if (input.NodeType == XmlNodeType.Element && input.Name == "entry")
					settings_[input.GetAttribute("key")] = input.GetAttribute("value");
			}

			input.Close();
		}

		// Saves the settings for the toolkit
		public static void SaveSettings()
		{
			if (settings_ == null)
				settings_ = new Open2DSettings();

			XmlWriter output = XmlWriter.Create(settings_path);

			output.WriteStartElement("settings");

			foreach (KeyValuePair<string, string> pair in settings_.entries)
			{
				output.WriteStartElement("entry");
				output.WriteAttributeString("key", pair.Key);
				output.WriteAttributeString("value", pair.Value);
				output.WriteEndElement();
			}
			
			output.WriteEndElement();

			output.Close();
		}
	}
}
