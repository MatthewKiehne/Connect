using SimpleECS.Internal;
using SimpleECS;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public sealed class WorldDataImp<T> : WorldData
{
    public T data;
    public SetComponentEvent<T> set_callback;
    public SetComponentEventRefOnly<T> set_ref_callback;
    public SetComponentEventCompOnly<T> set_comp_callback;
    public RemoveComponentEvent<T> remove_callback;
    public RemoveComponentEventCompOnly<T> remove_comp_callback;

    public Queue<T> set_queue = new Queue<T>();

    public override void Set(in Entity entity, in StructureEventHandler handler)
    {
        handler.Set(entity, set_queue.Dequeue());
    }

    public override void Set(in Entity entity, object component, in StructureEventHandler handler)
    {
        handler.Set(entity, (T)component);
    }

    public override void Remove(in Entity entity, in StructureEventHandler handler) => handler.Remove<T>(entity);
    public override void InvokeRemoveCallbackAll(in Entity[] entities, in object buffer, int count)
    {
        T[] array = (T[])buffer;
        for (int i = 0; i < count; ++i)
            remove_callback?.Invoke(entities[i], array[i]);
    }

    public override void InvokeRemoveCallback(in Entity entity, in object comp)
        => remove_callback?.Invoke(entity, (T)comp);


    public void CallSetRefCallback(Entity entity, T old_comp, ref T new_comp)
    {
        set_ref_callback.Invoke(entity, ref new_comp);
    }

    public void CallSetCompCallback(Entity entity, T old_comp, ref T new_comp)
    {
        set_comp_callback.Invoke(ref new_comp);
    }

    public void CallRemoveCompCallback(Entity entity, T component)
        => remove_comp_callback.Invoke(component);

    public override object GetData() => data;

    public override System.Type data_type => typeof(T);
}
