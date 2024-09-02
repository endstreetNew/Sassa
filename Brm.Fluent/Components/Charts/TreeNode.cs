using System.Collections.Generic;
using System.Linq;

public class FileTreeNode : Dictionary<int, FileTreeNode>
{
    public int ParentId;
    public int Id;
    public bool NodeType;
    public string? NodeName;
    public byte[]? NodeContent;

    private List<FileTreeNode> _files = new List<FileTreeNode>();
    public bool HasChildren
    {
        get { return this.Keys.Where(e => e == this.ParentId).Any(); }
    }

    public void AddOnParent(int parentId, FileTreeNode node)
    {
        FindParent(parentId).AddNode(node);
    }

    public void AddNode(FileTreeNode node)
    {
        if (node.NodeType) _files.Add(node);
        if (node.Equals(this)) return;
        this.Add(node.Id, node);
    }
    public List<FileTreeNode> GetFiles()
    {
        return _files;
    }
    private FileTreeNode FindParent(int parentid)
    {
        foreach (FileTreeNode node in this.Values)
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
