using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BidirectionalGraphNode<V>
{
    public V Data { get; set; }
    public BidirectionalGraphNode<V> Parent { get; set; }
    public List<BidirectionalGraphNode<V>> Children { get; set; } = new List<BidirectionalGraphNode<V>>();

    public BidirectionalGraphNode(V data)
    {
        Data = data;
    }
}
