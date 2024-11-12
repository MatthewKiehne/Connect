using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TransformDragable : Dragable
{
    public override RectTransform GetDragBody(Vector3 mousePosition, Canvas canvas)
    {
        return GetComponent<RectTransform>();
    }
}
