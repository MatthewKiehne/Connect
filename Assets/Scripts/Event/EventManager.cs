using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EventManager
{
    private static StrictEventManager<InputEventType, InputEvent> _input;
    public static StrictEventManager<InputEventType, InputEvent> Input
    {
        get
        {
            if (_input == null)
            {
                _input = new StrictEventManager<InputEventType, InputEvent>();
            }
            return _input;
        }
        private set { _input = value; }
    }

    private static StrictEventManager<DragEventType, DragEvent> _drag;
    public static StrictEventManager<DragEventType, DragEvent> Drag
    {
        get
        {
            if (_drag == null)
            {
                _drag = new StrictEventManager<DragEventType, DragEvent>();
            }
            return _drag;
        }
        private set { _drag = value; }
    }

    private static StrictEventManager<ConnectEventType, ConnectEvent> _connect;
    public static StrictEventManager<ConnectEventType, ConnectEvent> Connect
    {
        get
        {
            if (_connect == null)
            {
                _connect = new StrictEventManager<ConnectEventType, ConnectEvent>();
            }
            return _connect;
        }
        private set { _connect = value; }
    }
}
