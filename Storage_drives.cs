using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace Telegram_cloud
{
    class Storage_drives
    {
        public static Dictionary<long, string> drives = new Dictionary<long, string>();
        public static string filePath = "drives.json";
        private static void Read_drives()
        {
            Dictionary<long, string> data = new Dictionary<long, string>();
            try
            {
                if (!File.Exists(filePath))
                {
                    File.Create(filePath);
                }
                else
                {
                    string jsonData = File.ReadAllText(filePath);
                    data = JsonConvert.DeserializeObject<Dictionary<long, string>>(jsonData);
                }
            }
            catch { }
            if (data != null)
                drives = data;
        }
        private static void Save_drives()
        {
            try
            {
                File.WriteAllText(filePath, string.Empty);
                string jsonData = JsonConvert.SerializeObject(drives);
                File.WriteAllText(filePath, jsonData);
            }
            catch (IOException e)
            {
                if (e.GetType() == typeof(System.IO.IOException))
                    return;
                MessageBox.Show("Error with saving drives" + e.Message);
            }
        }
        public static void Add_drives(string s, long id)
        {
            if (drives.ContainsKey(id))
                MessageBox.Show("This drive allredy exists");
            else
                drives.Add(id, s);
            Save_drives();
        }
        public static void Delete_drives(long id)
        {
            if (!drives.ContainsKey(id))
                MessageBox.Show("This drive don`t exists");
            else
                drives.Remove(id);
            Save_drives();
        }
        public static void Update_drives()
        {
            Dictionary<long, string> remove_items = new Dictionary<long, string>();
            TdCloud.Find_id_channels();
            while (TdCloud.all_chats_id.Count == 0)
                TdCloud.Find_id_channels();
            Read_drives();
            foreach (var item in drives)
            {
                if (!TdCloud.all_chats_id.Contains(item.Key))
                {
                    remove_items.Add(item.Key, item.Value);
                }
            }
            foreach (var item in remove_items)
            {
                drives.Remove(item.Key);
                if (Directory.Exists(item.Value))
                {
                    try
                    {
                        Directory.Delete(item.Value, true);
                    }
                    catch { }
                }
            }
            Save_drives();
        }
        /*public static long Find_id_from_name(string name)
        {
            Read_drives();
            return drives.FirstOrDefault(x => x.Value == name).Key;
        }*/
    }

}