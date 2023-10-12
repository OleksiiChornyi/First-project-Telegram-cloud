using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Telegram_cloud
{
    public enum Formats
    {
        Directory,
        File
    }
    class TreeNode
    {
        public string Name { get; set; }
        public long Id{ get; set; }
        public Formats Format { get; set; }
        public TreeNode Parent { get; set; }
        public List<TreeNode> Children { get; set; }
        public TreeNode(string name, Formats format, long id)
        {
            Name = name;
            Format = format;
            Children = new List<TreeNode>();
            Parent = null;
            Id = id;
        }

        public void AddChild(TreeNode child)
        {
            Children.Add(child);
            child.Parent = this;
        }
        public Dictionary<string, Formats> Get_childrens()
        {
            Dictionary<string, Formats> childrens = new Dictionary<string, Formats>();
            foreach (TreeNode child in this.Children)
                childrens.Add(child.Name, child.Format);
            return childrens;
        }
        public List<TreeNode> Get_list_childrens()
        {
            List<TreeNode> childrens = new List<TreeNode>();
            foreach (TreeNode child in this.Children)
                childrens.Add(child);
            return childrens;
        }
        public List<string> Get_list_name_childrens()
        {
            List<string> childrens = new List<string>();
            foreach (TreeNode child in this.Children)
                childrens.Add(child.Name);
            return childrens;
        }
        public TreeNode Go_children(string name)
        {
            foreach(var item in this.Children)
            {
                if (item.Name == name)
                    return item;
            }
            return this;
        }
        public TreeNode DeleteChild(string name)
        {
            TreeNode childToRemove = null;
            foreach (var item in this.Children)
            {
                if (item.Name == name)
                {
                    childToRemove = item;
                    break;
                }
            }
            if (childToRemove != null)
            {
                this.Children.Remove(childToRemove);
            }

            return this;
        }

        public string Get_path()
        {
            string s = "";
            if (this.Parent != null)
            {
                s = this.Parent.Get_path();
                s += this.Name + "\\";
            }
            return s;
        }
        public static TreeNode Go_or_create_child(TreeNode root, string name_child, Formats format, long id)
        {
            foreach (var roots in root.Children)
                if (roots.Name.Equals(name_child))
                    return roots;
            var tmp = new TreeNode(name_child, format, id);
            root.AddChild(tmp);
            return tmp;
        }
        public static void Get_all_childrens_path(TreeNode node, string path, List<string> paths)
        {
            if (node == null)
                return;
            path += node.Name;
            if (node.Children.Count == 0)
            {
                if (node.Format == Formats.File)
                    paths.Add(path);
                return;
            }
            foreach (var child in node.Children)
            {
                Get_all_childrens_path(child, path + "\\", paths);
            }
        }
        public static void Get_all_full_path(TreeNode node, string path, List<string> paths)
        {
            if (node == null)
                return;
            path += node.Name;
            if (node.Children.Count == 0)
            {
                if (node.Format == Formats.File)
                    paths.Add(path);
                if (node.Format == Formats.Directory)
                    paths.Add(path);
                return;
            }
            foreach (var child in node.Children)
            {
                Get_all_childrens_path(child, path + "\\", paths);
            }
        }
        public static void Get_all_full_file_path(TreeNode node, string path, List<string> paths)
        {
            if (node == null)
                return;
            path += node.Name;
            if (node.Children.Count == 0)
            {
                if (node.Format == Formats.File)
                    paths.Add(path);
                return;
            }
            foreach (var child in node.Children)
            {
                Get_all_childrens_path(child, path + "\\", paths);
            }
        }
        public static void Get_all_files_ids(TreeNode node, List<long> leaves)
        {
            if (node == null)
                return;
            if (node.Children.Count == 0)
            {
                if (node.Format == Formats.File)
                    leaves.Add(node.Id);
                return;
            }

            foreach (var child in node.Children)
            {
                Get_all_childrens_id(child, leaves);
            }
        }
        public static TreeNode Go_child_by_id(TreeNode node, long id)
        {
            if (node == null)
                return null;
            TreeNode res = null;
            foreach(var child in node.Children)
            {
                if (child.Format == Formats.File && child.Id == id)
                    res = child;
                else if (child.Format == Formats.Directory)
                    res = Go_child_by_id(child, id);
                if (res != null)
                    break;
            }
            return res;
        }
        public static void Get_all_childrens_id(TreeNode node, List<long> leaves)
        {
            if (node == null)
                return;
            if (node.Children.Count == 0)
            {
                if (node.Format == Formats.File)
                    leaves.Add(node.Id);
                /*if (node.Format == Formats.Directory && node.Id != 0)
                    leaves.Add(node.Id);*/
                return;
            }

            foreach (var child in node.Children)
            {
                Get_all_childrens_id(child, leaves);
            }
        }
        public static void Get_all_childrens_directoryes_id(TreeNode node, List<long> leaves)
        {
            if (node == null)
                return;
            if (node.Format == Formats.Directory && node.Id != 0)
                leaves.Add(node.Id);
            if (node.Children.Count == 0)
            {
                return;
            }
            foreach (var child in node.Children)
            {
                Get_all_childrens_directoryes_id(child, leaves);
            }
        }
        public static void Get_all_childrens_id_with_name(TreeNode node, Dictionary<long, string> leaves)
        {
            if (node == null)
                return;
            if (node.Children.Count == 0)
            {
                if (node.Format == Formats.File)
                    leaves.Add(node.Id, node.Name);
                if (node.Format == Formats.Directory && node.Id != 0)
                    leaves.Add(node.Id, node.Name);
                return;
            }
            foreach (var child in node.Children)
            {
                Get_all_childrens_id_with_name(child, leaves);
            }
        }
        public static List<string> Get_all_childrens_name_by_path(TreeNode node, string path)
        {
            if (node == null)
                return null;
            List<string> result = new List<string>();
            foreach (var p in Path.GetDirectoryName(path).Split('\\'))
            {
                node = node.Go_children(p);
            }
            foreach (var child in node.Children)
            {
                if (child.Format == Formats.File)
                    result.Add(child.Name);
            }
            return result;
        }
        public static void Get_name_children_by_id(TreeNode node, long id, List<string> name)
        {
            if (node == null)
                return;
            if (node.Children.Count == 0)
            {
                if (node.Id == id)
                    name.Add(node.Name);
                return;
            }
            foreach (var child in node.Children)
            {
                Get_name_children_by_id(child, id, name);
            }
        }
        public static TreeNode GoUp(TreeNode root)
        {
            if (root.Parent != null)
                root = GoUp(root.Parent);
            return root;
        }
    }
}
