using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ConnectionDragable : Dragable
{
    [SerializeField]
    private GameObject _UiLinePrefab;

    public override RectTransform GetDragBody(Vector3 mousePosition, Canvas canvas)
    {
        GameObject line = Instantiate(_UiLinePrefab);
        line.transform.SetParent(canvas.transform, false);
        line.transform.SetAsFirstSibling();
        line.transform.position = mousePosition;

        return line.GetComponent<RectTransform>();
    }
}
