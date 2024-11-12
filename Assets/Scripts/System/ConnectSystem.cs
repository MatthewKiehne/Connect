using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEditor.U2D.Aseprite;
using UnityEngine;

public class ConnectSystem : MonoBehaviour
{
    // ConnectionDragable _connectionDragable = null;
    private RectTransform _startRectTransform;

    private const float _connectionWidth = 7f;

    public List<RectTransform> Holes = new List<RectTransform>();
    private List<Connection> _connections = new List<Connection>();

    private float _minConnectionDistance = 50f;

    // Start is called before the first frame update
    void Start()
    {
        EventManager.Drag.RegisterEvent(DragEventType.DownDrag, OnConnectDown, this);
        EventManager.Drag.RegisterEvent(DragEventType.HoldDrag, OnConnectHold, this);
        EventManager.Drag.RegisterEvent(DragEventType.ReleaseDrag, OnConnectRelease, this);
    }

    public void OnConnectDown(DragEvent dragData, EventChainLog eventChainLog)
    {
        ConnectionDragable connectionDragable = dragData.FoundComponentTransform.GetComponent<ConnectionDragable>();
        if (connectionDragable == null)
        {
            return;
        }

        Connection foundConnection = _connections.Where(c => c.Start.Equals(dragData.FoundComponentTransform)).FirstOrDefault();
        if (foundConnection != null)
        {
            Destroy(foundConnection.ConnectionObject.gameObject);
            _connections.Remove(foundConnection);
        }

        _startRectTransform = dragData.FoundComponentTransform;
    }

    public void OnConnectHold(DragEvent dragData, EventChainLog eventChainLog)
    {
        // update the connections in case something else is being dragged
        foreach (Connection connection in _connections)
        {
            UpdateConnection(connection.ConnectionObject, connection.Start.position, connection.End.position);
        }

        if (_startRectTransform == null)
        {
            return;
        }

        UpdateConnection(dragData.DragTransform, _startRectTransform.position, dragData.MousePosition);
    }

    public void OnConnectRelease(DragEvent dragData, EventChainLog eventChainLog)
    {
        if (_startRectTransform == null)
        {
            return;
        }

        if (Holes.Count != 0)
        {
            List<RectTransform> ordered = Holes.OrderBy(rectTransform => Vector2.Distance(rectTransform.position, dragData.MousePosition)).ToList();
            RectTransform closest = ordered[0];
            float distance = Vector2.Distance(closest.position, dragData.MousePosition);

            if (distance < _minConnectionDistance)
            {
                UpdateConnection(dragData.DragTransform, _startRectTransform.position, closest.position);

                _connections.Add(new Connection()
                {
                    Start = _startRectTransform,
                    End = closest,
                    ConnectionObject = dragData.DragTransform
                });
            }
            else
            {
                Destroy(dragData.DragTransform.gameObject);
            }
        }

        _startRectTransform = null;
    }

    private void UpdateConnection(RectTransform dragTransform, Vector3 startPosition, Vector3 destinationPosition)
    {
        float barHeight = Vector3.Distance(startPosition, destinationPosition);
        dragTransform.sizeDelta = new Vector2(_connectionWidth, barHeight);
        dragTransform.position = startPosition + ((destinationPosition - startPosition) / 2);

        float angle = Vector2.Angle(Vector2.up, destinationPosition - startPosition);
        if (startPosition.x < destinationPosition.x)
        {
            angle = -angle;
        }

        dragTransform.rotation = Quaternion.Euler(0, 0, angle);
    }

    private class Connection
    {
        public RectTransform Start { get; set; }
        public RectTransform End { get; set; }
        public RectTransform ConnectionObject { get; set; }
    }
}
