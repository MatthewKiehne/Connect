using BlockEvent;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EventChainLog
{
    public bool AllowLoops { get; private set; }
    public BidirectionalGraph<EventData> SourceGraph { get; private set; }
    public BidirectionalGraphNode<EventData> CurrentNode { get; set; }
    private int _index = 0;

    public EventChainLog(object source, object data, string eventName, bool allowLoops)
    {
        AllowLoops = allowLoops;
        _index = 0;

        EventData eventData = CreateEventData(source, data, eventName);
        CurrentNode = new BidirectionalGraphNode<EventData>(eventData);
        SourceGraph = new BidirectionalGraph<EventData>(CurrentNode);
    }

    public void AddEventToCurrent(object source, object data, string eventName)
    {
        if (SourceInHierarchy(source, CurrentNode))
        {
            throw new Exception("Data already exists in the graph");
        }

        EventData eventData = CreateEventData(source, data, eventName);
        BidirectionalGraphNode<EventData> node = new BidirectionalGraphNode<EventData>(eventData);
        SourceGraph.ConnectNodes(CurrentNode, node);

        CurrentNode = node;
    }

    private EventData CreateEventData(object source, object data, string eventName)
    {
        EventData eventData = new EventData();
        eventData.Source = source;
        eventData.Data = data;
        eventData.Event = eventName;
        eventData.Index = _index;

        _index++;

        return eventData;
    }

    private bool SourceInHierarchy(object source, BidirectionalGraphNode<EventData> activatingNode)
    {
        if (activatingNode.Parent == null)
        {
            return false;
        }

        // we start with the parent of the activating node because activating node could have multiple listeners
        // this would cause it to already in the bidirectional graph and cause think cause it to fail if we start with the activating node
        BidirectionalGraphNode<EventData> checkingNode = activatingNode.Parent;
        while (checkingNode != null)
        {
            if (checkingNode.Data.Source.Equals(source))
            {
                return true;
            }

            checkingNode = checkingNode.Parent;
        }

        return false;
    }

    /// <summary>
    /// Disconnects the nodes and sets all values to null
    /// </summary>
    public void DestroyLogs()
    {
        HashSet<BidirectionalGraphNode<EventData>> allNodes = SourceGraph.GetAllNodesFromRoot();
        foreach (BidirectionalGraphNode<EventData> node in allNodes)
        {
            node.Data = new EventData();
            node.Parent = null;
            node.Children.Clear();
        }

        SourceGraph = null;
        CurrentNode = null;
    }
}
