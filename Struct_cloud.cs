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
        private static void Add_directoryes(string nameDrive, long idDrive)
        {
            if (!TdCloud.all_paths_to_directiryes_without_files.ContainsKey(idDrive))
                return;
            var i = 0;
            foreach (string path in TdCloud.all_paths_to_directiryes_without_files[idDrive])
            {
                GoUp();
                tree = tree.Go_children(nameDrive);
                foreach (var item in path.Split('\\'))
                {
                    if (item != "" && i < TdCloud.all_messages_directiryes_without_files_id[idDrive].Count)
                        tree = TreeNode.Go_or_create_child(tree, item, Formats.Directory, TdCloud.all_messages_directiryes_without_files_id[idDrive][i]);
                }
                i++;
            }
        }
        private static void Add_files(string nameDrive, long idDrive)
        {
            try
            {
                if (!TdCloud.all_paths_to_files_with_name.ContainsKey(idDrive))
                    return;
                for (int i = 0; i < TdCloud.all_paths_to_files_with_name[idDrive].Count; i++)
                {
                    GoUp();
                    tree = tree.Go_children(nameDrive);
                    foreach (var item in Path.GetDirectoryName(TdCloud.all_paths_to_files_with_name[idDrive][i]).Split('\\'))
                    {
                        if (item != "")
                        {
                            tree = TreeNode.Go_or_create_child(tree, item, Formats.Directory, 0);
                        }
                    }
                    if (i < TdCloud.all_messages_files_id[idDrive].Count)
                        tree = TreeNode.Go_or_create_child(tree, Path.GetFileName(TdCloud.all_paths_to_files_with_name[idDrive][i]), Formats.File, TdCloud.all_messages_files_id[idDrive][i]);
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
                SaveLongArrayToJson(TdCloud.all_messages_id[id_drive], name + "\\all_messages_id.json");
                SaveStringArrayToJson(TdCloud.all_paths_to_directiryes_without_files[id_drive], name + "\\all_paths_to_directiryes_without_files.json");
                SaveLongArrayToJson(TdCloud.all_messages_directiryes_without_files_id[id_drive], name + "\\all_messages_directiryes_without_files_id.json");
                SaveStringArrayToJson(TdCloud.all_paths_to_files_with_name[id_drive], name + "\\all_paths_to_files_with_name.json");
                SaveLongArrayToJson(TdCloud.all_messages_files_id[id_drive], name + "\\all_messages_files_id.json");
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
        public static void Open_drive(string nameDrive, long idDrive)
        {
            try
            {
                foreach (var item in Storage_drives.drives)
                {
                    GoUp();
                    var list_name_child = tree.Get_list_name_childrens();
                    if (!list_name_child.Contains(item.Value))
                    {
                        OpenJsonToLongArray(TdCloud.all_messages_id, item.Key, item.Value + "\\all_messages_id.json");
                        OpenJsonToStringArray(TdCloud.all_paths_to_directiryes_without_files, item.Key, item.Value + "\\all_paths_to_directiryes_without_files.json");
                        OpenJsonToLongArray(TdCloud.all_messages_directiryes_without_files_id, item.Key, item.Value + "\\all_messages_directiryes_without_files_id.json");
                        OpenJsonToStringArray(TdCloud.all_paths_to_files_with_name, item.Key, item.Value + "\\all_paths_to_files_with_name.json");
                        OpenJsonToLongArray(TdCloud.all_messages_files_id, item.Key, item.Value + "\\all_messages_files_id.json");
                    }
                }
            }
            catch { }
            NameDrive = nameDrive;
            //tree = new TreeNode(nameDrive, Formats.Directory, 0);
            string path = Get_path();
            Open_drive_tree(nameDrive, idDrive, path);
        }
        public static void Open_drive_tree(string nameDrive, long idDrive, string path)
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
                if (idDrive == IdDrive)
                {
                    Search_by_path(path);
                }
                else
                {
                    GoUp();
                    tree = tree.Go_children(nameDrive);
                }
                IdDrive = idDrive;
            }
            catch { }
        }
        public static void Update_tree(string nameDrive = null, long idDrive = 0)
        {
            string path = Get_path();
            long tmp_id = IdDrive;
            if (nameDrive != null)
            {
                NameDrive = nameDrive;
                IdDrive = idDrive;
            }
            GoUp();
            tree.DeleteChild(NameDrive);
            //tree = new TreeNode(nameDrive, Formats.Directory, 0);
            tree = TreeNode.Go_or_create_child(tree, NameDrive, Formats.Directory, IdDrive);
            Add_directoryes(NameDrive, IdDrive);
            Add_files(NameDrive, IdDrive);
            GoUp();
            tree = tree.Go_children(NameDrive);
            if (tmp_id == IdDrive)
                Search_by_path(path);
        }
        public static void Update_tree_another_node(string nameDriveUpdate, long idDriveUpdate)
        {
            string path = Get_path();
            GoUp();
            tree.DeleteChild(nameDriveUpdate);
            //tree = new TreeNode(name_drive, Formats.Directory, 0);
            tree = TreeNode.Go_or_create_child(tree, nameDriveUpdate, Formats.Directory, idDriveUpdate);
            Add_directoryes(nameDriveUpdate, idDriveUpdate);
            Add_files(nameDriveUpdate, idDriveUpdate);
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
        public static bool Check_exist_in_children(string nameDrive, string path, string name)
        {
            var res = false;
            List<string> allChildrensPath = new List<string>();
            var node = TreeNode.GoUp(tree);
            node = node.Go_children(nameDrive);
            TreeNode.Get_all_childrens_path(node, "", allChildrensPath);
            path = Path.Combine(nameDrive, path);
            foreach (var current_path in allChildrensPath)
            {
                var tt = Path.Combine(path, name);
                if (current_path == Path.Combine(path, name))
                {
                    res = true;
                    return res;
                }
            }
            return res;
        }
        public static string Get_path()
        {
            if (tree == null || NameDrive == null)
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
            if (tree == null || NameDrive == null)
                return null;
            List<string> allChildrensPath = new List<string>();
            TreeNode.Get_all_childrens_path(tree.Go_children(name), "", allChildrensPath);
            return allChildrensPath;
        }
        public static List<string> Get_all_paths()
        {
            if (tree == null || NameDrive == null)
                return null;
            List<string> allChildrensPath = new List<string>();
            var node = TreeNode.GoUp(tree);
            node = node.Go_children(NameDrive);
            TreeNode.Get_all_full_file_path(node, "", allChildrensPath);
            return allChildrensPath;
        }
        public static List<long> Get_all_files_ids()
        {
            if (tree == null || NameDrive == null)
                return null;
            List<long> allChildrensPath = new List<long>();
            var node = TreeNode.GoUp(tree);
            node = node.Go_children(NameDrive);
            TreeNode.Get_all_files_ids(node, allChildrensPath);
            return allChildrensPath;
        }
        public static List<string> Get_all_children_paths()
        {
            if (tree == null || NameDrive == null)
                return null;
            List<string> allChildrensPath = new List<string>();
            var node = tree;
            TreeNode.Get_all_full_file_path(node, "", allChildrensPath);
            return allChildrensPath;
        }
        public static List<long> Get_all_children_files_ids()
        {
            if (tree == null || NameDrive == null)
                return null;
            List<long> allChildrensPath = new List<long>();
            var node = tree;
            TreeNode.Get_all_files_ids(node, allChildrensPath);
            return allChildrensPath;
        }
        public static string Get_full_path_by_name_children(string name)
        {
            if (tree == null || NameDrive == null)
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
            if (tree == null || NameDrive == null)
                return null;
            List<long> allChildrensId = new List<long>();
            TreeNode.Get_all_childrens_id(tree.Go_children(name), allChildrensId);
            return allChildrensId;
        }
        public static List<long> Get_all_childrens_directoryes_id(string name)
        {
            if (tree == null || NameDrive == null)
                return null;
            if (name.Contains("\\"))
                name = Path.GetFileName(name);
            List<long> allChildrensId = new List<long>();
            TreeNode.Get_all_childrens_directoryes_id(tree.Go_children(name), allChildrensId);
            return allChildrensId;
        }
        public static string Get_full_path_by_id(long id)
        {
            if (tree == null || NameDrive == null)
                return null;
            TreeNode node = TreeNode.GoUp(tree);
            node = node.Go_children(NameDrive);
            List<string> allChildrensPath = new List<string>();
            TreeNode.Get_all_childrens_path(node, "", allChildrensPath);
            List<long> allChildrensId = new List<long>();
            TreeNode.Get_all_childrens_id(node, allChildrensId);
            for (int i = 0; i < allChildrensId.Count; i++)
            {
                if (allChildrensId[i] == id)
                    if (allChildrensPath[i] == "")
                        return "";
                    else
                    {
                        string substring = NameDrive + "\\";
                        int index = allChildrensPath[i].IndexOf(substring);
                        if (index != -1)
                        {
                            allChildrensPath[i] = allChildrensPath[i].Remove(index, substring.Length);
                        }
                        return allChildrensPath[i];
                    }
            }
            return "";
        }
        public static long Get_childdren_id_by_full_path(string path, string name)
        {
            if (tree == null || NameDrive == null)
                return 0;
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
            if (tree == null || NameDrive == null)
                return null;
            if (name.Contains("\\"))
                name = Path.GetFileName(name);
            List<long> allChildrensId = Get_all_childdrens_id(name);
            List<string> allChildrensPath = Get_all_childrens_path(name);
            Dictionary<long, string> result = new Dictionary<long, string>();
            for (int i = 0; i < allChildrensId.Count; i++)
                result.Add(allChildrensId[i], allChildrensPath[i]);
            return result;
        }
        public static List<long> Get_all_current_directory_childdrens_images_index()
        {
            if (tree == null || NameDrive == null)
                return null;

            Dictionary<string, Formats> childrens = tree.Get_childrens();
            List<long> result = new List<long>();

            foreach (var child in childrens)
            {
                if (child.Value == Formats.File)
                {
                    var path = Get_full_path_by_name_children(child.Key).TrimEnd('\\');
                    if (TdCloud.images_formats.Any(path.EndsWith) && TdCloud.all_paths_to_files_with_name[IdDrive].Contains(path))
                    {
                        result.Add(TdCloud.all_paths_to_files_with_name[IdDrive].IndexOf(path));
                    }
                }
            }

            return result;
        }
        public static List<long> Get_all_current_directory_childdrens_videos_index()
        {
            if (tree == null || NameDrive == null)
                return null;

            Dictionary<string, Formats> childrens = tree.Get_childrens();
            List<long> result = new List<long>();

            foreach (var child in childrens)
            {
                if (child.Value == Formats.File)
                {
                    var path = Get_full_path_by_name_children(child.Key).TrimEnd('\\');
                    if (TdCloud.videos_formats.Any(path.EndsWith) || TdCloud.all_paths_to_files_with_name[IdDrive].Contains(path))
                    {
                        result.Add(TdCloud.all_paths_to_files_with_name[IdDrive].IndexOf(path));
                    }
                }
            }

            return result;
        }
        public static string Get_name_children_by_id(long id)
        {
            if (tree == null || NameDrive == null)
                return "";
            var name = new List<string>();
            //var node = tree;
            /*var node = TreeNode.GoUp(tree);
            node = node.Go_children(NameDrive);*/
            TreeNode.Get_name_children_by_id(tree, id, name);
            return name[0];
        }
        public static void GoUp()
        {
            if (tree == null || NameDrive == null)
                return;
            tree = TreeNode.GoUp(tree);
        }
    }
}
