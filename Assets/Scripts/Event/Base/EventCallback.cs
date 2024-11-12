using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public struct EventCallback<V>
{
    public UnityEvent<V, EventChainLog> Callback;
    public object Source;
}
