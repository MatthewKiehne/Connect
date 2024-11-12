using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class Dragable : MonoBehaviour
{
    public abstract RectTransform GetDragBody(Vector3 mousePosition, Canvas canvas);
}
