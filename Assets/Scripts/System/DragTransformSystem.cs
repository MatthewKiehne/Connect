using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.EventSystems;

public class DragTransformSystem : MonoBehaviour
{

    [SerializeField]
    private Canvas _canvas;

    private TransformDragable _currentlySelectedDragable;
    private Vector3 _dragOffset = Vector3.zero;

    void Start()
    {
        EventManager.Drag.RegisterEvent(DragEventType.DownDrag, DownOnElement, this);
        EventManager.Drag.RegisterEvent(DragEventType.HoldDrag, HoldOnElement, this);
        EventManager.Drag.RegisterEvent(DragEventType.ReleaseDrag, ReleaseOnElement, this);
    }

    public void DownOnElement(DragEvent dragData, EventChainLog eventChainLog)
    {
        _currentlySelectedDragable = dragData.DragTransform.GetComponent<TransformDragable>();
        if (_currentlySelectedDragable != null )
        {
            _dragOffset = Input.mousePosition - dragData.DragTransform.position;
        }
    }

    public void HoldOnElement(DragEvent dragData, EventChainLog eventChainLog)
    {
        if (_currentlySelectedDragable != null)
        {
            _currentlySelectedDragable.transform.position = Input.mousePosition - _dragOffset;
        }
    }

    public void ReleaseOnElement(DragEvent dragData, EventChainLog eventChainLog)
    {
        if (_currentlySelectedDragable != null)
        {
            _currentlySelectedDragable = null;
        }
    }
}
