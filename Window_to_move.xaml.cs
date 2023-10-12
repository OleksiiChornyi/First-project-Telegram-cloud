using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace Telegram_cloud
{
    /// <summary>
    /// Interaction logic for Window_to_move.xaml
    /// </summary>
    public partial class Window_to_move : Window
    {
        public static long move_drive_id;
        public static string move_drive_name;

        private static TreeNode tree;
        private static string nameDrive;
        private static long idDrive;

        public Window_to_move()
        {
            InitializeComponent();
            Create_tree();
            GoUp();
            TreeView_move.ItemsSource = Get_childrens(tree);
        }

        private void TreeView_move_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            var tree = sender as TreeView;
            if (tree == null || tree.SelectedItem == null)
                return;

            var node = tree.SelectedItem as TreeViewItem;
            if (node == null)
                return;
            string tooltip = node.ToolTip.ToString();
            string fullpath = node.Header.ToString();
            while (true)
            {
                node = ItemsControl.ItemsControlFromItemContainer(node) as TreeViewItem;
                if (node == null)
                    break;
                fullpath = node.Header.ToString() + "\\" + fullpath;
            }
            int index = fullpath.IndexOf("\\");
            string path;
            move_drive_id = long.Parse(tooltip);
            if (index > 0)
            {
                path = fullpath.Substring(index).TrimStart('\\');
                move_drive_name = fullpath.Substring(0, index);
            }
            else
            {
                path = "";
                move_drive_name = fullpath.Trim('\\');
            }
            MainWindow.new_text_message = path;
            this.Close();
        }
        /*public static bool Check_exist_in_children(string path, string name)
        {
            var res = false;
            List<string> all_childrens_path = new List<string>();
            GoUp();
            tree = TreeNode.Go_or_create_child(tree, move_drive_name, Formats.Directory, move_drive_id);
            TreeNode.Get_all_childrens_path(tree, "", all_childrens_path);
            path = System.IO.Path.Combine(move_drive_name, path);
            foreach (var current_path in all_childrens_path)
            {
                //foreach (var this_name in TreeNode.Get_all_childrens_name_by_path(tree, current_path))
                var tt = System.IO.Path.Combine(path, name);
                if (current_path == System.IO.Path.Combine(path, name)) // path == Path.GetDirectoryName(current_path) && this_name == name)
                {
                    res = true;
                    return res;
                }
            }
            return res;
        }*/
        private ObservableCollection<TreeViewItem> Get_childrens(TreeNode tree_node)
        {
            List<TreeNode> nodes = tree_node.Get_list_childrens();
            ObservableCollection<TreeViewItem> treeViewItem = new ObservableCollection<TreeViewItem>();
            foreach (var item in nodes)
            {
                if (item.Format == Formats.File)
                    continue;
                if (item.Id != 0)
                    idDrive = item.Id;
                treeViewItem.Add(new TreeViewItem()
                {
                    Header = item.Name,
                    ToolTip = idDrive,
                    ItemsSource = Get_childrens(item)
                });
            }
            return treeViewItem;
        }
        public void Create_tree()
        {
            tree = new TreeNode("", Formats.Directory, long.MinValue);
            foreach (var item in Storage_drives.drives)
            {
                GoUp();
                nameDrive = item.Value;
                idDrive = item.Key;
                tree = TreeNode.Go_or_create_child(tree, nameDrive, Formats.Directory, idDrive);

                string path_dir = System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, nameDrive, "all_paths_to_directiryes.json");
                string jsonData = File.ReadAllText(path_dir);
                List<string> all_paths_to_directiryes = new List<string>(JsonConvert.DeserializeObject<Dictionary<long, string>>(jsonData).Values);

                Add_directoryes(all_paths_to_directiryes);

                string path_all_paths_to_files_with_name = System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, nameDrive, "all_paths_to_files_with_name.json");
                jsonData = File.ReadAllText(path_all_paths_to_files_with_name);
                List<string> all_paths_to_files_with_name = new List<string>(JsonConvert.DeserializeObject<Dictionary<int, string>>(jsonData).Values);

                string path_all_paths_to_files = System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, nameDrive, "all_paths_to_files.json");
                jsonData = File.ReadAllText(path_all_paths_to_files);
                List<string> all_paths_to_files = new List<string>(JsonConvert.DeserializeObject<Dictionary<int, string>>(jsonData).Values);

                string path_all_messages_doc_id = System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, nameDrive, "all_messages_doc_id.json");
                jsonData = File.ReadAllText(path_all_messages_doc_id);
                List<long> all_messages_doc_id = new List<long>(JsonConvert.DeserializeObject<Dictionary<int, long>>(jsonData).Values);

                Add_files(all_paths_to_files_with_name, all_paths_to_files, all_messages_doc_id);
            }
        }
        private static void Add_directoryes(List<string> all_paths_to_directiryes)
        {
            var i = 0;
            foreach (string path in all_paths_to_directiryes)
            {
                GoUp();
                tree = TreeNode.Go_or_create_child(tree, nameDrive, Formats.Directory, idDrive);
                foreach (var item in path.Split('\\'))
                {
                    tree = TreeNode.Go_or_create_child(tree, item, Formats.Directory, 0);
                }
                i++;
            }
        }
        private static void Add_files(List<string> all_paths_to_files_with_name, List<string> all_paths_to_files, List<long> all_messages_doc_id)
        {
            try
            {
                for (int i = 0; i < all_paths_to_files_with_name.Count; i++)
                {
                    GoUp();
                    tree = TreeNode.Go_or_create_child(tree, nameDrive, Formats.Directory, idDrive);
                    foreach (var item in all_paths_to_files[i].Split('\\'))
                    {
                        if (item != "")
                        {
                            tree = TreeNode.Go_or_create_child(tree, item, Formats.Directory, 0);
                        }
                    }
                    if (i < all_messages_doc_id.Count)
                        tree = TreeNode.Go_or_create_child(tree, System.IO.Path.GetFileName(all_paths_to_files_with_name[i]), Formats.File, all_messages_doc_id[i]);
                }
            }
            catch { }
        }
        private static void GoUp()
        {
            tree = TreeNode.GoUp(tree);
        }
    }
}
