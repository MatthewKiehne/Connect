using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public struct DragEvent
{
    public Vector3 MousePosition { get; set; }
    public RectTransform DragTransform { get; set; }
    public RectTransform FoundComponentTransform { get; set; }
}
