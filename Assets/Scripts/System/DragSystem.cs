using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class DragSystem : MonoBehaviour
{
    [SerializeField]
    private Canvas _canvas;

    private RectTransform _currentlySelectedDragable;
    private RectTransform _foundComponentTransform;

    // Start is called before the first frame update
    void Start()
    {
        EventManager.Input.RegisterEvent(InputEventType.Mouse0Down, DownOnElement, this);
        EventManager.Input.RegisterEvent(InputEventType.Mouse0Hold, HoldOnElement, this);
        EventManager.Input.RegisterEvent(InputEventType.Mouse0Release, ReleaseOnElement, this);
    }

    public void DownOnElement(InputEvent inputData, EventChainLog eventChainLog)
    {
        List<RaycastResult> results = InputSystem.UIElements;

        foreach (RaycastResult result in results)
        {
            Dragable dragable = result.gameObject.GetComponent<Dragable>();

            if (dragable != null)
            {
                RectTransform foundTransform = dragable.GetComponent<RectTransform>();
                RectTransform dragTransform = dragable.GetDragBody(inputData.MousePosition, _canvas);

                if (foundTransform != null && dragTransform != null)
                {
                    _currentlySelectedDragable = dragTransform;
                    _foundComponentTransform = foundTransform;

                    DragEvent dragEvent = GetDragEventFromInputEvent(inputData);
                    EventManager.Drag.ContinueEventChain(DragEventType.DownDrag, dragEvent, eventChainLog);
                    break;
                } 
            }
        }
    }

    public void HoldOnElement(InputEvent inputData, EventChainLog eventChainLog)
    {
        if (_currentlySelectedDragable != null)
        {
            DragEvent dragEvent = GetDragEventFromInputEvent(inputData);
            EventManager.Drag.ContinueEventChain(DragEventType.HoldDrag, dragEvent, eventChainLog);
        }
    }

    public void ReleaseOnElement(InputEvent inputData, EventChainLog eventChainLog)
    {
        if (_currentlySelectedDragable != null)
        {
            DragEvent dragEvent = GetDragEventFromInputEvent(inputData);
            _currentlySelectedDragable = null;
            _foundComponentTransform = null;
            EventManager.Drag.ContinueEventChain(DragEventType.ReleaseDrag, dragEvent, eventChainLog);
        }
    }

    public DragEvent GetDragEventFromInputEvent(InputEvent inputData)
    {
        return new DragEvent()
        {
            MousePosition = inputData.MousePosition,
            DragTransform = _currentlySelectedDragable,
            FoundComponentTransform = _foundComponentTransform
        };
    }
}
