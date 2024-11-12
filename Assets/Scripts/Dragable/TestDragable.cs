using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TestDragable : Dragable
{
    public override RectTransform GetDragBody(Vector3 mousePosition, Canvas canvas)
    {
        return GetComponent<RectTransform>();
    }
}
