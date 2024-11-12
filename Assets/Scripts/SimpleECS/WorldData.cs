using SimpleECS.Internal;
using SimpleECS;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class WorldData
{
    public bool has_remove_callback, has_set_callback, assigned_data;
    public abstract void Set(in Entity entity, in StructureEventHandler handler);
    public abstract void Set(in Entity entity, object component, in StructureEventHandler handler);
    public abstract void Remove(in Entity entity, in StructureEventHandler handler);
    public abstract void InvokeRemoveCallbackAll(in Entity[] entities, in object buffer, int count);
    public abstract void InvokeRemoveCallback(in Entity entity, in object component);

    public abstract object GetData();
    public abstract System.Type data_type { get; }
}
