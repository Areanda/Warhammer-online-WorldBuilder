using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WorldBuilder
{
    public class Config
    {
        private string _configFile = "";
        private Dictionary<string, string> _settings = new Dictionary<string, string>();

        public Config(string configFile)
        {
            _configFile = configFile;
            LoadLocalSettings();
        }

        public void LoadLocalSettings()
        {
            if (File.Exists(_configFile))
            {
                foreach (string line in File.ReadAllLines(_configFile))
                {
                    if (!line.Trim().StartsWith(";"))
                    {
                        int index = line.IndexOf("=");

                        if (index > 0)
                        {
                            string key = line.Substring(0, index).ToUpper().Trim();
                            _settings[key] = "";
                            if (index + 1 < line.Length)
                            {
                                _settings[key] = line.Substring(index + 1);
                            }
                        }
                    }
                }
            }

        }

        public int GetInt(string key)
        {
            key = key.ToUpper();
            int retval = 0;
            if (_settings.ContainsKey(key))
                Int32.TryParse(_settings[key], out retval);

            return retval;
        }

        public int GetInt(string key, int defaultValue)
        {
            key = key.ToUpper();
            int retval = defaultValue;
            if (_settings.ContainsKey(key))
                Int32.TryParse(_settings[key], out retval);

            return retval;
        }

        public string GetString(string key)
        {
            key = key.ToUpper();
            if (_settings.ContainsKey(key))
                return _settings[key];

            return "";
        }


        public string GetString(string key, string defaultvalue)
        {
            key = key.ToUpper();
            if (_settings.ContainsKey(key))
                return _settings[key];

            return defaultvalue;
        }

    }
}
