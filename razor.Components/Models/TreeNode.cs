using System.Collections.Generic;
using System.Linq;

namespace razor.Components.Models
{
    public class TreeNode : Dictionary<int, TreeNode>
    {
        public int ParentId;
        public int Id;
        public bool NodeType;
        public string? NodeName;
        public byte[]? NodeContent;

        private List<TreeNode> _files = new List<TreeNode>();
        public bool HasChildren
        {
            get { return this.Keys.Where(e => e == this.ParentId).Any(); }
        }

        public void AddOnParent(int parentId, TreeNode node)
        {
            FindParent(parentId).AddNode(node);
        }

        public void AddNode(TreeNode node)
        {
            if (node.NodeType) _files.Add(node);
            if (node.Equals(this)) return;
            this.Add(node.Id, node);
        }
        public List<TreeNode> GetFiles()
        {
            return _files;
        }
        private TreeNode FindParent(int parentid)
        {
            foreach (TreeNode node in this.Values)
            {
                if (node.HasChildren)
                {
                    FindParent(parentid);
                }
                if (node.ContainsKey(parentid)) return node;
            }
            return this;
        }
    }
}
