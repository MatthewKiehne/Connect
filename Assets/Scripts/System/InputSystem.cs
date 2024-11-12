using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class InputSystem : MonoBehaviour
{
    [SerializeField]
    private GraphicRaycaster _graphicRaycaster;

    [SerializeField]
    private EventSystem _eventSystem;

    public static List<RaycastResult> UIElements { get; private set; } = new List<RaycastResult>();
    // public static Vector3 MousePosition { get; private set; } = Vector3.zero;

    // Update is called once per frame
    void Update()
    {
        /*        List<RaycastResult> uiResults = new List<RaycastResult>();
                PointerEventData pointerData = new PointerEventData(EventSystem.current)
                {
                    pointerId = -1,
                };
                pointerData.position = Input.mousePosition;
                EventSystem.current.RaycastAll(pointerData, uiResults);*/


        PointerEventData pointerEventData = new PointerEventData(_eventSystem);
        pointerEventData.position = Input.mousePosition;

        List<RaycastResult> raycastResults = new List<RaycastResult>();
        _graphicRaycaster.Raycast(pointerEventData, raycastResults);


        UIElements = raycastResults;
        InputEvent inputEvent = new InputEvent()
        {
            MousePosition = Input.mousePosition
        };

        if (Input.GetKeyDown(KeyCode.Mouse0))
        {
            EventManager.Input.StartEventChain(InputEventType.Mouse0Down, inputEvent, this, false);
        }
        else if (Input.GetKey(KeyCode.Mouse0))
        {
            EventManager.Input.StartEventChain(InputEventType.Mouse0Hold, inputEvent, this, false);
        }
        else if (Input.GetKeyUp(KeyCode.Mouse0))
        {
            EventManager.Input.StartEventChain(InputEventType.Mouse0Release, inputEvent, this, false);
        }
        else if (!Input.GetKeyDown(KeyCode.Mouse0) && !Input.GetKey(KeyCode.Mouse0) && !Input.GetKeyUp(KeyCode.Mouse0))
        {
            EventManager.Input.StartEventChain(InputEventType.Mouse0Hover, inputEvent, this, false);
        }
    }
}
