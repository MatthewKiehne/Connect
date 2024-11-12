using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum EventType
{
    CreateEntity,
    DestroyEntity,
    SetComponent,
    RemoveComponent,
    TransferEntity,
    DestroyArchetype,
    DestroyWorld,
    ResizeBackingArrays,
}
