using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class UIHighlightSystem : MonoBehaviour
{
    [SerializeField]
    private int _maxSearchDepth = 5;

    private UIHighlight _lastHighlighted = null;


    // Start is called before the first frame update
    void Start()
    {
        EventManager.Input.RegisterEvent(InputEventType.Mouse0Hover, CheckHighlight, this);
        EventManager.Input.RegisterEvent(InputEventType.Mouse0Down, ClearHighlight, this);
    }

    public void CheckHighlight(InputEvent data, EventChainLog log)
    {
        for (int i = 0; i < InputSystem.UIElements.Count && i < _maxSearchDepth; i++)
        {
            RaycastResult raycastResult = InputSystem.UIElements[i];

            UIHighlight hightlight = raycastResult.gameObject.GetComponent<UIHighlight>();
            if (hightlight != null)
            {
                if (_lastHighlighted != null && !_lastHighlighted.Equals(hightlight))
                {
                    _lastHighlighted.enabled = false;
                }

                _lastHighlighted = hightlight;
                _lastHighlighted.enabled = true;
                return;
            }
        }

        if (_lastHighlighted != null)
        {
            _lastHighlighted.enabled = false;
            _lastHighlighted = null;
        }
    }

    public void ClearHighlight(InputEvent data, EventChainLog log)
    {
        if(_lastHighlighted != null)
        {
            _lastHighlighted.enabled = false;
            _lastHighlighted = null;
        }
    }
}
