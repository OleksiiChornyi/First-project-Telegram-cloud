using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using Newtonsoft.Json;

namespace Telegram_cloud
{
    internal class Settings_json
    {
        public static void SaveDictionaryToJson(Dictionary<string, string> dictionary)
        {
            string filePath = "settings.json";
            if (!File.Exists(filePath))
            {
                File.Create(filePath).Close();
            }
            string json = JsonConvert.SerializeObject(dictionary);
            File.WriteAllText(filePath, json);
        }

        public static Dictionary<string, string> LoadDictionaryFromJson()
        {
            string filePath = "settings.json";
            if (!File.Exists(filePath))
            {
                File.Create(filePath).Close();
            }
            string json = File.ReadAllText(filePath);
            Dictionary<string, string> settings_json = JsonConvert.DeserializeObject<Dictionary<string, string>>(json);
            if (settings_json == null)
            {
                Dictionary<string, string> dictionary = new Dictionary<string, string>();
                string dictionary_json = JsonConvert.SerializeObject(dictionary);
                File.WriteAllText(filePath, dictionary_json);
                json = File.ReadAllText(filePath);
                settings_json = JsonConvert.DeserializeObject<Dictionary<string, string>>(json);
            }
            if (!settings_json.Keys.Contains("Is_autorization"))
            {
                settings_json.Add("Is_autorization", false.ToString());
                string dictionary_json = JsonConvert.SerializeObject(settings_json);
                File.WriteAllText(filePath, dictionary_json);
            }
            if (!settings_json.Keys.Contains("Current_drive_id"))
            {
                settings_json.Add("Current_drive_id", false.ToString());
                string dictionary_json = JsonConvert.SerializeObject(settings_json);
                File.WriteAllText(filePath, dictionary_json);
            }
            if (!settings_json.Keys.Contains("_scale"))
            {
                settings_json.Add("_scale", 1.ToString());
                string dictionary_json = JsonConvert.SerializeObject(settings_json);
                File.WriteAllText(filePath, dictionary_json);
            }
            if (!settings_json.Keys.Contains("open_with_one_click"))
            {
                settings_json.Add("open_with_one_click", false.ToString());
                string dictionary_json = JsonConvert.SerializeObject(settings_json);
                File.WriteAllText(filePath, dictionary_json);
            }
            return settings_json;
        }
    }
}
