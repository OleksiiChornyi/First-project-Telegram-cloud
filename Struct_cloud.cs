using Newtonsoft.Json;
using System;
using System.Windows;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Threading;

namespace Telegram_cloud
{
    class Struct_cloud
    {
        private static TreeNode tree = new TreeNode("", Formats.Directory, 0);
        private static string _NameDrive;
        public static string NameDrive
        {
            get { return _NameDrive; }
            set { _NameDrive = value; }
        }
        private static long _IdDrive;
        public static long IdDrive
        {
            get { return _IdDrive; }
            set { _IdDrive = value; }
        }
        private static void Add_directoryes(string name_drive, long id_drive)
        {
            if (!TdCloud.all_paths_to_directiryes.ContainsKey(id_drive))
                return;
            var i = 0;
            foreach (string path in TdCloud.all_paths_to_directiryes[id_drive])
            {
                GoUp();
                tree = tree.Go_children(name_drive);
                foreach (var item in path.Split('\\'))
                {
                    if (item != "" && i < TdCloud.all_messages_directiryes_id[id_drive].Count)
                        tree = TreeNode.Go_or_create_child(tree, item, Formats.Directory, TdCloud.all_messages_directiryes_id[id_drive][i]);
                }
                i++;
            }
        }
        private static void Add_files(string name_drive, long id_drive)
        {
            try
            {
                if (!TdCloud.all_paths_to_files_with_name.ContainsKey(id_drive))
                    return;
                for (int i = 0; i < TdCloud.all_paths_to_files_with_name[id_drive].Count; i++)
                {
                    GoUp();
                    tree = tree.Go_children(name_drive);
                    foreach (var item in Path.GetDirectoryName(TdCloud.all_paths_to_files_with_name[id_drive][i]).Split('\\'))
                    {
                        if (item != "")
                        {
                            tree = TreeNode.Go_or_create_child(tree, item, Formats.Directory, 0);
                        }
                    }
                    if (i < TdCloud.all_messages_doc_id[id_drive].Count)
                        tree = TreeNode.Go_or_create_child(tree, Path.GetFileName(TdCloud.all_paths_to_files_with_name[id_drive][i]), Formats.File, TdCloud.all_messages_doc_id[id_drive][i]);
                }
            }
            catch { }
        }
        private static void SaveStringArrayToJson(List<string> array, string filePath)
        {
            if (!File.Exists(filePath))
            {
                File.Create(filePath).Close();
            }
            Dictionary<int, string> dictionary = new Dictionary<int, string>();
            for (int i = 0; i < array.Count; i++)
            {
                dictionary.Add(i, array[i]);
            }
            string json = JsonConvert.SerializeObject(dictionary);
            try
            {
                File.WriteAllText(filePath, json);
            }
            catch (Exception ex)
            {
                if (ex is IOException)
                    return;
                MessageBox.Show("Error with saving drive to json\n" + ex.ToString());
            }
        }
        private static void SaveLongArrayToJson(List<long> array, string filePath)
        {
            if (!File.Exists(filePath))
            {
                File.Create(filePath).Close();
            }
            Dictionary<int, long> dictionary = new Dictionary<int, long>();
            for (int i = 0; i < array.Count; i++)
            {
                dictionary.Add(i, array[i]);
            }
            string json = JsonConvert.SerializeObject(dictionary);
            try
            {
                File.WriteAllText(filePath, json);
            }
            catch (Exception ex)
            {
                if (ex is IOException)
                    return;
                MessageBox.Show("Error with saving drive to json\n" + ex.ToString());
            }
        }
        public static void Save_drive(string name, long id_drive)
        {
            if (!Directory.Exists(name))
            {
                Directory.CreateDirectory(name);
            }
            try
            {
                SaveStringArrayToJson(TdCloud.all_paths[id_drive], name + "\\all_paths.json");
                SaveStringArrayToJson(TdCloud.all_paths_to_directiryes[id_drive], name + "\\all_paths_to_directiryes.json");
                SaveStringArrayToJson(TdCloud.all_paths_to_files_with_name[id_drive], name + "\\all_paths_to_files_with_name.json");
                SaveStringArrayToJson(TdCloud.all_images_paths[id_drive], name + "\\all_images_paths.json");
                SaveLongArrayToJson(TdCloud.all_messages_doc_id[id_drive], name + "\\all_messages_doc_id.json");
                SaveLongArrayToJson(TdCloud.all_messages_images_id[id_drive], name + "\\all_images_id.json");
                SaveLongArrayToJson(TdCloud.all_messages_directiryes_id[id_drive], name + "\\all_messages_directiryes_id.json");
                SaveLongArrayToJson(TdCloud.all_messages_id[id_drive], name + "\\all_messages_id.json");
            }
            catch { }
        }
        public static void Save_all_drives()
        {
            try
            {
                foreach (var item in Storage_drives.drives)
                {
                    Save_drive(item.Value, item.Key);
                }
            }
            catch { }
        }
        private static void OpenJsonToLongArray(Dictionary<long, List<long>> dict, long id, string filePath)
        {
            try
            {
                if (!File.Exists(filePath))
                    File.Create(filePath).Close();
                string json = File.ReadAllText(filePath);
                Dictionary<int, long> dictionary = JsonConvert.DeserializeObject<Dictionary<int, long>>(json);


                if (dictionary == null)
                    return;

                dict[id] = new List<long>(dictionary.Values);

            }
            catch(Exception ex)
            {
                if (ex.GetType() == typeof(DirectoryNotFoundException))
                    return;
                //MessageBox.Show(ex.ToString());
            }
        }
        private static void OpenJsonToStringArray(Dictionary<long, List<string>> dict, long id, string filePath)
        {
            try
            {
                if (!File.Exists(filePath))
                    File.Create(filePath).Close();
                string json = File.ReadAllText(filePath);
                Dictionary<int, string> dictionary = JsonConvert.DeserializeObject<Dictionary<int, string>>(json);

                if (dictionary == null)
                    return;

                dict[id] = new List<string>(dictionary.Values);

            }
            catch (Exception ex)
            {
                if (ex.GetType() == typeof(DirectoryNotFoundException))
                    return;
                //MessageBox.Show(ex.ToString());
            }
        }
        public static void Open_drive(string name_drive, long id_drive)
        {
            try
            {
                foreach (var item in Storage_drives.drives)
                {
                    GoUp();
                    var list_name_child = tree.Get_list_name_childrens();
                    if (!list_name_child.Contains(item.Value))
                    {
                        OpenJsonToStringArray(TdCloud.all_paths, item.Key, item.Value + "\\all_paths.json");
                        OpenJsonToStringArray(TdCloud.all_paths_to_directiryes, item.Key, item.Value + "\\all_paths_to_directiryes.json");
                        OpenJsonToStringArray(TdCloud.all_paths_to_files_with_name, item.Key, item.Value + "\\all_paths_to_files_with_name.json");
                        OpenJsonToStringArray(TdCloud.all_images_paths, item.Key, item.Value + "\\all_images_paths.json");
                        OpenJsonToLongArray(TdCloud.all_messages_doc_id, item.Key, item.Value + "\\all_messages_doc_id.json");
                        OpenJsonToLongArray(TdCloud.all_messages_images_id, item.Key, item.Value + "\\all_images_id.json");
                        OpenJsonToLongArray(TdCloud.all_messages_directiryes_id, item.Key, item.Value + "\\all_messages_directiryes_id.json");
                        OpenJsonToLongArray(TdCloud.all_messages_id, item.Key, item.Value + "\\all_messages_id.json");
                    }
                }
            }
            catch { }
            NameDrive = name_drive;
            //tree = new TreeNode(name_drive, Formats.Directory, 0);
            string path = Get_path();
            Open_drive_tree(name_drive, id_drive, path);
        }
        public static void Open_drive_tree(string name_drive, long id_drive, string path)
        {
            try
            {
                foreach (var item in Storage_drives.drives)
                {
                    GoUp();
                    var list_name_child = tree.Get_list_name_childrens();
                    if (!list_name_child.Contains(item.Value))
                    {
                        tree = TreeNode.Go_or_create_child(tree, item.Value, Formats.Directory, item.Key);
                        Add_directoryes(item.Value, item.Key);
                        Add_files(item.Value, item.Key);
                    }
                    Save_drive(item.Value, item.Key);
                }
                if (id_drive == IdDrive)
                {
                    Search_by_path(path);
                }
                else
                {
                    GoUp();
                    tree = tree.Go_children(name_drive);
                }
                IdDrive = id_drive;
            }
            catch { }
        }
        public static void Update_tree(string name_drive = null, long id_drive = 0)
        {
            string path = Get_path();
            long tmp_id = IdDrive;
            if (name_drive != null)
            {
                NameDrive = name_drive;
                IdDrive = id_drive;
            }
            GoUp();
            tree.DeleteChild(NameDrive);
            //tree = new TreeNode(name_drive, Formats.Directory, 0);
            tree = TreeNode.Go_or_create_child(tree, NameDrive, Formats.Directory, IdDrive);
            Add_directoryes(NameDrive, IdDrive);
            Add_files(NameDrive, IdDrive);
            GoUp();
            tree = tree.Go_children(NameDrive);
            if (tmp_id == IdDrive)
                Search_by_path(path);
        }
        public static void Update_tree_another_node(string name_drive_update, long id_drive_update)
        {
            string path = Get_path();
            GoUp();
            tree.DeleteChild(name_drive_update);
            //tree = new TreeNode(name_drive, Formats.Directory, 0);
            tree = TreeNode.Go_or_create_child(tree, name_drive_update, Formats.Directory, id_drive_update);
            Add_directoryes(name_drive_update, id_drive_update);
            Add_files(name_drive_update, id_drive_update);
            GoUp();
            tree = tree.Go_children(NameDrive);
            Search_by_path(path);
        }
        public static Dictionary<string, Formats> Search_by_path(string path)
        {
            GoUp();
            tree = tree.Go_children(NameDrive);
            foreach (var item in path.Split('\\'))
            {
                tree = tree.Go_children(item);
            }
            Dictionary<string, Formats> res = tree.Get_childrens();
            return res;
        }
        public static Formats Get_format(string name)
        {
            return tree.Go_children(name).Format;
        }
        public static Dictionary<string, Formats> Search_by_child(string child)
        {
            tree = tree.Go_children(child);
            Dictionary<string, Formats> res = tree.Get_childrens();
            return res;
        }
        public static bool Check_directory_in_children(string name)
        {
            foreach (var i in tree.Get_childrens())
            {
                if (i.Value == Formats.Directory)
                    if (i.Key == name)
                        return true;
            }
            return false;
        }
        public static bool Check_exist_in_children(string name_drive, string path, string name)
        {
            var res = false;
            List<string> all_childrens_path = new List<string>();
            var node = TreeNode.GoUp(tree);
            node = node.Go_children(name_drive);
            TreeNode.Get_all_childrens_path(node, "", all_childrens_path);
            path = Path.Combine(name_drive, path);
            foreach (var current_path in all_childrens_path)
            {
                //foreach (var this_name in TreeNode.Get_all_childrens_name_by_path(tree, current_path))
                var tt = Path.Combine(path, name);
                if (current_path == Path.Combine(path, name)) // path == Path.GetDirectoryName(current_path) && this_name == name)
                {
                    res = true;
                    return res;
                }
            }
            return res;
        }
        public static string Get_path()
        {
            if (tree == null)
                return "";
            string res = tree.Get_path().TrimEnd('\\');
            int index = res.IndexOf(NameDrive);
            if (index > -1)
            {
                res = res.Substring(index + NameDrive.Length).TrimStart('\\');
            }
            return res;
        }
        public static void Get_parent()
        {
            if (tree != null && tree.Parent != null && tree.Parent.Name != "")
                tree = tree.Parent;
        }
        public static long Get_message_id(string name)
        {
            return tree.Go_children(name).Id;
        }
        public static List<string> Get_all_childrens_path(string name)
        {
            if (tree == null)
                return null;
            List<string> all_childrens_path = new List<string>();
            TreeNode.Get_all_childrens_path(tree.Go_children(name), "", all_childrens_path);
            return all_childrens_path;
        }
        public static List<string> Get_all_paths()
        {
            if (tree == null)
                return null;
            List<string> all_childrens_path = new List<string>();
            var node = TreeNode.GoUp(tree);
            node = node.Go_children(NameDrive);
            TreeNode.Get_all_full_file_path(node, "", all_childrens_path);
            return all_childrens_path;
        }
        public static List<long> Get_all_files_ids()
        {
            if (tree == null)
                return null;
            List<long> all_childrens_path = new List<long>();
            var node = TreeNode.GoUp(tree);
            node = node.Go_children(NameDrive);
            TreeNode.Get_all_files_ids(node, all_childrens_path);
            return all_childrens_path;
        }
        public static List<string> Get_all_children_paths()
        {
            if (tree == null)
                return null;
            List<string> all_childrens_path = new List<string>();
            var node = tree;
            TreeNode.Get_all_full_file_path(node, "", all_childrens_path);
            return all_childrens_path;
        }
        public static List<long> Get_all_children_files_ids()
        {
            if (tree == null)
                return null;
            List<long> all_childrens_path = new List<long>();
            var node = tree;
            TreeNode.Get_all_files_ids(node, all_childrens_path);
            return all_childrens_path;
        }
        public static string Get_full_path_by_name_children(string name)
        {
            if (tree == null)
                return null;
            var node = tree;
            var res = node.Go_children(name).Get_path();
            int index = res.IndexOf("\\");
            if (index > 0)
            {
                res = res.Substring(index + "\\".Length);
            }

            return res;
        }
        public static List<long> Get_all_childdrens_id(string name)
        {
            if (tree == null)
                return null;
            List<long> all_childrens_id = new List<long>();
            TreeNode.Get_all_childrens_id(tree.Go_children(name), all_childrens_id);
            return all_childrens_id;
        }
        public static List<long> Get_all_childrens_directoryes_id(string name)
        {
            if (tree == null)
                return null;
            if (name.Contains("\\"))
                name = Path.GetFileName(name);
            List<long> all_childrens_id = new List<long>();
            TreeNode.Get_all_childrens_directoryes_id(tree.Go_children(name), all_childrens_id);
            return all_childrens_id;
        }
        public static string Get_full_path_by_id(long id)
        {
            if (tree == null)
                return null;
            TreeNode node = TreeNode.GoUp(tree);
            node = node.Go_children(NameDrive);
            List<string> all_childrens_path = new List<string>();
            TreeNode.Get_all_childrens_path(node, "", all_childrens_path);
            List<long> all_childrens_id = new List<long>();
            TreeNode.Get_all_childrens_id(node, all_childrens_id);
            for (int i = 0; i < all_childrens_id.Count; i++)
            {
                if (all_childrens_id[i] == id)
                    if (all_childrens_path[i] == "")
                        return "";
                    else
                    {
                        string substring = NameDrive + "\\";
                        int index = all_childrens_path[i].IndexOf(substring);
                        if (index != -1)
                        {
                            all_childrens_path[i] = all_childrens_path[i].Remove(index, substring.Length);
                        }
                        return all_childrens_path[i];
                    }
            }
            return "";
        }
        public static long Get_childdren_id_by_full_path(string path, string name)
        {
            name = Path.GetFileName(name);
            TreeNode node = TreeNode.GoUp(tree);
            node = node.Go_children(NameDrive);
            foreach (var item in path.Split('\\'))
            {
                node = node.Go_children(item);
            }
            return node.Go_children(name).Id;
        }
        public static Dictionary<long, string> Get_all_childdrens_id_with_path(string name)
        {
            if (tree == null)
                return null;
            if (name.Contains("\\"))
                name = Path.GetFileName(name);
            List<long> all_childrens_id = Get_all_childdrens_id(name);
            List<string> all_childrens_path = Get_all_childrens_path(name);
            Dictionary<long, string> result = new Dictionary<long, string>();
            for (int i = 0; i < all_childrens_id.Count; i++)
                result.Add(all_childrens_id[i], all_childrens_path[i]);
            return result;
        }
        public static List<long> Get_all_current_directory_childdrens_images_index()
        {
            if (tree == null)
                return null;

            Dictionary<string, Formats> childrens = tree.Get_childrens();
            List<long> result = new List<long>();

            foreach (var child in childrens)
            {
                if (child.Value == Formats.File)
                {
                    var path = Get_full_path_by_name_children(child.Key).TrimEnd('\\');
                    if (TdCloud.images_formats.Any(path.EndsWith) && TdCloud.all_images_paths[IdDrive].Contains(path))
                    {
                        result.Add(TdCloud.all_images_paths[IdDrive].IndexOf(path));
                    }
                }
            }

            return result;
        }
        public static List<long> Get_all_current_directory_childdrens_videos_index()
        {
            if (tree == null)
                return null;

            Dictionary<string, Formats> childrens = tree.Get_childrens();
            List<long> result = new List<long>();

            foreach (var child in childrens)
            {
                if (child.Value == Formats.File)
                {
                    var path = Get_full_path_by_name_children(child.Key).TrimEnd('\\');
                    if (TdCloud.videos_formats.Any(path.EndsWith) || TdCloud.all_images_paths[IdDrive].Contains(path))
                    {
                        result.Add(TdCloud.all_images_paths[IdDrive].IndexOf(path));
                    }
                }
            }

            return result;
        }
        public static string Get_name_children_by_id(long id)
        {
            var name = new List<string>();
            //var node = tree;
            /*var node = TreeNode.GoUp(tree);
            node = node.Go_children(NameDrive);*/
            TreeNode.Get_name_children_by_id(tree, id, name);
            return name[0];
        }
        /*public static string Get_drive_name()
        {
            return NameDrive;
        }*/
        public static void GoUp()
        {
            tree = TreeNode.GoUp(tree);
        }
    }
}
