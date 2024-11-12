using BlockEvent;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UIElements;

public class StrictEventManager<T, V> where T : System.Enum where V : struct
{
    private Dictionary<T, List<EventCallback<V>>> _events = new Dictionary<T, List<EventCallback<V>>>();

    public void RegisterEvent(T eventType, UnityAction<V, EventChainLog> callback, object source)
    {
        if (!_events.ContainsKey(eventType))
        {
            _events.Add(eventType, new List<EventCallback<V>>());
        }

        EventCallback<V> callbackEvent = new EventCallback<V>();
        callbackEvent.Callback = new UnityEvent<V, EventChainLog>();
        callbackEvent.Callback.AddListener(callback);
        callbackEvent.Source = source;

        _events[eventType].Add(callbackEvent);
    }

    public void UnregisterEvent(T eventType, UnityAction<V> callback, object source)
    {
        if (!_events.ContainsKey(eventType))
        {
            return;
        }

        EventCallback<V> foundCallback = _events[eventType].Where(ec => ec.Callback.Equals(callback) && ec.Source.Equals(source)).FirstOrDefault();
        _events[eventType].Remove(foundCallback);
    }

    public void StartEventChain(T eventType, V value, object source, bool allowLoops)
    {
        if (!_events.ContainsKey(eventType))
        {
            return;
        }

        EventChainLog log = new EventChainLog(source, value, eventType.ToString(), allowLoops);
        ContinueEventChain(eventType, value, log);

        log.DestroyLogs();
    }

    public void ContinueEventChain(T eventType, V value, EventChainLog eventChainLog)
    {
        if (!_events.ContainsKey(eventType))
        {
            return;
        }

        foreach (EventCallback<V> callbackEvent in _events[eventType])
        {
            eventChainLog.AddEventToCurrent(callbackEvent.Source, value, eventType.ToString());
            BidirectionalGraphNode<EventData> currentNode = eventChainLog.CurrentNode;

            callbackEvent.Callback.Invoke(value, eventChainLog);

            eventChainLog.CurrentNode = currentNode;
        }
    }
}
