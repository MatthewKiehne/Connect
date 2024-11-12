using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BidirectionalGraph<V>
{
    public BidirectionalGraphNode<V> Root { get; private set; }

    public BidirectionalGraph(BidirectionalGraphNode<V> root)
    {
        Root = root;
    }

    public void ConnectNodes(BidirectionalGraphNode<V> parent, BidirectionalGraphNode<V> child)
    {
        if (parent.Children.Contains(child))
        {
            return;
        }

        DisconnectParent(child);

        child.Parent = parent;
        parent.Children.Add(child);
    }

    public void DisconnectParent(BidirectionalGraphNode<V> node)
    {
        if (node.Parent == null)
        {
            return;
        }

        node.Parent.Children.Remove(node);
        node.Parent = null;
    }

    public HashSet<BidirectionalGraphNode<V>> GetAllNodesFromRoot()
    {
        HashSet<BidirectionalGraphNode<V>> nodes = new HashSet<BidirectionalGraphNode<V>>();
        return GetAllNodesFromRoot(Root, nodes);
    }

    private HashSet<BidirectionalGraphNode<V>> GetAllNodesFromRoot(BidirectionalGraphNode<V> currentNode, HashSet<BidirectionalGraphNode<V>> existingNodes)
    {
        HashSet<BidirectionalGraphNode<V>> set = existingNodes == null ? new HashSet<BidirectionalGraphNode<V>>() : existingNodes;

        if (currentNode == null || set.Contains(currentNode))
        {
            return set;
        }

        set.Add(currentNode);

        foreach (BidirectionalGraphNode<V> child in currentNode.Children)
        {
            GetAllNodesFromRoot(child, set);
        }

        return set;
    }
}
